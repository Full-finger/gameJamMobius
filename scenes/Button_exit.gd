extends Button

# C# Button_exit 的 GDScript 版（Web 导出用）


func _ready() -> void:
	pressed.connect(QuitGame)


func QuitGame() -> void:
	get_tree().quit()
