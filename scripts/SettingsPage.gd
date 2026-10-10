class_name SettingsPage
extends CanvasLayer

# C# SettingsPage 的 GDScript 版（Web 导出用）

# 暂停页打开时填入；主菜单直接切场景时保持为空。
var ReturnToPauseMenu: PauseMenu


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	layer = 10

	(get_node("Control/Return") as Button_ReturnToMain).BackRequested.connect(GoBack)


func _input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_cancel"):
		get_viewport().set_input_as_handled()
		GoBack()


func GoBack() -> void:
	if ReturnToPauseMenu != null:
		ReturnToPauseMenu.CloseSettings()
	else:
		TransitionManager.TransitionTo("res://scenes/main_menu.tscn")
