extends Area2D

# C# Collectible 的 GDScript 版（Web 导出用）

@export_range(1, 3, 1)
var level := 1


func _ready() -> void:
	body_entered.connect(OnBodyEntered)


func OnBodyEntered(body: Node2D) -> void:
	if not body is Player:
		return

	var player := body as Player

	if not player.is_physics_processing():
		return

	GameState.Collect(level)

	queue_free()
