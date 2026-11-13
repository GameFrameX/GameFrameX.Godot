// C1 Spike-B：GDExtension 内嵌 dotnet/runtime Mono（libcoreclr.dylib = Mono sgen），
// C1 Spike-C 增补：数组 marshalling、实例对象生命周期、InternalCall 反向调用。
// 过程验证代码，非生产实现。
#include <godot_cpp/classes/ref_counted.hpp>
#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/variant/packed_int32_array.hpp>
#include <godot_cpp/classes/engine.hpp>
#include <godot_cpp/variant/vector3.hpp>
#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/mono-config.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/class.h>
#include <mono/metadata/object.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/loader.h>

#include <vector>
#include <chrono>

#include <dlfcn.h>
#include <unistd.h>
#include <limits.h>

using namespace godot;

namespace {

MonoDomain *g_domain = nullptr;
char g_self_dir[PATH_MAX] = {0};

// InternalCall 实现体：托管经 NativeMultiply 声明反向调用到这里
int32_t native_multiply(int32_t left_value, int32_t right_value) {
    return left_value * right_value;
}

void resolve_self_dir() {
    // dylib 自身目录：用于定位同目录复制的 Mono 运行时、BCL 与托管程序集
    Dl_info info{};
    if (dladdr(reinterpret_cast<void *>(&resolve_self_dir), &info) && info.dli_fname) {
        char resolved[PATH_MAX] = {0};
        realpath(info.dli_fname, resolved);
        char *slash = strrchr(resolved, '/');
        if (slash) { *slash = 0; strncpy(g_self_dir, resolved, sizeof(g_self_dir) - 1); }
    }
}

String exception_to_string(MonoObject *exception) {
    // 注意：Exception.ToString() 在本嵌入式构建（libcoreclr 门面）会于 System.SR
    // 资源字符串类构造处崩溃，故直读 System.Exception._message 字段（沿父类链），
    // 不触发任何托管格式化代码。
    MonoClass *exception_class = mono_object_get_class(exception);
    String class_name(mono_class_get_name(exception_class));
    MonoString *message = nullptr;
    for (MonoClass *klass = exception_class; klass != nullptr && message == nullptr; klass = mono_class_get_parent(klass)) {
        MonoClassField *message_field = mono_class_get_field_from_name(klass, "_message");
        if (message_field) {
            mono_field_get_value(exception, message_field, &message);
        }
    }
    return String("MANAGED_EXCEPTION: ") + class_name + ": " +
           String(message ? mono_string_to_utf8(message) : "");
}

bool init_mono(String &error) {
    if (g_domain) return true;
    resolve_self_dir();

    std::string directory(g_self_dir);
    // BCL（System.Private.CoreLib.dll 等）与托管程序集均在 dylib 同目录
    mono_set_dirs(directory.c_str(), directory.c_str());
    mono_set_assemblies_path(directory.c_str());

    g_domain = mono_jit_init_version("mono-godot-spike", "v4.0.30319");
    if (!g_domain) { error = "mono_jit_init 失败"; return false; }
    // 注册 InternalCall：托管 Spike.NativeMultiply 由 native_multiply 实现
    mono_add_internal_call("ManagedSpike.Spike::NativeMultiply",
                           reinterpret_cast<const void *>(&native_multiply));
    return true;
}

// 规范化为绝对路径：mono_domain_assembly_open 需要可解析的文件系统路径
std::string normalize_path(const String &path) {
    CharString utf8 = path.utf8();
    char resolved[PATH_MAX] = {0};
    if (realpath(utf8.get_data(), resolved) && resolved[0] != '\0') {
        return std::string(resolved);
    }
    return std::string(utf8.get_data());
}

MonoMethod *find_method(MonoImage *image, const char *method_description) {
    MonoMethodDesc *desc = mono_method_desc_new(method_description, /*include_namespace=*/1);
    MonoMethod *method = mono_method_desc_search_in_image(desc, image);
    mono_method_desc_free(desc);
    return method;
}

// 统一把 invoke 结果（string 或异常对象）转成 Godot String
String invoke_result_to_string(MonoObject *result, MonoObject *exception) {
    if (exception) {
        return exception_to_string(exception);
    }
    if (!result) return "NULL_RESULT";
    MonoString *text = mono_object_to_string(result, nullptr);
    return String(mono_string_to_utf8(text));
}

} // namespace

