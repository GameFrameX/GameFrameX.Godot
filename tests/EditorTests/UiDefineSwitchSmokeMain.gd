# UI 后端宏快捷切换 headless 冒烟验收入口（同 AsmdefSmokeMain.gd 的 -e -s 模式）。
# C# 驱动挂到 SceneTree root，由驱动自行 Quit(exitCode)。
extends SceneTree

func _initialize():
	var runner = load("res://tests/EditorTests/src/UiDefineSwitchSmokeTest.cs").new()
	runner.name = "UiDefineSwitchSmokeTest"
	root.add_child(runner)
