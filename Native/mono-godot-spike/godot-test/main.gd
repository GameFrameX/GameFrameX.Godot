extends Node

# C1 Spike-B/C/D 验证：多版本共存、双向交互、运行中热切换、性能基线。
# 热切换 demo：_process 帧驱动 Tick，第 HOT_SWAP_FRAME 帧把激活槽位从 v1 切到 v2。

const HOT_SWAP_FRAME := 4
const TOTAL_FRAMES := 6
const BENCHMARK_ITERATIONS := 200000
const SCRIPT_CALL_ITERATIONS := 10000

var failed_checks: int = 0
var spike: RefCounted
var slot_one: int = -1
var slot_two: int = -1
var active_slot: int = -1
var frame_index: int = 0
var build_directory: String = ""
var tick_history: Array[String] = []

func _check(label: String, actual, expected) -> void:
	var passed := str(actual) == str(expected)
	if not passed:
		failed_checks += 1
	print("SPIKE CHECK: ", label, " -> ", actual, " (expect ", expected, ") ", "PASS" if passed else "FAIL")

func _resident_size_kilobytes() -> int:
	var output: Array = []
	OS.execute("ps", ["-o", "rss=", "-p", str(OS.get_process_id())], output)
	return int(str(output[0]).strip_edges())
func _contains(label: String, actual: String, marker: String) -> void:
	var passed := actual.contains(marker)
	if not passed:
		failed_checks += 1
	print("SPIKE CHECK: ", label, " -> ", actual, " (contains ", marker, ") ", "PASS" if passed else "FAIL")

func _ready() -> void:
	if not ClassDB.class_exists("MonoSpike"):
		print("SPIKE RESULT: FAIL (MonoSpike 未注册，扩展加载失败)")
		get_tree().quit(1)
		return
	if OS.get_environment("SPIKE_PROBE_UNLOAD") == "1":
		# 隔离运行：门面若禁用 create/unload 会直接 abort 本进程，由外层观察存活
		var probe_spike = ClassDB.instantiate("MonoSpike")
		probe_spike.load_assembly(ProjectSettings.globalize_path("res://addons/../../build") + "/ManagedSpikeV1.dll")
		print("SPIKE PROBE-UNLOAD: ", probe_spike.probe_domain_unload())
		get_tree().quit(0)
		return
	spike = ClassDB.instantiate("MonoSpike")
	# res://addons/../../build = spike 产物目录（gdextension 同款相对定位）
	build_directory = ProjectSettings.globalize_path("res://addons/../../build")
	slot_one = spike.load_assembly(build_directory + "/ManagedSpikeV1.dll")
	slot_two = spike.load_assembly(build_directory + "/ManagedSpikeV2.dll")
	_check("槽位索引", [slot_one, slot_two], [0, 1])
	active_slot = slot_one

