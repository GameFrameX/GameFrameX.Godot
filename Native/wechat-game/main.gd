extends Node

# 微信小游戏 Godot+C# 场景化热更新宿主：
# Godot 侧不写死任何场景内容/UI——全部由 C# 热更程序集下发（SPEC 命令拉取规格），
# 行为状态（方向/速度/配色/标签文本/脉冲）由每次 TICK 的 state 字段驱动，按钮交互经 UIEV
# 命令转发进 C# MEMFS 事件文件；热切换程序集（SWITCH）后重新拉取规格 → 场景与 UI 整体换脸。
# 协议（保持与 wechat-integrated/game.js 判定严格一致，勿改 print 关键字）：
#   Godot→JS：print 通道 "INTEGRATED-GD-GDCMD: <命令> <序号> msec=<引擎毫秒>"
#     TICK / SPEC / UIEV <按钮ID> / SWITCH <程序集名> / PROBE <程序集名>
#   JS→Godot：user://js_to_godot.txt；"csharp bridge ready ..." 就绪标记；之后
#     "OK <序号> count=N ver=V state=<编码> msec="（TICK）
#     "OK <序号> assembly=<名> scene=<编码> ui=<编码> specError= msec="（SPEC）
#   编码：行以 ; 连接、空格以 ~ 占位。
# 已证死路（勿回退）：JavaScriptBridge.eval 返回 <null>；JS 读引擎写文件会死锁 wx fs。

const TICK_EVERY_FRAMES := 120  # 模拟器引擎帧率较高，大步长节流 wx 文件通道
const PROBE_AT_FRAME := 3720  # 热更边界取证：探测未预置程序集（版本切换已由 C# 依据 CDN manifest 决策）
const HEARTBEAT_EVERY_FRAMES := 600

var status_label: Label
var counter_label: Label
var interface_layer: CanvasLayer
var frame_index := 0
var command_sequence := 0
var latest_result_sequence := -1
var last_result_content := ""
var result_count := 0
var first_command_sent := false
var switched_to_v2 := false
var last_command_line := "SPEC"
var active_assembly_name := ""
var pending_switch_respec := false

# C# 规格驱动的场景状态
var scene_root: Node3D
var display_objects := {}  # 对象ID -> {instance, material, baseSpeedY, baseSpeedX}
var motion_direction := 1.0
var speed_multiplier := 1.0

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
	last_command_line = command_line
	print("INTEGRATED-GD-GDCMD: ", command_line, " ", command_sequence, " msec=", Time.get_ticks_msec())

func _parse_field(line: String, key: String) -> String:
	for part in line.split(" "):
		if part.begins_with(key + "="):
			return part.substr(key.length() + 1)
	return ""

func _decode_spec_field(encoded: String) -> Array:
	var lines := []
	for raw_line in encoded.replace("~", " ").split(";"):
		var trimmed := raw_line.strip_edges()
		if trimmed != "":
			lines.append(trimmed)
	return lines

func _clear_scene() -> void:
	for object_entry in display_objects.values():
		if is_instance_valid(object_entry.instance):
			object_entry.instance.queue_free()
	display_objects.clear()
	if is_instance_valid(scene_root):
		scene_root.queue_free()
	scene_root = null

func _clear_interface() -> void:
	for button in ui_buttons.values():
		if is_instance_valid(button):
			button.queue_free()
	ui_buttons.clear()
	for label in ui_labels.values():
		if is_instance_valid(label):
			label.queue_free()
	ui_labels.clear()

var ui_buttons := {}
var ui_labels := {}

func _build_mesh_for_type(object_type: String, object_size: float) -> Mesh:
	var mesh: Mesh
	if object_type == "sphere":
		var sphere_mesh := SphereMesh.new()
		sphere_mesh.radius = object_size * 0.5
		sphere_mesh.height = object_size
		mesh = sphere_mesh
	elif object_type == "torus":
		var torus_mesh := TorusMesh.new()
		torus_mesh.inner_radius = object_size * 0.35
		torus_mesh.outer_radius = object_size * 0.65
		mesh = torus_mesh
	elif object_type == "cylinder":
		var cylinder_mesh := CylinderMesh.new()
		cylinder_mesh.top_radius = object_size * 0.5
		cylinder_mesh.bottom_radius = object_size * 0.5
		cylinder_mesh.height = object_size * 2.0
		mesh = cylinder_mesh
	else:
		var box_mesh := BoxMesh.new()
		box_mesh.size = Vector3(object_size, object_size, object_size)
		mesh = box_mesh
	return mesh

