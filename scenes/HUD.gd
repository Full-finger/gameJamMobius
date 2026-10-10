extends CanvasLayer

# C# HUD 的 GDScript 版（Web 导出用）

@export var player: Player
@export var lifeBar: TextureProgressBar


func _ready() -> void:
	lifeBar.min_value = 0.0
	lifeBar.max_value = 1.0
	lifeBar.value = 1.0


func _process(delta: float) -> void:
	if player == null:
		return

	lifeBar.value = player.LifeRatio
