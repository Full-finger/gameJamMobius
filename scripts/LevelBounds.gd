extends StaticBody2D

# C# LevelBounds 的 GDScript 版（Web 导出用）

@export var player: Player


func _ready() -> void:
	var bounds := Rect2()
	var foundBackground := false

	# 把本关所有 LevelBackground 组里的背景图片合成一个总范围。
	for node in get_tree().get_nodes_in_group("LevelBackground"):
		var sprite := node as Sprite2D

		if sprite == null or not get_parent().is_ancestor_of(sprite):
			continue

		var piece: Rect2 = sprite.global_transform * sprite.get_rect()

		bounds = bounds.merge(piece) if foundBackground else piece
		foundBackground = true

	if not foundBackground:
		push_error("本关没有加入 LevelBackground 分组的背景图片。")
		return

	var camera := player.get_node("Camera2D") as LevelCamera

	camera.SetBackgroundBounds(bounds)

	# 两侧是无限高的边界，藤蔓把人甩高时也不能绕过墙顶。
	(get_node("WallLeft") as CollisionShape2D).global_position = Vector2(camera.limit_left, 0)
	(get_node("WallRight") as CollisionShape2D).global_position = Vector2(camera.limit_right, 0)

	# 底部保留坠落玩法，掉出画面后走玩家原有的死亡与重开流程。
	player.deathY = camera.limit_bottom + 200.0
