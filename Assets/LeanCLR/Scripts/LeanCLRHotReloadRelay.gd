extends Node
## LeanCLR 热更新中继：轮询 marker 文件，驱动 LeanCLRHotReloadHost 交换程序集。
## 生产模式下 marker/程序集由下载管线写入（user:// 或 res:// 均可读）。

@export var marker_path := "res://Assets/LeanCLR/live_reload.txt"
@export var attached_assembly_name := "LeanCLRHotfix"
@export var reload_type_name := "GameFrameX.LeanCLR.HotfixDemo"
@export var hot_reload_host_path: NodePath
@export var demo_path: NodePath
@export var reload_poll_seconds := 0.25

@onready var hot_reload_host: Node = get_node_or_null(hot_reload_host_path)

var _elapsed := 0.0

func _ready() -> void:
	if hot_reload_host == null:
		push_error("[LeanCLRRelay] hot_reload_host node not found: " + str(hot_reload_host_path))
		set_process(false)
		return
	# 状态迁移来源 = 挂 .lcs 脚本的 Demo 节点上的托管对象。
	hot_reload_host.set_script_owner_path(demo_path)
	reload_from_marker()
	set_process(reload_poll_seconds > 0.0)

func _process(delta: float) -> void:
	_elapsed += delta
	if _elapsed >= reload_poll_seconds:
		_elapsed = 0.0
		reload_from_marker()

func reload_from_marker() -> void:
	if hot_reload_host == null:
		return

	var assembly_name := attached_assembly_name
	if marker_path != "" and FileAccess.file_exists(marker_path):
		assembly_name = FileAccess.get_file_as_string(marker_path).strip_edges()
		if assembly_name == "":
			assembly_name = attached_assembly_name

	if assembly_name == "" or assembly_name == hot_reload_host.get_loaded_assembly_name():
		return

	if assembly_name == attached_assembly_name:
		hot_reload_host.use_attached_script(assembly_name)
		return

	if hot_reload_host.reload_assembly(assembly_name, reload_type_name):
		# 交换成功后移除 Demo 节点上的旧脚本实例，避免旧/新对象同时收到 _Process。
		var demo := get_node_or_null(demo_path)
		if demo != null:
			demo.set_script(null)
