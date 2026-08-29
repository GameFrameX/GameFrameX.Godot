# G4 Asmdef 属性编辑器 headless 冒烟验收入口（同 EngineTestMain.gd 的 -e -s 模式）。
# C# 驱动挂到 SceneTree root，由驱动自行 Quit(exitCode)。
extends SceneTree

func _initialize():
	var runner = load("res://tests/EditorTests/src/AsmdefEditorSmokeTest.cs").new()
	runner.name = "AsmdefEditorSmokeTest"
	root.add_child(runner)
