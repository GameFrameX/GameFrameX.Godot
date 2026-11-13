extends Node

var frame_index := 0

func _process(_delta: float) -> void:
	frame_index += 1
	if frame_index % 60 == 0:
		print("MINIMAL-GD: frame ", frame_index)
