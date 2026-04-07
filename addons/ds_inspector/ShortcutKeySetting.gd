@tool
extends VBoxContainer

"""
如果关闭了窗口则不监听其他快捷键（只监听隐藏/显示窗口、暂停/播放、单步执行、拣选节点）
可以自己录制快捷键，支持 ctrl、alt、shift、command 组合键
可以设置是否启用快捷键
"""

@export
var debug_tool: CanvasLayer

@export
var shortcut_dialog: ConfirmationDialog

# 使用字典存储快捷键按钮的引用
var _shortcut_buttons: Dictionary = {}

# 弹窗中的标签
var _dialog_label: Label
# 输入控件
var _input_control: Control
# 当前正在录制的快捷键名称
var _current_shortcut_name: String = ""
# 录制的键位数据
var _recorded_keycode: int = 0
var _recorded_ctrl: bool = false
var _recorded_alt: bool = false
var _recorded_shift: bool = false
var _recorded_meta: bool = false

# 快捷键配置数据
const SHORTCUT_CONFIGS = [
	{"key": "toggle_window", "label_key": "shortcut_toggle_window"},
	{"key": "pause_play", "label_key": "shortcut_pause_play"},
	{"key": "step_execute", "label_key": "shortcut_step_execute"},
	{"key": "prev_node", "label_key": "shortcut_prev_node"},
	{"key": "next_node", "label_key": "shortcut_next_node"},
	{"key": "save_node", "label_key": "shortcut_save_node"},
	{"key": "delete_node", "label_key": "shortcut_delete_node"},
	{"key": "pick_node", "label_key": "shortcut_pick_node"},
	{"key": "disable_outline", "label_key": "shortcut_disable_outline"},
	{"key": "collapse_expand", "label_key": "shortcut_collapse_expand"},
	{"key": "focus_search_node", "label_key": "shortcut_focus_search_node"},
	{"key": "focus_search_attr", "label_key": "shortcut_focus_search_attr"},
	{"key": "open_node_scene", "label_key": "shortcut_open_node_scene"},
	{"key": "open_node_script", "label_key": "shortcut_open_node_script"},
	{"key": "toggle_selected_node", "label_key": "shortcut_toggle_selected_node"},
	{"key": "record_node_instance", "label_key": "shortcut_record_node_instance"},
	{"key": "collect_path", "label_key": "shortcut_collect_path"},
	{"key": "exclude_path", "label_key": "shortcut_exclude_path"},
]

# Input Action 名称前缀
const ACTION_PREFIX = "ds_inspector_"

# 快捷键名称到 Action 名称的映射
const SHORTCUT_TO_ACTION = {
	"toggle_window": "ds_inspector_toggle_window",
	"pause_play": "ds_inspector_pause_play",
	"step_execute": "ds_inspector_step_execute",
	"prev_node": "ds_inspector_prev_node",
	"next_node": "ds_inspector_next_node",
	"save_node": "ds_inspector_save_node",
	"delete_node": "ds_inspector_delete_node",
	"pick_node": "ds_inspector_pick_node",
	"collapse_expand": "ds_inspector_collapse_expand",
	"focus_search_node": "ds_inspector_focus_search_node",
	"focus_search_attr": "ds_inspector_focus_search_attr",
	"toggle_selected_node": "ds_inspector_toggle_selected_node",
	"open_node_scene": "ds_inspector_open_node_scene",
	"open_node_script": "ds_inspector_open_node_script",
	"record_node_instance": "ds_inspector_record_node_instance",
	"collect_path": "ds_inspector_collect_path",
	"exclude_path": "ds_inspector_exclude_path",
	"disable_outline": "ds_inspector_disable_outline"
}

func _ready():
	if !debug_tool:
		return
	
	# 动态创建快捷键UI
	_create_shortcut_items()
	
	debug_tool.local.change_language.connect(_on_language_changed)
	_on_language_changed()
	
	# 初始化弹窗
	_setup_dialog()
	
	# 初始化所有 Input Action
	_init_input_actions()
	
	# 加载并显示当前的快捷键
	_load_shortcuts()

