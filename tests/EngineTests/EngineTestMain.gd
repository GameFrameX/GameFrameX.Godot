# GameFrameX 引擎测试编辑器模式入口（gdUnit 同款 headless -e -s 模式）。
# C# runner 挂到 SceneTree root，由 runner 自行 Quit(exitCode)。
extends SceneTree

func _initialize():
	var runner = load("res://tests/EngineTests/src/EngineTestRunner.cs").new()
	runner.name = "EngineTestRunner"
	root.add_child(runner)
