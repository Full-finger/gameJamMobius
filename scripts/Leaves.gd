extends StaticBody2D

# C# Leaves 的 GDScript 版（Web 导出用）

var triggered := false

@export_group("节点")
@export var leafVisual: CanvasItem
@export var collision: CollisionShape2D
@export var stepArea: Area2D
@export var leafParticles: GPUParticles2D

@export_group("时间")
@export var disappearDelay := 2.0
@export var particleWaitTime := 1.0


func _ready() -> void:
	stepArea.body_entered.connect(OnBodyEntered)

	# 确保刚开始不会喷粒子
	leafParticles.emitting = false


func OnBodyEntered(body: Node2D) -> void:
	if triggered or not body is Player:
		return

	var player := body as Player

	# 从下面向上穿过时不触发
	if player.velocity.y < 0.0:
		return

	triggered = true

	# ==========================================
	# 1. 等两秒
	# 暂停游戏时这个 Timer 也暂停
	# ==========================================

	await get_tree().create_timer(disappearDelay, false).timeout

	# ==========================================
	# 2. 树叶本体消失
	# ==========================================

	leafVisual.visible = false

	collision.set_deferred("disabled", true)
	stepArea.set_deferred("monitoring", false)

	# ==========================================
	# 3. 播放叶片散落粒子
	# ==========================================

	leafParticles.restart()
	leafParticles.emitting = true

	# ==========================================
	# 4. 给粒子一点时间播完
	# ==========================================

	await get_tree().create_timer(particleWaitTime, false).timeout

	# ==========================================
	# 5. 整个树叶节点删除
	# ==========================================

	queue_free()
