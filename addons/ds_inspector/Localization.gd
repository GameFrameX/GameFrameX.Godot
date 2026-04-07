@tool
extends Node
class_name DsLocalization

# 信号，当语言改变时发出
signal change_language

var curr_locale: String = "en"
var curr_map: Dictionary = {}

var default_locale: String = "en"
var default_map: Dictionary = {}

# 所有可用的语言列表 {"locale_code": "locale_name"}
var available_locales: Dictionary = {}

var _is_initialized: bool = false

func _init():
    _scan_available_locales()
    change_locale(default_locale)
    _is_initialized = true

func change_locale(new_locale: String):
    curr_locale = new_locale
    var locale_path = "res://addons/ds_inspector/Localization/" + new_locale + ".json"
    
    # 加载语言文件
    if FileAccess.file_exists(locale_path):
        var file = FileAccess.open(locale_path, FileAccess.READ)
        if file:
            var json_string = file.get_as_text()
            file.close()
            
            var json = JSON.new()
            var error = json.parse(json_string)
            if error == OK:
                curr_map = json.data
            else:
                push_error("Failed to parse JSON for locale: " + new_locale)
                curr_map = {}
    else:
        push_error("Locale file not found: " + locale_path)
        curr_map = {}
    
    # 加载默认语言文件（如果还没加载）
    if default_map.is_empty():
        var default_path = "res://addons/ds_inspector/Localization/" + default_locale + ".json"
        if FileAccess.file_exists(default_path):
            var file = FileAccess.open(default_path, FileAccess.READ)
            if file:
                var json_string = file.get_as_text()
                file.close()
                
                var json = JSON.new()
                var error = json.parse(json_string)
                if error == OK:
                    default_map = json.data
    
    if _is_initialized:
        change_language.emit()
    

func get_str(key: String) -> String:
    # 返回当前语言的字符串，如果当前语言没有则返回默认语言的字符串，如果默认语言也没有则返回key
    if curr_map.has(key):
        return curr_map[key]
    elif default_map.has(key):
        return default_map[key]
    else:
        return key

func get_str_replace1(key: String, var1: String) -> String:
    return get_str(key).replace("{0}", var1)

func get_str_replace2(key: String, var1: String, var2: String) -> String:
    return get_str(key).replace("{0}", var1).replace("{1}", var2)

# 扫描 Localization 文件夹，识别所有可用的语言
func _scan_available_locales():
    available_locales.clear()
    var localization_path = "res://addons/ds_inspector/Localization/"
    
    var dir = DirAccess.open(localization_path)
    if dir:
        dir.list_dir_begin()
        var file_name = dir.get_next()
        
        while file_name != "":
            # 只处理 .json 文件
            if not dir.current_is_dir() and file_name.ends_with(".json"):
                # 提取语言代码（文件名去掉 .json 后缀）
                var locale = file_name.trim_suffix(".json")
                
                # 读取语言文件获取语言名称
                var locale_file_path = localization_path + file_name
                var file = FileAccess.open(locale_file_path, FileAccess.READ)
                if file:
                    var json_string = file.get_as_text()
                    file.close()
                    
                    var json = JSON.new()
                    var error = json.parse(json_string)
                    if error == OK:
                        var locale_data = json.data
                        # 获取 locale_name 字段，如果没有则使用语言代码作为名称
                        var locale_name = locale_data.get("locale_name", locale)
                        available_locales[locale] = locale_name
                    else:
                        push_warning("Failed to parse JSON for locale: " + locale)
                        available_locales[locale] = locale
                else:
                    available_locales[locale] = locale
            
            file_name = dir.get_next()
        
        dir.list_dir_end()
    else:
        push_error("Failed to open Localization directory: " + localization_path)
    
    # 确保默认语言在列表中
    if not available_locales.has(default_locale):
        push_warning("Default locale '" + default_locale + "' not found in available locales")
