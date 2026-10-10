extends Area2D

# C# TrueEndingArea 的 GDScript 版（Web 导出用）

@export_file("*.tscn")
var trueEndingScene := "res://scenes/true_ending.tscn"


func _ready() -> void:
	body_entered.connect(OnBodyEntered)

	var unlocked: bool = GameState.AllCollected

	visible = unlocked
	monitoring = unlocked
	monitorable = unlocked


func OnBodyEntered(body: Node2D) -> void:
	if not body is Player:
		return

	TransitionManager.TransitionTo(trueEndingScene)
