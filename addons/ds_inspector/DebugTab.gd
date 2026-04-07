@tool
extends TabContainer

@export
var tab_names: Array = []
@export
var debug_tool: CanvasLayer

func _ready():
    debug_tool.local.change_language.connect(_on_language_changed)
    _on_language_changed()
    pass

func _on_language_changed():
    for i in range(tab_names.size()):
        set_tab_title(i, debug_tool.local.get_str(tab_names[i]))