class MonoSpike : public RefCounted {
    GDCLASS(MonoSpike, RefCounted)

private:
    // 每个槽位对应一次独立加载的程序集版本，各自持有独立的方法句柄与静态状态
    struct LoadedAssembly {
        MonoAssembly *assembly = nullptr;
        MonoImage *image = nullptr;
        MonoMethod *hello_method = nullptr;
        MonoMethod *increment_method = nullptr;
        MonoMethod *add_method = nullptr;
        MonoMethod *throw_if_method = nullptr;
        MonoMethod *sum_values_method = nullptr;
        MonoMethod *multiply_method = nullptr;
        MonoClass *instance_class = nullptr;
        MonoMethod *tick_method = nullptr;
        MonoClass *spike_class = nullptr;
        MonoMethod *move_point_method = nullptr;
        MonoMethod *echo_double_method = nullptr;
        MonoMethod *instance_constructor = nullptr;
        MonoMethod *instance_add_value_method = nullptr;
        MonoMethod *instance_describe_method = nullptr;
    };
    std::vector<LoadedAssembly> loaded_assemblies;

    const LoadedAssembly *assembly_at(int slot, String &error) {
        if (slot < 0 || slot >= static_cast<int>(loaded_assemblies.size())) {
            error = String("SLOT_OUT_OF_RANGE: ") + String::num(slot);
            return nullptr;
        }
        return &loaded_assemblies[slot];
    }

protected:
    static void _bind_methods();

public:
    // 加载托管程序集，返回槽位索引；失败返回 -1
    int load_assembly(const String &path) {
        String error;
        if (!init_mono(error)) {
            UtilityFunctions::printerr("SPIKE init mono 失败: ", error);
            return -1;
        }
        const std::string full_path = normalize_path(path);
        MonoAssembly *assembly = mono_domain_assembly_open(g_domain, full_path.c_str());
        if (!assembly) {
            UtilityFunctions::printerr("SPIKE 程序集打开失败: ", String(full_path.c_str()));
            return -1;
        }
        MonoImage *image = mono_assembly_get_image(assembly);

        LoadedAssembly entry;
        entry.assembly = assembly;
        entry.image = image;
        entry.hello_method = find_method(image, "ManagedSpike.Spike:Hello()");
        entry.increment_method = find_method(image, "ManagedSpike.Spike:Increment()");
        entry.add_method = find_method(image, "ManagedSpike.Spike:Add(int,int)");
        entry.throw_if_method = find_method(image, "ManagedSpike.Spike:ThrowIf(bool)");
        entry.sum_values_method = find_method(image, "ManagedSpike.Spike:SumValues(int[])");
        entry.tick_method = find_method(image, "ManagedSpike.Spike:Tick(int)");
        entry.multiply_method = find_method(image, "ManagedSpike.Spike:MultiplyThroughNative(int,int)");
        entry.instance_class = mono_class_from_name(image, "ManagedSpike", "SpikeInstance");
        if (entry.instance_class) {
            entry.instance_constructor = mono_class_get_method_from_name(entry.instance_class, ".ctor", 1);
            entry.instance_add_value_method = mono_class_get_method_from_name(entry.instance_class, "AddValue", 1);
            entry.instance_describe_method = mono_class_get_method_from_name(entry.instance_class, "Describe", 0);
        }
        entry.spike_class = mono_class_from_name(image, "ManagedSpike", "Spike");
        if (entry.spike_class) {
            entry.move_point_method = mono_class_get_method_from_name(entry.spike_class, "MovePoint", 2);
            entry.echo_double_method = mono_class_get_method_from_name(entry.spike_class, "EchoDouble", 1);
        }
        if (!entry.hello_method || !entry.increment_method || !entry.add_method || !entry.throw_if_method ||
            !entry.sum_values_method || !entry.multiply_method || !entry.instance_class || !entry.tick_method ||
            !entry.move_point_method || !entry.echo_double_method ||
            !entry.instance_constructor || !entry.instance_add_value_method || !entry.instance_describe_method) {
            UtilityFunctions::printerr("SPIKE 方法查找失败: ", String(full_path.c_str()));
            return -1;
        }
        loaded_assemblies.push_back(entry);
        return static_cast<int>(loaded_assemblies.size()) - 1;
    }