func _build_scene_from_spec(encoded_spec: String) -> void:
	_clear_scene()
	scene_root = Node3D.new()
	scene_root.name = "CSharpSpecScene"
	add_child(scene_root)
	var environment := Environment.new()
	var sky_material := ProceduralSkyMaterial.new()
	var camera := Camera3D.new()
	var sun_light := DirectionalLight3D.new()
	var ground := MeshInstance3D.new()
	for line in _decode_spec_field(encoded_spec):
		var parts: PackedStringArray = line.split(" ")
		if parts.size() >= 10 and parts[0] == "sky":
			sky_material.sky_top_color = Color(parts[1].to_float(), parts[2].to_float(), parts[3].to_float())
			sky_material.sky_horizon_color = Color(parts[4].to_float(), parts[5].to_float(), parts[6].to_float())
			sky_material.ground_bottom_color = Color(parts[7].to_float(), parts[8].to_float(), parts[9].to_float())
			sky_material.ground_horizon_color = sky_material.sky_horizon_color
		elif parts.size() >= 4 and parts[0] == "ground":
			var ground_mesh := PlaneMesh.new()
			ground_mesh.size = Vector2(24.0, 24.0)
			var ground_material := StandardMaterial3D.new()
			ground_material.albedo_color = Color(parts[1].to_float(), parts[2].to_float(), parts[3].to_float())
			ground_material.roughness = 0.9
			ground_mesh.material = ground_material
			ground.mesh = ground_mesh
		elif parts.size() >= 5 and parts[0] == "camera":
			camera.position = Vector3(parts[1].to_float(), parts[2].to_float(), parts[3].to_float())
			var look_height := 0.4
			if parts.size() >= 5:
				look_height = parts[4].to_float()
			camera.look_at_from_position(camera.position, Vector3(0.0, look_height, 0.0), Vector3.UP)
		elif parts.size() >= 4 and parts[0] == "light":
			sun_light.rotation_degrees = Vector3(parts[1].to_float(), parts[2].to_float(), 0.0)
			sun_light.light_energy = parts[3].to_float()
		elif parts.size() >= 13 and parts[0] == "object":
			var object_identifier := parts[1]
			var object_type := parts[2]
			var object_position := Vector3(parts[3].to_float(), parts[4].to_float(), parts[5].to_float())
			var object_size := parts[6].to_float()
			var object_material := StandardMaterial3D.new()
			object_material.albedo_color = Color(parts[7].to_float(), parts[8].to_float(), parts[9].to_float())
			object_material.metallic = parts[10].to_float()
			object_material.roughness = 0.35
			var mesh_instance := MeshInstance3D.new()
			mesh_instance.name = object_identifier
			mesh_instance.mesh = _build_mesh_for_type(object_type, object_size)
			mesh_instance.material_override = object_material
			mesh_instance.position = object_position
			scene_root.add_child(mesh_instance)
			display_objects[object_identifier] = {
				"instance": mesh_instance,
				"material": object_material,
				"baseSpeedY": parts[11].to_float(),
				"baseSpeedX": parts[12].to_float(),
			}
	var sky := Sky.new()
	sky.sky_material = sky_material
	environment.background_mode = Environment.BG_SKY
	environment.sky = sky
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_SKY
	environment.ambient_light_energy = 1.1
	environment.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	var world_environment := WorldEnvironment.new()
	world_environment.environment = environment
	scene_root.add_child(world_environment)
	scene_root.add_child(camera)
	camera.current = true
	scene_root.add_child(sun_light)
	if ground.mesh != null:
		scene_root.add_child(ground)

