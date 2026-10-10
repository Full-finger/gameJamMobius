class_name Cutscene
extends Control

# C# Cutscene 的 GDScript 版（Web 导出用）

@export var animation: AnimatedSprite2D

@export_file("*.tscn")
var nextScene := ""


func _ready() -> void:
	animation.sprite_frames.set_animation_loop_mode(
		animation.animation,
		SpriteFrames.LOOP_NONE
	)

	animation.animation_finished.connect(OnAnimationFinished)

	animation.play()


func OnAnimationFinished() -> void:
	TransitionManager.TransitionTo(nextScene)
