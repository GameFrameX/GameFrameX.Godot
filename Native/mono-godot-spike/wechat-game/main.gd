extends Node

# 微信小游戏 Godot+C# 全链路集成（阶段2/3/4）：
# 已证死路：JavaScriptBridge.eval 在 godots.app 微信定制构建返回 <null>（探针取证落盘）。
# 已证活路：user://（WXMEMFS→wx USER_DATA_PATH/godot/app_userdata/<项目名>/）双向文件通道。
# 协议（均为整行覆盖写，另一侧轮询比对新内容）：
#   Godot→JS：user://godot_command.txt，格式 "<命令> <序号> msec=<引擎毫秒>"
#     TICK（驱动 C# Tick+读计数）/ SWITCH <程序集名>（热切换）/ PROBE <程序集名>（句柄探测）
#   JS→Godot：user://js_to_godot.txt，"csharp bridge ready ..." 为就绪标记，之后 "OK <序号> key=value ..."；
#     桥未就绪时回应 "WAIT ..."（Godot 30 帧后重发）
# 程序化取证：JS 桥把命令/响应/耗时写微信取证文件；本脚本 print 由 console 钩子同文件收录。

const TICK_EVERY_FRAMES := 120  # 模拟器引擎帧率极高(~300fps)，大步长节流 wx 文件通道
const SWITCH_AT_FRAME := 3600
const HEARTBEAT_EVERY_FRAMES := 600

var tick_label: Label
var status_label: Label
var frame_index := 0
var command_sequence := 0
var latest_result_sequence := -1
var last_result_content := ""
var result_count := 0
var first_tick_sent := false
var switched_to_v2 := false

func _write_user_file(file_name: String, content: String) -> void:
	var file := FileAccess.open("user://" + file_name, FileAccess.WRITE)
	if file:
		file.store_string(content)
		file.close()

func _read_user_file(file_name: String) -> String:
	if not FileAccess.file_exists("user://" + file_name):
		return ""
	var reader := FileAccess.open("user://" + file_name, FileAccess.READ)
	if reader == null:
		return ""
	var content := reader.get_as_text()
	reader.close()
	return content

func _send_command(command_line: String) -> void:
	command_sequence += 1
	# 命令走 print 通道（console 钩子同步接收；JS 读引擎写的文件会死锁 wx fs，已实证）
	print("INTEGRATED-GD-GDCMD: ", command_line, " ", command_sequence, " msec=", Time.get_ticks_msec())

func _parse_field(line: String, key: String) -> String:
	for part in line.split(" "):
		if part.begins_with(key + "="):
			return part.substr(key.length() + 1)
	return ""

func _handle_result(result: String) -> void:
	var parts := result.split(" ")
	if parts.size() < 2 or (parts[0] != "OK" and parts[0] != "WAIT"):
		status_label.text = result
		return
	var sequence := int(parts[1])
	if sequence > command_sequence:
		return  # 上一会话残留响应（命令文件跨重启持久），忽略
	if sequence <= latest_result_sequence:
		return
	latest_result_sequence = sequence
	result_count += 1
	var roundtrip := Time.get_ticks_msec() - int(_parse_field(result, "msec"))
	if _parse_field(result, "count") != "":
		var count := int(_parse_field(result, "count"))
		var version := int(_parse_field(result, "ver"))
		tick_label.text = "C# ticks: %d (v%d)" % [count, version]
		if sequence == 1:
			print("INTEGRATED-GD: stage2 godot-driven first count -> ", count, " roundtripMs=", roundtrip)
		elif result_count % 10 == 0:
			print("INTEGRATED-GD: tick seq=", sequence, " count=", count, " ver=", version, " roundtripMs=", roundtrip)
	if _parse_field(result, "switched") != "":
		var switched := int(_parse_field(result, "switched"))
		var target := _parse_field(result, "target")
		print("INTEGRATED-GD: hot switch target=", target, " switched=", switched)
		if switched == 1:
			switched_to_v2 = true
			status_label.text = "switched to " + target
	if _parse_field(result, "handle") != "":
		print("INTEGRATED-GD: assembly probe target=", _parse_field(result, "target"), " handle=", _parse_field(result, "handle"))