    String call_hello(int slot) {
        String error;
        const LoadedAssembly *entry = assembly_at(slot, error);
        if (!entry) return error;
        MonoObject *exception = nullptr;
        MonoObject *result = mono_runtime_invoke(entry->hello_method, nullptr, nullptr, &exception);
        return invoke_result_to_string(result, exception);
    }

    int call_increment(int slot) {
        String error;
        const LoadedAssembly *entry = assembly_at(slot, error);
        if (!entry) { UtilityFunctions::printerr(error); return -1; }
        MonoObject *exception = nullptr;
        MonoObject *result = mono_runtime_invoke(entry->increment_method, nullptr, nullptr, &exception);
        if (exception) {
            UtilityFunctions::printerr(invoke_result_to_string(nullptr, exception));
            return -1;
        }
        return result ? *static_cast<int *>(mono_object_unbox(result)) : -1;
    }

    int call_add(int slot, int left_value, int right_value) {
        String error;
        const LoadedAssembly *entry = assembly_at(slot, error);
        if (!entry) { UtilityFunctions::printerr(error); return -1; }
        // 值类型参数按地址传给 mono_runtime_invoke，须存活到调用返回
        void *arguments[] = { &left_value, &right_value };
        MonoObject *exception = nullptr;
        MonoObject *result = mono_runtime_invoke(entry->add_method, nullptr, arguments, &exception);
        if (exception) {
            UtilityFunctions::printerr(invoke_result_to_string(nullptr, exception));
            return -1;
        }
        return result ? *static_cast<int *>(mono_object_unbox(result)) : -1;
    }

    String call_throw_if(int slot, bool should_throw) {
        String error;
        const LoadedAssembly *entry = assembly_at(slot, error);
        if (!entry) return error;
        // bool 在 Mono 嵌入 ABI 中按 32 位 int 装箱传递
        int flag_value = should_throw ? 1 : 0;
        void *arguments[] = { &flag_value };
        MonoObject *exception = nullptr;
        MonoObject *result = mono_runtime_invoke(entry->throw_if_method, nullptr, arguments, &exception);
        return invoke_result_to_string(result, exception);
    }

    // 数组跨边界：native 构造 int[] 传入托管求和
    int call_sum(int slot, const PackedInt32Array &values) {
        String error;
        const LoadedAssembly *entry = assembly_at(slot, error);
        if (!entry) { UtilityFunctions::printerr(error); return -1; }
        const int element_count = static_cast<int>(values.size());
        MonoArray *managed_array = mono_array_new(g_domain, mono_get_int32_class(),
                                                  static_cast<uintptr_t>(element_count));
        if (!managed_array) { UtilityFunctions::printerr("SPIKE mono_array_new 失败"); return -1; }
        for (int index = 0; index < element_count; ++index) {
            int32_t *slot_pointer = reinterpret_cast<int32_t *>(
                    mono_array_addr_with_size(managed_array, sizeof(int32_t),
                                              static_cast<uintptr_t>(index)));
            *slot_pointer = values[index];
        }
        // 引用类型参数：mono_runtime_invoke 参数表直接放对象指针
        void *arguments[] = { managed_array };
        MonoObject *exception = nullptr;
        MonoObject *result = mono_runtime_invoke(entry->sum_values_method, nullptr, arguments, &exception);
        if (exception) {
            UtilityFunctions::printerr(invoke_result_to_string(nullptr, exception));
            return -1;
        }
        return result ? *static_cast<int *>(mono_object_unbox(result)) : -1;
    }

    // InternalCall 反向调用：托管方法内部回调 native_multiply
    int call_multiply_through_native(int slot, int left_value, int right_value) {
        String error;
        const LoadedAssembly *entry = assembly_at(slot, error);
        if (!entry) { UtilityFunctions::printerr(error); return -1; }
        void *arguments[] = { &left_value, &right_value };
        MonoObject *exception = nullptr;
        MonoObject *result = mono_runtime_invoke(entry->multiply_method, nullptr, arguments, &exception);
        if (exception) {
            UtilityFunctions::printerr(invoke_result_to_string(nullptr, exception));
            return -1;
        }
        return result ? *static_cast<int *>(mono_object_unbox(result)) : -1;
    }

