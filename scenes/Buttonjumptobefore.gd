extends Button

# C# Buttonjumptobefore 的 GDScript 版（Web 导出用）


func _ready() -> void:
	pressed.connect(StartGame)


func StartGame() -> void:
	# 防止这 0.7 秒里重复点击
	disabled = true

	# 等待 0.1 秒，让动画播完
	await get_tree().create_timer(0.1).timeout

	TransitionManager.TransitionTo("res://scenes/JUMPSCENES/before_level_winter.tscn")
