extends Area2D

# C# GrabArea 的 GDScript 版（Web 导出用）


func _physics_process(delta: float) -> void:
	# 跳离冷却结束后，即使仍在范围里也能再次抓住。
	for body in get_overlapping_bodies():
		if not body is Player:
			continue

		var player := body as Player
		var target := get_parent() as RigidBody2D

		var center: Vector2 = (player.get_node("CollisionShape2D") as CollisionShape2D).global_position
		var distance := center.distance_squared_to(target.global_position)
		var nearest := true

		for node in target.get_parent().get_children():
			var other := node as RigidBody2D

			if other != null and other != target \
					and (other.get_node("GrabArea") as Area2D).overlaps_body(player) \
					and center.distance_squared_to(other.global_position) < distance:
				nearest = false
				break

		if nearest:
			player.GrabVine(target)
