class_name CutscenePlayer
extends Control

# C# CutscenePlayer 的 GDScript 版（Web 导出用）

@export_group("节点")
@export var cgDisplay: TextureRect
@export var dialogueBox: Control
@export var textLabel: RichTextLabel
@export var continueHint: Control

@export_group("剧情")
@export var steps: Array[CutsceneStep] = []

@export_group("打字机")
@export var charactersPerSecond := 35.0

@export_group("下一场景")
@export_file("*.tscn")
var nextScene := ""


var currentStep := 0

var characterCounter := 0.0

var textFinished := false

var finished := false


func _ready() -> void:
	continueHint.visible = false

	if steps.is_empty():
		FinishCutscene()
		return

	ShowStep(0)


# =========================================================
# 显示某一步
# =========================================================

func ShowStep(index: int) -> void:
	if index < 0 or index >= steps.size():
		return

	var step := steps[index]

	# -----------------------------------------
	# CG
	# -----------------------------------------

	# 没填图片就继续保持上一张
	if step.cg != null:
		cgDisplay.texture = step.cg

		cgDisplay.modulate = Color(1.0, 1.0, 1.0, 0.0)

		var tween := create_tween()

		tween.tween_property(cgDisplay, "modulate:a", 1.0, 0.25
		).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)

	# -----------------------------------------
	# 对话
	# -----------------------------------------

	textLabel.text = step.text

	characterCounter = 0.0
	textLabel.visible_characters = 0

	textFinished = false
	continueHint.visible = false

	# 如果这一 Step 根本没文字，就直接认为文字显示完成
	if step.text.is_empty():
		textFinished = true
		continueHint.visible = true


# =========================================================
# 打字机
# =========================================================

func _process(delta: float) -> void:
	if finished or textFinished:
		return

	characterCounter += charactersPerSecond * delta

	var total := textLabel.get_total_character_count()

	var visible := mini(int(characterCounter), total)

	textLabel.visible_characters = visible

	if visible >= total:
		textFinished = true

		continueHint.visible = true


# =========================================================
# 键盘 / 鼠标继续
# =========================================================

func _input(event: InputEvent) -> void:
	if finished:
		return

	var pressed := false

	# 键盘
	if event is InputEventKey:
		if event.pressed and not event.echo:
			pressed = true
	# 鼠标

	elif event is InputEventMouseButton:
		if event.pressed:
			pressed = true
	# 手柄

	elif event is InputEventJoypadButton:
		if event.pressed:
			pressed = true

	if not pressed:
		return

	get_viewport().set_input_as_handled()

	# =====================================================
	# 当前文字还没打完：
	# 第一次点击直接全部显示
	# =====================================================

	if not textFinished:
		textLabel.visible_characters = textLabel.get_total_character_count()

		textFinished = true

		continueHint.visible = true

		return

	# =====================================================
	# 已经打完：下一 Step
	# =====================================================

	NextStep()


# =========================================================
# 下一步
# =========================================================

func NextStep() -> void:
	currentStep += 1

	if currentStep >= steps.size():
		FinishCutscene()
		return

	ShowStep(currentStep)


# =========================================================
# CG结束
# =========================================================

func FinishCutscene() -> void:
	if finished:
		return

	finished = true

	# 使用全局黑屏转场
	if not nextScene.is_empty():
		TransitionManager.TransitionTo(nextScene, 0.6, 0.6)