func _process(_delta: float) -> void:
	frame_index += 1
	if frame_index == HOT_SWAP_FRAME:
		active_slot = slot_two
		print("SPIKE HOT-SWAP: frame ", frame_index, " 激活槽位切到 v2")
	tick_history.append(spike.call_tick(active_slot, frame_index))
	if frame_index < TOTAL_FRAMES:
		return

	# ---- 帧循环热切换断言 ----
	for entry in tick_history:
		print("SPIKE TICK HISTORY: ", entry)
	_contains("热切换前 v1 驱动（frame 1）", tick_history[0], "logic version one tick 1 at frame 1")
	_contains("切换前最后 v1 帧（frame 3）", tick_history[2], "logic version one tick 3 at frame 3")
	_contains("切换当帧已走 v2（frame 4）", tick_history[3], "logic version two tick 1 at frame 4")
	_contains("切换后 v2 计数独立（frame 6）", tick_history[5], "logic version two tick 3 at frame 6")

	# ---- Spike-B/C 静态能力断言 ----
	_contains("hello v1", spike.call_hello(slot_one), "version one")
	_check("add v2 参数边界", spike.call_add(slot_two, 2, 3), 5)
	var sample_values: PackedInt32Array = [10, 20, 30, 40]
	_check("数组求和（v2）", spike.call_sum(slot_two, sample_values), 100)
	_check("InternalCall 反向调用（v2）", spike.call_multiply_through_native(slot_two, 6, 7), 42)
	# ---- Spike-E：结构体/double/缓存/内存 ----
	var moved: Vector3 = spike.call_move_point(slot_two, Vector3(1.0, 2.0, 3.0), 0.5)
	_check("结构体按值往返（v2）", [moved.x, moved.y, moved.z], [1.5, 2.5, 3.5])
	_check("double 跨边界（v2）", spike.call_echo_double(slot_two, 2.25), 4.5)

	var reload_slot: int = spike.load_assembly(build_directory + "/ManagedSpikeV1.dll")
	var address_first: int = spike.get_assembly_address(slot_one)
	var address_second: int = spike.get_assembly_address(reload_slot)
	_check("同程序集重复加载命中缓存（指针相同）", address_first == address_second and address_first != 0, true)

	var resident_before_kilobytes: int = _resident_size_kilobytes()
	var memory_slots: Array[int] = []
	for series_number in ["01", "02", "03", "04", "05"]:
		memory_slots.append(spike.load_assembly(
			build_directory + "/memory-series/R" + series_number + "/ManagedSpikeR" + series_number + ".dll"))
	var resident_after_kilobytes: int = _resident_size_kilobytes()
	_check("内存序列槽位全部加载成功", memory_slots, [3, 4, 5, 6, 7])
	var growth_per_reload_kilobytes: float = (resident_after_kilobytes - resident_before_kilobytes) / 5.0
	print("SPIKE MEMORY: RSS ", resident_before_kilobytes, " KB -> ", resident_after_kilobytes,
		" KB，每版本驻留约 ", growth_per_reload_kilobytes, " KB（含 JIT/元数据，不可回收部分待长期评估）")
	_contains("实例对象流程（v2）", spike.call_instance_flow(slot_two, "spike-instance", 5, 37), "accumulated 42")
	_contains("异常回传", spike.call_throw_if(slot_two, true), "MANAGED_EXCEPTION: InvalidOperationException")

	# ---- 性能基线 ----
	var invoke_microseconds: int = spike.benchmark_invoke(slot_two, BENCHMARK_ITERATIONS)
	var native_loop_microseconds: int = spike.benchmark_native_loop(BENCHMARK_ITERATIONS)
	var script_start_usec: int = Time.get_ticks_usec()
	for index in range(SCRIPT_CALL_ITERATIONS):
		spike.call_add(slot_two, 1, 2)
	var script_roundtrip_microseconds: int = Time.get_ticks_usec() - script_start_usec
	var per_invoke_nanoseconds: float = invoke_microseconds * 1000.0 / BENCHMARK_ITERATIONS
	var per_native_loop_nanoseconds: float = native_loop_microseconds * 1000.0 / BENCHMARK_ITERATIONS
	var per_script_call_nanoseconds: float = script_roundtrip_microseconds * 1000.0 / SCRIPT_CALL_ITERATIONS
	print("SPIKE BENCHMARK: mono invoke 边界开销 = ", per_invoke_nanoseconds, " ns/次 (", BENCHMARK_ITERATIONS, " 次 ", invoke_microseconds, " us)")
	print("SPIKE BENCHMARK: native 空转循环参照 = ", per_native_loop_nanoseconds, " ns/次")
	print("SPIKE BENCHMARK: GDScript->native->mono 往返 = ", per_script_call_nanoseconds, " ns/次 (", SCRIPT_CALL_ITERATIONS, " 次 ", script_roundtrip_microseconds, " us)")
	print("runtime ready: ", spike.is_runtime_ready())
	print("loaded assembly count: ", spike.loaded_assembly_count())
	print("SPIKE RESULT: ", "PASS" if failed_checks == 0 else "FAIL (" + str(failed_checks) + " checks failed)")
	get_tree().quit(0 if failed_checks == 0 else 1)
