class_name SettingsPage
extends CanvasLayer

# C# SettingsPage 的 GDScript 版（Web 导出用）

# 暂停页打开时填入；主菜单直接切场景时保持为空。
var ReturnToPauseMenu: PauseMenu

var returnButton: Button_ReturnToMain


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	layer = 10

	returnButton = get_node("Control/Return") as Button_ReturnToMain
	returnButton.BackRequested.connect(GoBack)


func _input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_cancel"):
		get_viewport().set_input_as_handled()
		returnButton.PlayReturnAnimation()


func GoBack() -> void:
	if ReturnToPauseMenu != null:
		ReturnToPauseMenu.CloseSettings()
	else:
		TransitionManager.TransitionTo("res://scenes/main_menu.tscn")
