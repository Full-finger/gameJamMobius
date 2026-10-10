extends Area2D

# C# Light 的 GDScript 版（Web 导出用）


func _ready() -> void:
	body_entered.connect(OnBodyEntered)
	body_exited.connect(OnBodyExited)


func OnBodyEntered(body: Node2D) -> void:
	if body is Player:
		(body as Player).isInLight = true


func OnBodyExited(body: Node2D) -> void:
	if body is Player:
		(body as Player).isInLight = false
