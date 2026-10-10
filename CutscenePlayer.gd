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

@export_group("结尾黑屏文字")
@export_multiline
var finalBlackoutText := ""


var currentStep := 0

var characterCounter := 0.0

var textFinished := false

var finished := false

var showingFinalBlackout := false

var blackScreen: ColorRect

var finalTextLabel: Label


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

	var total := GetActiveCharacterCount()

	var visible := mini(int(characterCounter), total)

	SetActiveVisibleCharacters(visible)

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
		SetActiveVisibleCharacters(GetActiveCharacterCount())

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
		if not showingFinalBlackout and not finalBlackoutText.is_empty():
			ShowFinalBlackout()
			return

		FinishCutscene()
		return

	ShowStep(currentStep)


# =========================================================
# 结尾黑屏
# =========================================================

func ShowFinalBlackout() -> void:
	showingFinalBlackout = true

	cgDisplay.hide()
	dialogueBox.hide()

	blackScreen = ColorRect.new()
	blackScreen.color = Color.BLACK
	blackScreen.mouse_filter = Control.MOUSE_FILTER_IGNORE
	blackScreen.z_index = 100
	add_child(blackScreen)
	blackScreen.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	finalTextLabel = Label.new()
	finalTextLabel.text = finalBlackoutText
	finalTextLabel.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	finalTextLabel.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	finalTextLabel.add_theme_color_override("font_color", Color.WHITE)
	finalTextLabel.add_theme_font_size_override("font_size", 56)

	var font := textLabel.get_theme_default_font()

	if font != null:
		finalTextLabel.add_theme_font_override("font", font)

	finalTextLabel.z_index = 101
	finalTextLabel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(finalTextLabel)
	finalTextLabel.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	characterCounter = 0.0
	finalTextLabel.visible_characters = 0
	textFinished = false
	continueHint.visible = false


func GetActiveCharacterCount() -> int:
	if showingFinalBlackout:
		return finalTextLabel.get_total_character_count()

	return textLabel.get_total_character_count()


func SetActiveVisibleCharacters(count: int) -> void:
	if showingFinalBlackout:
		finalTextLabel.visible_characters = count
		return

	textLabel.visible_characters = count


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
