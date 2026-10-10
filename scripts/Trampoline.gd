extends Area2D

# C# Trampoline 的 GDScript 版（Web 导出用）
# 落到荷叶上时自动弹起；从下方跳过时不触发。

@export var launchSpeed := 1700.0


func _physics_process(delta: float) -> void:
	for body in get_overlapping_bodies():
		if not body is Player:
			continue

		var player := body as Player

		if not player.is_physics_processing() or player.velocity.y < 0.0:
			continue

		var playerShape := player.get_node("CollisionShape2D") as CollisionShape2D

		var feetY: float = playerShape.global_position.y \
			+ (playerShape.shape as RectangleShape2D).size.y \
			* playerShape.global_scale.y * 0.5

		var areaShape := get_node("CollisionShape2D") as CollisionShape2D

		var areaBottom: float = areaShape.global_position.y \
			+ (areaShape.shape as RectangleShape2D).size.y \
			* areaShape.global_scale.y * 0.5

		# 脚已低于弹跳区时属于侧面擦过，不要把玩家突然弹上去。
		if feetY <= areaBottom + 4.0:
			player.Launch(launchSpeed)
