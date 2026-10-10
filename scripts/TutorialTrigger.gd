extends Area2D

# C# TutorialTrigger 的 GDScript 版（Web 导出用）

@export_group("教程")
@export var messages: Array[String] = []
@export var oneShot := true

# static 不会随着重开关卡而清空；关闭游戏后才清空。
static var shownTutorials := {}

var tutorialKey := ""


func _ready() -> void:
	# 用关卡文件和节点路径区分每个触发器，不需要手动填编号。
	var level: Node = self

	while level.get_parent() != get_tree().root:
		level = level.get_parent()

	tutorialKey = level.scene_file_path + ":" + str(level.get_path_to(self))

	if oneShot and shownTutorials.has(tutorialKey):
		set_deferred("monitoring", false)

	body_entered.connect(OnBodyEntered)


func OnBodyEntered(body: Node2D) -> void:
	if not body is Player or (oneShot and shownTutorials.has(tutorialKey)):
		return

	var tutorialUI := get_tree().get_first_node_in_group("TutorialUI") as TutorialUI

	if tutorialUI == null:
		push_error("没有找到 TutorialUI")
		return

	# UI 忙着显示其他教程、或内容为空时，不算已经展示。
	if not tutorialUI.ShowMessages(messages):
		return

	if oneShot:
		shownTutorials[tutorialKey] = true

		set_deferred("monitoring", false)
