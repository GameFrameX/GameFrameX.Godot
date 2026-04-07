@tool
extends HBoxContainer

@export
var node_btn: Button

@export
var del_btn: Button

var debug_tool: CanvasLayer

# 信号：请求删除此记录项
signal delete_requested

# 存储节点信息
var node_ref: WeakRef = null
var node_tree = null  # NodeTree 引用
var time_accumulator: float = 0.0  # 时间累积器，用于每秒更新路径

func _ready():
	# 连接按钮信号
	if node_btn:
		node_btn.pressed.connect(_on_node_button_pressed)
		# 在Button上也启用拖放转发，让拖放事件能够传递到父容器
		node_btn.set_drag_forwarding(Callable(), _can_drop_data_fw, _drop_data_fw)
	if del_btn:
		del_btn.pressed.connect(_on_delete_button_pressed)
	
	del_btn.tooltip_text = debug_tool.local.get_str("delete")
	
	# 启用拖放转发，让拖放事件能够传递到父容器
	set_drag_forwarding(Callable(), _can_drop_data_fw, _drop_data_fw)

func _process(delta: float):
	# 累积时间，每秒更新一次路径
	time_accumulator += delta
	if time_accumulator >= 1.0:
		time_accumulator = 0.0
		_update_node_path()

# 设置节点信息
func setup_node(node: Node, tree) -> void:
	if !node or !is_instance_valid(node):
		return
	
	# 保存节点引用
	node_ref = weakref(node)
	node_tree = tree
	
	# 设置图标
	var icon_path = node_tree.icon_mapping.get_icon(node)
	if icon_path and icon_path != "":
		node_btn.icon = load(icon_path)

	# 更新节点路径显示
	_update_node_path()

# 更新节点路径显示
func _update_node_path() -> void:
	if !node_ref:
		return
	
	var node = node_ref.get_ref()
	if node == null or !is_instance_valid(node):
		# 节点已失效，删除此记录项
		delete_requested.emit()
		return
	
	# 更新按钮文本和提示信息
	if node_btn:
		# 名称 + （路径）
		node_btn.text = node.name + " (" + str(node.get_path()) + ")"
		
		# 设置提示信息（显示节点路径）
		node_btn.tooltip_text = debug_tool.local.get_str_replace1("click_to_locate", node.get_path())

# 点击节点按钮时
func _on_node_button_pressed() -> void:
	if !node_ref:
		return
	
	var node = node_ref.get_ref()
	if node == null or !is_instance_valid(node):
		# 节点已失效，删除此记录项
		delete_requested.emit()
		return
	
	# 定位到节点树中的节点
	if node_tree and node_tree.has_method("locate_selected"):
		node_tree.call_deferred("locate_selected", node)

# 点击删除按钮时
func _on_delete_button_pressed() -> void:
	delete_requested.emit()

# 判断是否可以接收拖放的数据（转发给父容器RecordContainer）
func _can_drop_data_fw(_position: Vector2, drag_data: Variant) -> bool:
	# 获取RecordContainer（RecordNodeItem的父容器）
	var parent = get_parent()
	if parent and parent.has_method("_can_drop_data_fw"):
		# 将位置转换为父容器的本地坐标
		# position是相对于当前控件的本地坐标，转换为相对于父容器的坐标
		var parent_local_pos = _position + get_position()
		return parent._can_drop_data_fw(parent_local_pos, drag_data)
	return false

# 执行拖放操作（转发给父容器RecordContainer）
func _drop_data_fw(_position: Vector2, drag_data: Variant) -> void:
	# 获取RecordContainer（RecordNodeItem的父容器）
	var parent = get_parent()
	if parent and parent.has_method("_drop_data_fw"):
		# 将位置转换为父容器的本地坐标
		# position是相对于当前控件的本地坐标，转换为相对于父容器的坐标
		var parent_local_pos = _position + get_position()
		parent._drop_data_fw(parent_local_pos, drag_data)