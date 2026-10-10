class_name Button_ReturnToMain
extends Button

# C# Button_ReturnToMain 的 GDScript 版（Web 导出用）

signal BackRequested


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_INHERIT
	pressed.connect(StartGame)


func StartGame() -> void:
	BackRequested.emit()