func _build_interface_from_spec(encoded_spec: String) -> void:
	_clear_interface()
	if interface_layer == null:
		return
	for line in _decode_spec_field(encoded_spec):
		var parts: PackedStringArray = line.split(" ", true, 4)
		if parts.size() >= 5 and parts[0] == "button":
			var button := Button.new()
			button.name = parts[1]
			button.text = parts[4]
			button.position = Vector2(parts[2].to_float(), parts[3].to_float())
			button.add_theme_font_size_override("font_size", parts[4].length() * 0 + 40)
			button.pressed.connect(_on_spec_button_pressed.bind(parts[1]))
			interface_layer.add_child(button)
			ui_buttons[parts[1]] = button
		elif parts.size() >= 5 and parts[0] == "label":
			var label := Label.new()
			label.name = parts[1]
			label.text = parts[4]
			label.position = Vector2(parts[2].to_float(), parts[3].to_float())
			label.add_theme_font_size_override("font_size", 34)
			interface_layer.add_child(label)
			ui_labels[parts[1]] = label
			if parts[1] == "status":
				status_label = label
			elif parts[1] == "counter":
				counter_label = label

func _on_spec_button_pressed(button_identifier: String) -> void:
	print("INTEGRATED-GD: ui button pressed id=", button_identifier)
	_send_command("UIEV " + button_identifier)

func _apply_state(encoded_state: String) -> void:
	for line in _decode_spec_field(encoded_state):
		var parts: PackedStringArray = line.split(" ", true, 4)
		if parts.size() >= 3 and parts[0] == "motion":
			motion_direction = parts[1].to_float()
			speed_multiplier = parts[2].to_float()
		elif parts.size() >= 6 and parts[0] == "objectState":
			var object_entry = display_objects.get(parts[1])
			if object_entry != null and is_instance_valid(object_entry.instance):
				object_entry.material.albedo_color = Color(parts[2].to_float(), parts[3].to_float(), parts[4].to_float())
				var pulse := parts[5].to_float()
				if pulse > 0.0:
					var visual_scale := 1.0 + pulse
					object_entry.instance.scale = Vector3(visual_scale, visual_scale, visual_scale)
				elif object_entry.instance.scale != Vector3.ONE:
					object_entry.instance.scale = Vector3.ONE
		elif parts.size() >= 3 and parts[0] == "labelText":
			var label = ui_labels.get(parts[1])
			if label != null and is_instance_valid(label):
				label.text = parts[2]

func _handle_result(result: String) -> void:
	var parts := result.split(" ")
	if parts.size() < 2 or (parts[0] != "OK" and parts[0] != "WAIT"):
		status_label.text = result.substr(0, 60)
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
		var state_encoded := _parse_field(result, "state")
		if state_encoded != "":
			_apply_state(state_encoded)
		elif counter_label != null and counter_label.text.begins_with("C# ticks"):
			counter_label.text = "C# ticks: %d (v%d)" % [count, version]
		# C# 热更决策执行后的程序集变化：重新拉取规格（场景与 UI 换脸）
		var assembly_field := _parse_field(result, "assembly")
		if assembly_field != "" and assembly_field != active_assembly_name:
			active_assembly_name = assembly_field
			pending_switch_respec = true
			print("INTEGRATED-GD: active assembly changed -> ", assembly_field)
		if sequence == 1:
			print("INTEGRATED-GD: stage2 godot-driven first count -> ", count, " roundtripMs=", roundtrip)
		elif result_count % 10 == 0:
			print("INTEGRATED-GD: tick seq=", sequence, " count=", count, " ver=", version, " roundtripMs=", roundtrip)
	var scene_encoded := _parse_field(result, "scene")
	if scene_encoded != "" or _parse_field(result, "specError") != "":
		var assembly_name := _parse_field(result, "assembly")
		var spec_error := _parse_field(result, "specError")
		if scene_encoded != "":
			_build_scene_from_spec(scene_encoded)
			_build_interface_from_spec(_parse_field(result, "ui"))
			print("INTEGRATED-GD: spec applied assembly=", assembly_name,
				" objects=", display_objects.size(), " buttons=", ui_buttons.size())
			if status_label != null:
				status_label.text = "C# 场景已加载: " + assembly_name
		elif status_label != null:
			status_label.text = "spec error: " + spec_error.substr(0, 40)
	if _parse_field(result, "switched") != "":
		var switched := int(_parse_field(result, "switched"))
		var target := _parse_field(result, "target")
		print("INTEGRATED-GD: hot switch target=", target, " switched=", switched)
		if switched == 1:
			switched_to_v2 = true
			pending_switch_respec = true
	var event_error := _parse_field(result, "eventError")
	if event_error != "" and status_label != null:
		status_label.text = "ui event error: " + event_error.substr(0, 40)
	if _parse_field(result, "handle") != "":
		print("INTEGRATED-GD: assembly probe target=", _parse_field(result, "target"), " handle=", _parse_field(result, "handle"))

