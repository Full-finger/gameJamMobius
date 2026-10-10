class_name PauseMenu
extends CanvasLayer

# C# PauseMenu 的 GDScript 版（Web 导出用）

@export var restartButton: Button
@export var menuButton: Button
@export var settingsButton: Button

var settingsWindow: SettingsPage


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS

	hide()

	restartButton.pressed.connect(Restart)

	if menuButton != null:
		menuButton.pressed.connect(BackToMenu)

	if settingsButton != null:
		settingsButton.pressed.connect(OpenSettings)


func _input(event: InputEvent) -> void:
	# 设置打开时，Esc 由设置页处理，不能同时解除暂停。
	if settingsWindow != null:
		return

	# ui_cancel 默认包含 Esc。
	if event.is_action_pressed("ui_cancel"):
		visible = not visible
		get_tree().paused = visible

		get_viewport().set_input_as_handled()


func OpenSettings() -> void:
	if settingsWindow != null:
		return

	settingsWindow = (load("res://scenes/settings.tscn") as PackedScene).instantiate() as SettingsPage

	settingsWindow.ReturnToPauseMenu = self

	get_tree().paused = true

	add_child(settingsWindow)

	(get_node("Control") as Control).hide()


func CloseSettings() -> void:
	settingsWindow.queue_free()
	settingsWindow = null

	(get_node("Control") as Control).show()

	get_tree().paused = true

	settingsButton.grab_focus()


func Restart() -> void:
	TransitionManager.ReloadCurrentScene()


func BackToMenu() -> void:
	TransitionManager.TransitionTo("res://scenes/main_menu.tscn")
