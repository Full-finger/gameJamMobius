extends TextureRect

# C# InLightCrown 的 GDScript 版（Web 导出用）

@export var player: Player


func _process(delta: float) -> void:
	if player == null:
		return

	visible = is_equal_approx(player.LifeRatio, 1.0)
