// C1 Spike-W：Web(WASM) 平台 GDExtension 可用性探针。
// 仅验证 side module 动态链接与 ClassDB 注册调用链（不含 Mono——
// WASM 平台无桌面嵌入 API，见 spike 结论）。
#include <godot_cpp/classes/ref_counted.hpp>
#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/variant/string.hpp>

using namespace godot;

class WebProbe : public RefCounted {
    GDCLASS(WebProbe, RefCounted)

protected:
    static void _bind_methods();

public:
    String probe_identity() const {
        return "web gdextension side module alive";
    }

    // 计算密集调用：验证多次跨边界调用稳定性
    int64_t sum_range(int64_t count) const {
        volatile int64_t accumulated = 0;
        for (int64_t index = 0; index < count; ++index) {
            accumulated += index;
        }
        return accumulated;
    }

    WebProbe() = default;
    ~WebProbe() override = default;
};

void WebProbe::_bind_methods() {
    ClassDB::bind_method(D_METHOD("probe_identity"), &WebProbe::probe_identity);
    ClassDB::bind_method(D_METHOD("sum_range", "count"), &WebProbe::sum_range);
}

extern "C" {

GDExtensionBool GDE_EXPORT web_probe_library_init(
        const GDExtensionInterfaceGetProcAddress p_get_proc_address,
        const GDExtensionClassLibraryPtr p_library,
        GDExtensionInitialization *r_initialization) {
    GDExtensionBinding::InitObject init_obj(p_get_proc_address, p_library, r_initialization);
    init_obj.register_initializer([](ModuleInitializationLevel p_level) {
        if (p_level != MODULE_INITIALIZATION_LEVEL_SCENE) return;
        ClassDB::register_class<WebProbe>();
    });
    return init_obj.init();
}

} // extern "C"
