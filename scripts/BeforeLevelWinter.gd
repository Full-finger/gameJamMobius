class_name BeforeLevelWinter
extends Control

# C# BeforeLevelWinter 的 GDScript 版（Web 导出用）

@export_group("三个填充图")
@export var item1: TextureRect
@export var item2: TextureRect
@export var item3: TextureRect

@export_group("时间")
@export var interval := 1.0

@export_group("下一场景")
@export_file("*.tscn")
var nextScene := "res://scenes/final_level.tscn"


func _ready() -> void:
	item1.visible = false
	item2.visible = false
	item3.visible = false

	PlaySequence()


func PlaySequence() -> void:
	# =========================
	# 第一件
	# =========================

	if GameState.Level1Collected:
		ShowItem(item1)

	await get_tree().create_timer(interval).timeout

	# =========================
	# 第二件
	# =========================

	if GameState.Level2Collected:
		ShowItem(item2)

	await get_tree().create_timer(interval).timeout

	# =========================
	# 第三件
	# =========================

	if GameState.Level3Collected:
		ShowItem(item3)

	await get_tree().create_timer(interval).timeout

	# =========================
	# 进入最终关
	# =========================

	TransitionManager.TransitionTo(nextScene)


func ShowItem(item: TextureRect) -> void:
	item.visible = true

	# 稍微做一个很简单的出现效果
	var originalScale := item.scale

	item.scale = originalScale * 0.7

	item.modulate = Color(
		item.modulate.r,
		item.modulate.g,
		item.modulate.b,
		0.0
	)

	var tween := create_tween()
	tween.set_parallel(true)

	tween.tween_property(item, "scale", originalScale, 0.2
	).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

	tween.tween_property(item, "modulate:a", 1.0, 0.15)