func _ready() -> void:
	interface_layer = CanvasLayer.new()
	add_child(interface_layer)
	status_label = Label.new()
	status_label.name = "status"
	status_label.text = "waiting csharp scene spec"
	status_label.position = Vector2(36.0, 270.0)
	status_label.add_theme_font_size_override("font_size", 34)
	interface_layer.add_child(status_label)
	ui_labels["status"] = status_label
	counter_label = Label.new()
	counter_label.name = "counter"
	counter_label.text = "C# ticks: booting"
	counter_label.position = Vector2(36.0, 322.0)
	counter_label.add_theme_font_size_override("font_size", 30)
	interface_layer.add_child(counter_label)
	ui_labels["counter"] = counter_label
	print("INTEGRATED-GD: scene ready, frame=", Engine.get_process_frames())
	print("INTEGRATED-GD: JavaScriptBridge class ", "present" if ClassDB.class_exists("JavaScriptBridge") else "MISSING")
	print("INTEGRATED-GD: eval-probe 1+1 -> [", str(JavaScriptBridge.eval("1+1", true)), "]")

func _process(delta: float) -> void:
	# 视觉：按 C# 下发的速度/方向驱动每个对象自转
	for object_entry in display_objects.values():
		if not is_instance_valid(object_entry.instance):
			continue
		var spin := delta * float(object_entry.baseSpeedY) * speed_multiplier * motion_direction
		object_entry.instance.rotate_y(spin)
		var tilt := delta * float(object_entry.baseSpeedX) * speed_multiplier * motion_direction
		object_entry.instance.rotate_x(tilt)
	if frame_index <= 30:
		print("INTEGRATED-GD: proc frame=", frame_index + 1, " msec=", Time.get_ticks_msec())
	frame_index += 1
	# 文件通道节流：所有 user:// 读写统一到每 TICK_EVERY_FRAMES 帧一次（高频读写会饿死 JS 定时器）
	if frame_index % TICK_EVERY_FRAMES != 0:
		return
	print("INTEGRATED-GD: channel-read-begin frame=", frame_index)
	var result := _read_user_file("js_to_godot.txt")
	print("INTEGRATED-GD: channel-read-end frame=", frame_index, " len=", result.length())
	if result != "" and result != last_result_content:
		last_result_content = result
		_handle_result(result)
	# 首命令门控：C# 桥就绪标记出现后拉取场景规格（场景/UI/逻辑全部来自热更程序集）
	if not first_command_sent:
		if result.find("csharp bridge ready") >= 0:
			first_command_sent = true
			print("INTEGRATED-GD: js channel READY -> ", result)
			_send_command("SPEC")
		else:
			status_label.text = "waiting csharp bridge (frame %d)" % frame_index
			if frame_index % 600 == 0:
				print("INTEGRATED-GD: waiting csharp bridge at frame ", frame_index, " fps=", Engine.get_frames_per_second())
			return
	# 桥未就绪期发出的命令被 WAIT：30 帧后重发上一命令
	if last_result_content.begins_with("WAIT") and frame_index % 30 == 0:
		_send_command(last_command_line)
	# 热切换成功后：下一拍重新拉取规格（V2 场景与 UI 换脸）
	if pending_switch_respec:
		pending_switch_respec = false
		_send_command("SPEC")
		return
	# 通道心跳（Godot→JS 方向持续取证）
	if frame_index % HEARTBEAT_EVERY_FRAMES == 0:
		print("INTEGRATED-GD: heartbeat frame=", frame_index, " msec=", Time.get_ticks_msec())
	# 帧循环驱动 C# Tick（状态经 TICK 响应回填）
	if frame_index % TICK_EVERY_FRAMES == 0:
		_send_command("TICK")
	# 能力边界取证：探测未预置程序集（load 不到句柄即边界证据；版本切换已由 C# 决策驱动）
	if frame_index == PROBE_AT_FRAME:
		_send_command("PROBE WasmLogicMissing")
