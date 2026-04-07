@tool
extends VBoxContainer

@export
var cheat_package_scene: PackedScene
@export
var cheat_list: VBoxContainer
@export
var cheat_label: Label
@export
var debug_tool: CanvasLayer

func _ready():
	debug_tool.local.change_language.connect(_on_language_changed)
	_on_language_changed()

func _on_language_changed():
	cheat_label.text = debug_tool.local.get_str("use_dsinspector_add_cheat_button_to_add_cheat_buttons")

func add_cheat_button(title: String, target: Node, method: String):
	add_cheat_button_callable(title, Callable(target, method))

func add_cheat_button_callable(title: String, callable: Callable):
	var item: Control = cheat_package_scene.instantiate();
	var t: Label = item.get_node("Title");
	var b: Button = item.get_node("Button");
	t.text = title;
	b.text = debug_tool.local.get_str("execute")
	b.pressed.connect(callable)
	cheat_list.add_child(item);