func _create_shortcut_items():
	"""动态创建所有快捷键选项"""
	var container = get_node("VBoxContainer2")
	if !container:
		return
	
	# 清空容器中的所有子节点
	for child in container.get_children():
		child.queue_free()
	
	# 动态创建每个快捷键选项
	for config in SHORTCUT_CONFIGS:
		var label_text = debug_tool.local.get_str(config["label_key"])
		_create_shortcut_item(container, config["key"], config["label_key"], label_text)

func _create_shortcut_item(container: VBoxContainer, key: String, label_key: String, label_text: String):
	"""创建单个快捷键选项"""
	var item_container = HBoxContainer.new()
	item_container.size_flags_horizontal = Control.SIZE_FILL
	item_container.set_meta("label_key", label_key)  # 存储翻译键，用于语言切换
	container.add_child(item_container)
	
	# 创建标签
	var label = Label.new()
	label.custom_minimum_size = Vector2(150, 27)
	label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	label.size_flags_vertical = Control.SIZE_SHRINK_CENTER
	label.text = label_text
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	label.name = "Label"
	item_container.add_child(label)
	
	# 创建按钮
	var button = Button.new()
	button.custom_minimum_size = Vector2(200, 0)
	button.text = "未设置"
	button.pressed.connect(_on_shortcut_btn_pressed.bind(key, label_text))
	item_container.add_child(button)
	
	# 存储按钮引用
	_shortcut_buttons[key] = button

func _on_language_changed():
	"""语言切换时更新所有文本"""
	if !debug_tool or !debug_tool.local:
		return
	
	shortcut_dialog.title = debug_tool.local.get_str("record_shortcut_keys")
	shortcut_dialog.ok_button_text = debug_tool.local.get_str("confirm")
	shortcut_dialog.cancel_button_text = debug_tool.local.get_str("cancel")
	
	# 更新标题
	var header = get_node_or_null("HBoxContainer")
	if header:
		var title_label = header.get_node_or_null("Title")
		if title_label:
			title_label.text = debug_tool.local.get_str("enable_keyboard_shortcuts")
	
	# 更新所有快捷键选项的标签文本
	var container = get_node_or_null("VBoxContainer2")
	if container:
		for item in container.get_children():
			if item is HBoxContainer and item.has_meta("label_key"):
				var label_key = item.get_meta("label_key")
				var label = item.get_node_or_null("Label")
				if label:
					label.text = debug_tool.local.get_str(label_key)

func _exit_tree():
	"""节点退出时清理 Input Actions"""
	_cleanup_input_actions()

func _cleanup_input_actions():
	"""清理所有创建的 Input Action"""
	for action_name in SHORTCUT_TO_ACTION.values():
		if InputMap.has_action(action_name):
			InputMap.erase_action(action_name)

func _init_input_actions():
	"""初始化所有 Input Action"""
	if !debug_tool or !debug_tool.save_config:
		return
	
	var shortcut_data = debug_tool.save_config.get_shortcut_key_data()
	
	# 为每个快捷键创建对应的 Action
	_create_or_update_action("toggle_window", shortcut_data.toggle_window)
	_create_or_update_action("pause_play", shortcut_data.pause_play)
	_create_or_update_action("step_execute", shortcut_data.step_execute)
	_create_or_update_action("prev_node", shortcut_data.prev_node)
	_create_or_update_action("next_node", shortcut_data.next_node)
	_create_or_update_action("save_node", shortcut_data.save_node)
	_create_or_update_action("delete_node", shortcut_data.delete_node)
	_create_or_update_action("pick_node", shortcut_data.pick_node)
	_create_or_update_action("collapse_expand", shortcut_data.collapse_expand)
	_create_or_update_action("focus_search_node", shortcut_data.focus_search_node)
	_create_or_update_action("focus_search_attr", shortcut_data.focus_search_attr)
	_create_or_update_action("toggle_selected_node", shortcut_data.toggle_selected_node)
	_create_or_update_action("open_node_scene", shortcut_data.open_node_scene)
	_create_or_update_action("open_node_script", shortcut_data.open_node_script)
	_create_or_update_action("record_node_instance", shortcut_data.record_node_instance)
	_create_or_update_action("collect_path", shortcut_data.collect_path)
	_create_or_update_action("exclude_path", shortcut_data.exclude_path)
	_create_or_update_action("disable_outline", shortcut_data.disable_outline)

