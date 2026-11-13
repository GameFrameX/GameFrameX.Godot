extends Node

# C1 Spike-W：Web(WASM) GDExtension side module 加载与调用链验证。

func _ready() -> void:
	if not ClassDB.class_exists("WebProbe"):
		print("SPIKE-W RESULT: FAIL (WebProbe 未注册，side module 加载失败)")
		return
	var probe = ClassDB.instantiate("WebProbe")
	var identity: String = probe.probe_identity()
	var checksum: int = probe.sum_range(1000)
	var passed: bool = identity == "web gdextension side module alive" and checksum == 499500
	print("SPIKE-W CHECK: identity -> ", identity)
	print("SPIKE-W CHECK: sum_range(1000) -> ", checksum, " (expect 499500)")
	print("SPIKE-W RESULT: ", "PASS" if passed else "FAIL")
