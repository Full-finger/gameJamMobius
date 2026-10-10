class_name TutorialUI
extends CanvasLayer

# C# TutorialUI 的 GDScript 版（Web 导出用）

@export_group("节点")
@export var root: Control
@export var textLabel: RichTextLabel
@export var continueHint: Control

@export_group("打字机")
@export var charactersPerSecond := 35.0

var messages: Array[String] = []

var currentIndex := 0

var characterCounter := 0.0

var active := false
var textFinished := false

# 用于恢复教程出现之前的暂停状态
var previousPausedState := false


func _ready() -> void:
	# 非常重要：
	# 游戏暂停以后这个 UI 仍然必须继续运行
	process_mode = Node.PROCESS_MODE_ALWAYS

	root.visible = false
	continueHint.visible = false


# =========================================================
# 外部调用这个函数开始教程
# =========================================================

func ShowMessages(newMessages: Array) -> bool:
	# 正在播放教程就不重复触发
	if active:
		return false

	messages.assign(newMessages)

	if messages.is_empty():
		return false

	active = true
	currentIndex = 0

	# 记录原本是不是暂停状态
	previousPausedState = get_tree().paused

	# 暂停整个游戏
	get_tree().paused = true

	root.visible = true

	StartCurrentMessage()

	return true


# =========================================================
# 开始显示当前这一句话
# =========================================================

func StartCurrentMessage() -> void:
	textLabel.text = messages[currentIndex]

	textLabel.visible_characters = 0

	characterCounter = 0.0

	textFinished = false

	continueHint.visible = false


# =========================================================
# 打字机
# =========================================================

func _process(delta: float) -> void:
	if not active or textFinished:
		return

	characterCounter += charactersPerSecond * delta

	var totalCharacters := textLabel.get_total_character_count()

	var visibleCharacters := mini(int(characterCounter), totalCharacters)

	textLabel.visible_characters = visibleCharacters

	# 当前文字全部打完
	if visibleCharacters >= totalCharacters:
		textFinished = true

		continueHint.visible = true


# =========================================================
# 任意键继续
# =========================================================

func _input(event: InputEvent) -> void:
	if not active:
		return

	var pressed := false

	# 键盘任意键
	if event is InputEventKey:
		if event.pressed and not event.echo:
			pressed = true
	# 鼠标任意按键

	elif event is InputEventMouseButton:
		if event.pressed:
			pressed = true
	# 手柄按钮，顺手也支持

	elif event is InputEventJoypadButton:
		if event.pressed:
			pressed = true

	if not pressed:
		return

	# 防止这次输入继续传给暂停菜单等其他东西
	get_viewport().set_input_as_handled()

	# 文字还没打完，不翻页
	if not textFinished:
		return

	NextMessage()


# =========================================================
# 下一句
# =========================================================

func NextMessage() -> void:
	currentIndex += 1

	# 还有下一句
	if currentIndex < messages.size():
		StartCurrentMessage()
		return

	# 全部结束
	FinishTutorial()


# =========================================================
# 教程结束
# =========================================================

func FinishTutorial() -> void:
	active = false

	root.visible = false
	continueHint.visible = false

	# 恢复教程出现之前的暂停状态
	get_tree().paused = previousPausedState