func _create_or_update_action(shortcut_name: String, shortcut_dict: Dictionary):
	"""创建或更新一个 Input Action"""
	if shortcut_dict.is_empty():
		return
	
	var action_name = SHORTCUT_TO_ACTION.get(shortcut_name, "")
	if action_name.is_empty():
		return
	
	# 如果 Action 不存在，创建它
	if !InputMap.has_action(action_name):
		InputMap.add_action(action_name)
	else:
		# 清除现有的事件
		InputMap.action_erase_events(action_name)
	
	# 创建键盘事件
	var event = InputEventKey.new()
	event.keycode = shortcut_dict.get("keycode", 0)
	event.ctrl_pressed = shortcut_dict.get("ctrl", false)
	event.alt_pressed = shortcut_dict.get("alt", false)
	event.shift_pressed = shortcut_dict.get("shift", false)
	event.meta_pressed = shortcut_dict.get("meta", false)
	
	# 添加事件到 Action
	InputMap.action_add_event(action_name, event)

func _setup_dialog():
	if !shortcut_dialog:
		return
	
	# 创建一个控件来接收输入
	_input_control = Control.new()
	_input_control.custom_minimum_size = Vector2(280, 80)
	_input_control.focus_mode = Control.FOCUS_ALL
	_input_control.mouse_filter = Control.MOUSE_FILTER_PASS
	
	# 创建标签显示提示信息
	_dialog_label = Label.new()
	_dialog_label.text = debug_tool.local.get_str("please_press_the_shortcut_key")
	_dialog_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_dialog_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	_dialog_label.custom_minimum_size = Vector2(280, 80)
	_dialog_label.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	_dialog_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_input_control.add_child(_dialog_label)
	
	shortcut_dialog.add_child(_input_control)
	
	# 连接输入事件
	_input_control.gui_input.connect(_on_dialog_input)
	
	# 连接确认和取消信号
	shortcut_dialog.confirmed.connect(_on_dialog_confirmed)
	shortcut_dialog.canceled.connect(_on_dialog_canceled)

func _on_shortcut_btn_pressed(shortcut_name: String, _display_name: String):
	_current_shortcut_name = shortcut_name
	_recorded_keycode = 0
	_recorded_ctrl = false
	_recorded_alt = false
	_recorded_shift = false
	_recorded_meta = false
	
	_dialog_label.text = debug_tool.local.get_str("please_press_the_shortcut_key")
	shortcut_dialog.popup_centered()
	# 延迟设置焦点到输入控件，确保弹窗已完全显示
	_input_control.call_deferred("grab_focus")