    // 实例对象生命周期：构造 → 多次驱动实例方法 → 读取实例状态
    String call_instance_flow(int slot, const String &identity, int first_amount, int second_amount) {
        String error;
        const LoadedAssembly *entry = assembly_at(slot, error);
        if (!entry) return error;

        MonoObject *instance = mono_object_new(g_domain, entry->instance_class);
        if (!instance) return "INSTANCE_ALLOC_FAIL";
        MonoString *identity_string = mono_string_new(g_domain, identity.utf8().get_data());
        void *ctor_arguments[] = { identity_string };
        MonoObject *exception = nullptr;
        mono_runtime_invoke(entry->instance_constructor, instance, ctor_arguments, &exception);
        if (exception) return invoke_result_to_string(nullptr, exception);

        for (int amount : { first_amount, second_amount }) {
            int amount_value = amount;
            void *add_arguments[] = { &amount_value };
            mono_runtime_invoke(entry->instance_add_value_method, instance, add_arguments, &exception);
            if (exception) return invoke_result_to_string(nullptr, exception);
        }

        MonoObject *description = mono_runtime_invoke(entry->instance_describe_method, instance, nullptr, &exception);
        return invoke_result_to_string(description, exception);
    }

    // 热更循环帧驱动：调用当前激活版本程序集的 Tick，返回其标记输出
    String call_tick(int slot, int frame_index) {
        String error;
        const LoadedAssembly *entry = assembly_at(slot, error);
        if (!entry) return error;
        int frame_value = frame_index;
        void *arguments[] = { &frame_value };
        MonoObject *exception = nullptr;
        MonoObject *result = mono_runtime_invoke(entry->tick_method, nullptr, arguments, &exception);
        return invoke_result_to_string(result, exception);
    }

    // 性能基线：native 循环 invoke 托管 Add，返回总耗时微秒
    int64_t benchmark_invoke(int slot, int64_t iterations) {
        String error;
        const LoadedAssembly *entry = assembly_at(slot, error);
        if (!entry) { UtilityFunctions::printerr(error); return -1; }
        int left_value = 2;
        int right_value = 3;
        void *arguments[] = { &left_value, &right_value };
        MonoObject *exception = nullptr;
        const auto start_time = std::chrono::steady_clock::now();
        for (int64_t index = 0; index < iterations; ++index) {
            MonoObject *result = mono_runtime_invoke(entry->add_method, nullptr, arguments, &exception);
            if (exception || !result) { UtilityFunctions::printerr("SPIKE benchmark 中途异常"); return -1; }
        }
        const auto end_time = std::chrono::steady_clock::now();
        return std::chrono::duration_cast<std::chrono::microseconds>(end_time - start_time).count();
    }

    // 性能基线参照：等量纯 native 空转循环，返回总耗时微秒
    int64_t benchmark_native_loop(int64_t iterations) {
        volatile int64_t accumulated_value = 0;
        const auto start_time = std::chrono::steady_clock::now();
        for (int64_t index = 0; index < iterations; ++index) {
            accumulated_value = accumulated_value + index;
        }
        const auto end_time = std::chrono::steady_clock::now();
        return std::chrono::duration_cast<std::chrono::microseconds>(end_time - start_time).count();
    }

    // 与托管 ManagedSpike.Spike.NativeVector3 布局一致的 blittable 结构体
    struct NativeVector3Layout {
        float x;
        float y;
        float z;
    };

    // 结构体按值跨边界：native 组栈结构传入，托管返回值 unbox 后拆回 Vector3
    Vector3 call_move_point(int slot, const Vector3 &point, float delta) {
        String error;
        const LoadedAssembly *entry = assembly_at(slot, error);
        if (!entry) return Vector3();
        NativeVector3Layout point_value = { point.x, point.y, point.z };
        float delta_value = delta;
        void *arguments[] = { &point_value, &delta_value };
        MonoObject *exception = nullptr;
        MonoObject *result = mono_runtime_invoke(entry->move_point_method, nullptr, arguments, &exception);
        if (exception) {
            UtilityFunctions::printerr(invoke_result_to_string(nullptr, exception));
            return Vector3();
        }
        if (!result) return Vector3();
        const NativeVector3Layout *moved = static_cast<const NativeVector3Layout *>(mono_object_unbox(result));
        return Vector3(moved->x, moved->y, moved->z);
    }

    double call_echo_double(int slot, double value) {
        String error;
        const LoadedAssembly *entry = assembly_at(slot, error);
        if (!entry) { UtilityFunctions::printerr(error); return -1.0; }
        void *arguments[] = { &value };
        MonoObject *exception = nullptr;
        MonoObject *result = mono_runtime_invoke(entry->echo_double_method, nullptr, arguments, &exception);
        if (exception) {
            UtilityFunctions::printerr(invoke_result_to_string(nullptr, exception));
            return -1.0;
        }
        return result ? *static_cast<double *>(mono_object_unbox(result)) : -1.0;
    }

