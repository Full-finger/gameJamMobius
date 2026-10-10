extends Area2D

# C# endPoint 的 GDScript 版（Web 导出用）

@export var endUI: CanvasLayer
@export var nextScenePath := "res://scenes/main_menu.tscn"


func _ready() -> void:
	if endUI != null:
		endUI.hide()

	body_entered.connect(OnBodyEntered)


func OnBodyEntered(body: Node2D) -> void:
	if not body is Player:
		return

	var player := body as Player

	if not player.is_physics_processing():
		return

	# 拿到道具并成功到达终点，这时才永久计入本局。
	GameState.ConfirmLevelCollection()

	if endUI != null:
		endUI.process_mode = Node.PROCESS_MODE_ALWAYS
		endUI.show()

		# 禁止暂停菜单处理 Esc，避免玩家用 Esc 解除暂停。
		(get_parent().get_node("PauseMenu") as PauseMenu).set_process_input(false)

		get_tree().paused = true
	else:
		TransitionManager.TransitionTo(nextScenePath)