func _ready() -> void:
	tick_label = Label.new()
	tick_label.text = "C# ticks: booting"
	tick_label.position = Vector2(36, 200)
	tick_label.add_theme_font_size_override("font_size", 52)
	# 注意：微信定制构建下 max_fps 限帧会卡死帧循环（已实证），改用大步长节流文件通道
	status_label = Label.new()
	status_label.text = "waiting js channel"
	status_label.position = Vector2(36, 300)
	status_label.add_theme_font_size_override("font_size", 26)
	add_child(status_label)
	print("INTEGRATED-GD: scene ready, frame=", Engine.get_process_frames())
	print("INTEGRATED-GD: JavaScriptBridge class ", "present" if ClassDB.class_exists("JavaScriptBridge") else "MISSING")
	print("INTEGRATED-GD: eval-probe 1+1 -> [", str(JavaScriptBridge.eval("1+1", true)), "]")

func _process(_delta: float) -> void:
	# 修复：本函数声明行此前丢失（循环体被并入 _ready 只跑一次 → 无心跳无 TICK）
	if frame_index <= 30:
		print("INTEGRATED-GD: proc frame=", frame_index + 1, " msec=", Time.get_ticks_msec())
	frame_index += 1
	# 文件通道节流：所有 user:// 读写统一到每 TICK_EVERY_FRAMES 帧一次（高频读写会饿死 JS 定时器）
	if frame_index % TICK_EVERY_FRAMES != 0:
		return
	# 通道读取定点取证：begin 有 end 无 → 引擎卡死在 user:// 读（WXMEMFS 死锁定位用）
	print("INTEGRATED-GD: channel-read-begin frame=", frame_index)
	var result := _read_user_file("js_to_godot.txt")
	print("INTEGRATED-GD: channel-read-end frame=", frame_index, " len=", result.length())
	if result != "" and result != last_result_content:
		last_result_content = result
		_handle_result(result)
	# 阶段2 门控：C# 桥就绪标记出现后才发首个 TICK（Godot 主动驱动）
	if not first_tick_sent:
		if result.find("csharp bridge ready") >= 0:
			first_tick_sent = true
			print("INTEGRATED-GD: js channel READY -> ", result)
			_send_command("TICK")
		else:
			status_label.text = "waiting csharp bridge (frame %d)" % frame_index
			if frame_index % 600 == 0:
				print("INTEGRATED-GD: waiting csharp bridge at frame ", frame_index, " fps=", Engine.get_frames_per_second())
			return
	# 桥未就绪期发出的命令被 WAIT：30 帧后重发
	if last_result_content.begins_with("WAIT") and frame_index % 30 == 0:
		_send_command("TICK")
	# 通道心跳（Godot→JS 方向持续取证）
	if frame_index % HEARTBEAT_EVERY_FRAMES == 0:
		# 心跳取证同样走 print（不再写文件，避免 JS 侧读引擎文件）
		print("INTEGRATED-GD: heartbeat frame=", frame_index, " msec=", Time.get_ticks_msec())
	# 阶段3：每 TICK_EVERY_FRAMES 帧驱动一次 C# Tick
	if frame_index % TICK_EVERY_FRAMES == 0:
		_send_command("TICK")
	# 阶段4：帧阈值触发热切换 + 能力边界探测（未打包 V2 时 switched=0 即边界证据）
	if frame_index == SWITCH_AT_FRAME:
		_send_command("SWITCH WasmLogicV2")
	if frame_index == SWITCH_AT_FRAME + 120:
		_send_command("PROBE WasmLogicMissing")
