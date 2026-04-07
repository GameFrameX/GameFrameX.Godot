@tool
extends VBoxContainer

@export
var search_btn: Button
@export
var clear_search_btn: Button
@export
var search_input: LineEdit
@export
var node_tree: Tree
@export
var search_tree: Tree
@export
var debug_tool: CanvasLayer

var auto_search_enabled: bool = false

func _ready():
	debug_tool.local.change_language.connect(_on_language_changed)
	_on_language_changed()

	search_btn.pressed.connect(_do_serach)
	clear_search_btn.pressed.connect(_do_clear_serach)
	search_input.text_submitted.connect(_do_text_submitted)
	search_input.text_changed.connect(_do_text_changed)
	pass

func _on_language_changed():
	search_btn.text = debug_tool.local.get_str("search")
	clear_search_btn.text = debug_tool.local.get_str("clear")
	search_input.placeholder_text = debug_tool.local.get_str("please_enter_node_name")
	pass


func set_auto_search_enabled(enabled: bool) -> void:
	auto_search_enabled = enabled
	search_btn.visible = !enabled

func _do_text_changed(_new_text: String):
	if auto_search_enabled:
		_do_serach()

func _do_text_submitted(_text: String):
	_do_serach()
	pass

func _do_serach():
	# print("camera" in "camera2d")
	var text: String = search_input.text
	if text.length() == 0:
		node_tree.visible = true
		search_tree.visible = false
	else:
		node_tree.visible = false
		search_tree.visible = true
		text = text.to_lower()
		var arr = _get_search_node_list(text)
		search_tree.set_search_node(arr)
	pass

func _do_clear_serach():
	search_input.text = ""
	_do_serach()
	pass

func _get_search_node_list(text: String) -> Array:
	var arr: Array = []
	for ch in get_tree().root.get_children(true):
		if debug_tool and ch == debug_tool:
			continue
		_each_node(ch, text, arr)
	return arr

func _each_node(node: Node, text: String, arr: Array):
	var n: String = node.name.to_lower()
	if text in n:
		arr.append(node)
	for ch in node.get_children(true):
		_each_node(ch, text, arr)
	pass