    // 重复加载同程序集的缓存行为探针：返回 MonoAssembly 指针数值供比较
    int64_t get_assembly_address(int slot) const {
        String error;
        const LoadedAssembly *entry = const_cast<MonoSpike *>(this)->assembly_at(slot, error);
        return entry ? reinterpret_cast<int64_t>(entry->assembly) : 0;
    }

    // AppDomain 卸载可用性探测（隔离运行）：若门面禁用会直接 abort 进程，
    // 存活并返回本字符串即代表 create/unload 路径可用。
    String probe_domain_unload() {
        if (!g_domain) return "RUNTIME_NOT_READY";
        MonoDomain *probe_domain = mono_domain_create_appdomain("mono-spike-unload-probe", nullptr);
        if (!probe_domain) return "CREATE_APPDOMAIN_FAIL";
        mono_domain_unload(probe_domain);
        return "UNLOAD_RETURNED_WITHOUT_ABORT";
    }

    bool is_runtime_ready() const { return g_domain != nullptr; }

    int loaded_assembly_count() const { return static_cast<int>(loaded_assemblies.size()); }

    MonoSpike() = default;
    ~MonoSpike() override = default;
};

void MonoSpike::_bind_methods() {
    ClassDB::bind_method(D_METHOD("load_assembly", "path"), &MonoSpike::load_assembly);
    ClassDB::bind_method(D_METHOD("call_hello", "slot"), &MonoSpike::call_hello);
    ClassDB::bind_method(D_METHOD("call_increment", "slot"), &MonoSpike::call_increment);
    ClassDB::bind_method(D_METHOD("call_add", "slot", "left_value", "right_value"), &MonoSpike::call_add);
    ClassDB::bind_method(D_METHOD("call_throw_if", "slot", "should_throw"), &MonoSpike::call_throw_if);
    ClassDB::bind_method(D_METHOD("is_runtime_ready"), &MonoSpike::is_runtime_ready);
    ClassDB::bind_method(D_METHOD("loaded_assembly_count"), &MonoSpike::loaded_assembly_count);
    ClassDB::bind_method(D_METHOD("call_sum", "slot", "values"), &MonoSpike::call_sum);
    ClassDB::bind_method(D_METHOD("call_multiply_through_native", "slot", "left_value", "right_value"), &MonoSpike::call_multiply_through_native);
    ClassDB::bind_method(D_METHOD("call_instance_flow", "slot", "identity", "first_amount", "second_amount"), &MonoSpike::call_instance_flow);
    ClassDB::bind_method(D_METHOD("call_tick", "slot", "frame_index"), &MonoSpike::call_tick);
    ClassDB::bind_method(D_METHOD("benchmark_invoke", "slot", "iterations"), &MonoSpike::benchmark_invoke);
    ClassDB::bind_method(D_METHOD("benchmark_native_loop", "iterations"), &MonoSpike::benchmark_native_loop);
    ClassDB::bind_method(D_METHOD("call_move_point", "slot", "point", "delta"), &MonoSpike::call_move_point);
    ClassDB::bind_method(D_METHOD("call_echo_double", "slot", "value"), &MonoSpike::call_echo_double);
    ClassDB::bind_method(D_METHOD("get_assembly_address", "slot"), &MonoSpike::get_assembly_address);
    ClassDB::bind_method(D_METHOD("probe_domain_unload"), &MonoSpike::probe_domain_unload);
}
extern "C" {

// GDExtension 入口：注册 MonoSpike 到 ClassDB，供 Godot 脚本实例化调用
GDExtensionBool GDE_EXPORT mono_spike_library_init(
        const GDExtensionInterfaceGetProcAddress p_get_proc_address,
        const GDExtensionClassLibraryPtr p_library,
        GDExtensionInitialization *r_initialization) {
    GDExtensionBinding::InitObject init_obj(p_get_proc_address, p_library, r_initialization);
    init_obj.register_initializer([](ModuleInitializationLevel p_level) {
        if (p_level != MODULE_INITIALIZATION_LEVEL_SCENE) return;
        ClassDB::register_class<MonoSpike>();
    });
    return init_obj.init();
}

} // extern "C"