func _on_dialog_input(event: InputEvent):
	# 监听弹窗中的键盘输入
	if event is InputEventKey and event.pressed:
		# 忽略修饰键本身
		if event.keycode in [KEY_CTRL, KEY_SHIFT, KEY_ALT, KEY_META]:
			return
		
		# 记录键码和修饰键
		_recorded_keycode = event.keycode
		_recorded_ctrl = event.ctrl_pressed
		_recorded_alt = event.alt_pressed
		_recorded_shift = event.shift_pressed
		_recorded_meta = event.meta_pressed
		
		# 显示录制的快捷键
		var shortcut_text = _get_shortcut_text(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		_dialog_label.text = debug_tool.local.get_str("recorded_shortcut_keys") + "\n" + shortcut_text
		
		# 接受事件，防止传递
		_input_control.accept_event()

func _on_dialog_confirmed():
	if _current_shortcut_name.is_empty() or _recorded_keycode == 0:
		return
	
	# 保存快捷键到配置
	var save_config = debug_tool.save_config
	match _current_shortcut_name:
		"toggle_window":
			save_config.set_toggle_window_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"pause_play":
			save_config.set_pause_play_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"step_execute":
			save_config.set_step_execute_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"prev_node":
			save_config.set_prev_node_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"next_node":
			save_config.set_next_node_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"save_node":
			save_config.set_save_node_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"delete_node":
			save_config.set_delete_node_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"pick_node":
			save_config.set_pick_node_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"collapse_expand":
			save_config.set_collapse_expand_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"focus_search_node":
			save_config.set_focus_search_node_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"focus_search_attr":
			save_config.set_focus_search_attr_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"toggle_selected_node":
			save_config.set_toggle_selected_node_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"open_node_scene":
			save_config.set_open_node_scene_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"open_node_script":
			save_config.set_open_node_script_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"record_node_instance":
			save_config.set_record_node_instance_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"collect_path":
			save_config.set_collect_path_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"exclude_path":
			save_config.set_exclude_path_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
		"disable_outline":
			save_config.set_disable_outline_shortcut(_recorded_keycode, _recorded_ctrl, _recorded_alt, _recorded_shift, _recorded_meta)
	
	# 更新对应的 Input Action
	var shortcut_dict = {
		"keycode": _recorded_keycode,
		"ctrl": _recorded_ctrl,
		"alt": _recorded_alt,
		"shift": _recorded_shift,
		"meta": _recorded_meta
	}
	_create_or_update_action(_current_shortcut_name, shortcut_dict)
	
	# 更新按钮显示
	_load_shortcuts()

func _on_dialog_canceled():
	_current_shortcut_name = ""

func _load_shortcuts():
	if !debug_tool or !debug_tool.save_config:
		return
	
	var save_config = debug_tool.save_config
	var shortcut_data = save_config.get_shortcut_key_data()
	
	# 更新所有按钮的文本
	for key in _shortcut_buttons:
		var button = _shortcut_buttons[key]
		var shortcut_dict = shortcut_data.get(key)
		_update_button_text(button, shortcut_dict)

func _update_button_text(button: Button, shortcut_dict: Dictionary):
	if !button or shortcut_dict.is_empty():
		return
	
	var keycode = shortcut_dict.get("keycode", 0)
	var ctrl = shortcut_dict.get("ctrl", false)
	var alt = shortcut_dict.get("alt", false)
	var shift = shortcut_dict.get("shift", false)
	var meta = shortcut_dict.get("meta", false)
	
	button.text = _get_shortcut_text(keycode, ctrl, alt, shift, meta)

func get_action_name(shortcut_name: String) -> String:
	"""获取快捷键对应的 Action 名称"""
	return SHORTCUT_TO_ACTION.get(shortcut_name, "")

func is_shortcut_pressed(shortcut_name: String) -> bool:
	"""检查快捷键是否被按下"""
	var action_name = get_action_name(shortcut_name)
	if action_name.is_empty() or !InputMap.has_action(action_name):
		return false
	return Input.is_action_pressed(action_name)

func is_shortcut_just_pressed(shortcut_name: String) -> bool:
	"""检查快捷键是否刚被按下"""
	var action_name = get_action_name(shortcut_name)
	if action_name.is_empty() or !InputMap.has_action(action_name):
		return false
	return Input.is_action_just_pressed(action_name)

func is_recording() -> bool:
	"""检查是否正在录制快捷键"""
	return shortcut_dialog != null and shortcut_dialog.visible

func _get_shortcut_text(keycode: int, ctrl: bool, alt: bool, shift: bool, meta: bool) -> String:
	var parts: Array[String] = []
	
	if ctrl:
		parts.append("Ctrl")
	if alt:
		parts.append("Alt")
	if shift:
		parts.append("Shift")
	if meta:
		parts.append("Cmd")
	
	# 获取键名
	var key_name = OS.get_keycode_string(keycode)
	
	# 将特殊符号键转换为实际符号显示
	var symbol_map = {
		"Equal": "=",
		"Minus": "-",
		"BracketLeft": "[",
		"BracketRight": "]",
		"BackSlash": "\\",
		"Semicolon": ";",
		"Apostrophe": "'",
		"Comma": ",",
		"Period": ".",
		"Slash": "/"
	}
	
	if symbol_map.has(key_name):
		key_name = symbol_map[key_name]
	
	if !key_name.is_empty():
		parts.append(key_name)
	
	return " + ".join(parts)