extends Area2D

# C# NormalEndingArea 的 GDScript 版（Web 导出用）

@export_file("*.tscn")
var endingScene := "res://scenes/normal_ending.tscn"


func _ready() -> void:
	body_entered.connect(OnBodyEntered)


func OnBodyEntered(body: Node2D) -> void:
	if not body is Player:
		return

	get_tree().change_scene_to_file(endingScene)
