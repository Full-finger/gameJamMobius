extends Control

# C# MenuButton 的 GDScript 版（Web 导出用）

@export_group("节点")
@export var button: Button
@export var visualRoot: Control
@export var hover: TextureRect
@export var click: TextureRect

@export_group("Hover")
@export var hoverRotation := -1.5
@export var hoverScale := 1.02
@export var hoverTime := 0.12

@export_group("点击震动")
@export var shakeAmount := 5.0
@export var shakeTime := 0.025

@export_group("Click图案")
@export var clickStartXScale := 0.1
@export var clickTime := 0.22

var hoverTween: Tween
var clickTween: Tween

# 原始状态
var normalPosition: Vector2
var normalVisualScale: Vector2
var normalClickScale: Vector2
var normalRotation: float


func _ready() -> void:
	# 记录 Inspector 里原本设置好的值
	normalPosition = visualRoot.position
	normalVisualScale = visualRoot.scale
	normalClickScale = click.scale
	normalRotation = visualRoot.rotation_degrees

	# VisualRoot 围绕中心缩放/旋转
	visualRoot.pivot_offset = visualRoot.size / 2.0

	# Click 围绕自身中心横向展开
	click.pivot_offset = click.size / 2.0

	# Hover 只隐藏，不改变它的 Scale
	hover.modulate = Color(
		hover.modulate.r,
		hover.modulate.g,
		hover.modulate.b,
		0.0
	)

	# Click 初始隐藏
	click.modulate = Color(
		click.modulate.r,
		click.modulate.g,
		click.modulate.b,
		0.0
	)

	# Click 保持原 Y Scale，只把 X 压窄
	click.scale = Vector2(
		normalClickScale.x * clickStartXScale,
		normalClickScale.y
	)

	button.mouse_entered.connect(OnMouseEntered)
	button.mouse_exited.connect(OnMouseExited)
	button.pressed.connect(OnPressed)


func OnMouseEntered() -> void:
	if clickTween != null and clickTween.is_running():
		return

	if hoverTween != null:
		hoverTween.kill()

	hoverTween = create_tween()
	hoverTween.set_parallel(true)

	# 在原本角度基础上轻微歪斜
	hoverTween.tween_property(visualRoot, "rotation_degrees", normalRotation + hoverRotation, hoverTime
	).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)

	# 在原本大小基础上轻微放大
	hoverTween.tween_property(visualRoot, "scale", normalVisualScale * hoverScale, hoverTime
	).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)

	# Hover 只淡入
	hoverTween.tween_property(hover, "modulate:a", 1.0, hoverTime
	).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)


func OnMouseExited() -> void:
	if clickTween != null and clickTween.is_running():
		return

	if hoverTween != null:
		hoverTween.kill()

	hoverTween = create_tween()
	hoverTween.set_parallel(true)

	# 恢复原旋转
	hoverTween.tween_property(visualRoot, "rotation_degrees", normalRotation, hoverTime
	).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)

	# 恢复原 Scale
	hoverTween.tween_property(visualRoot, "scale", normalVisualScale, hoverTime
	).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)

	# Hover 淡出
	hoverTween.tween_property(hover, "modulate:a", 0.0, hoverTime
	).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)


func OnPressed() -> void:
	PlayClick()


func PlayClick() -> void:
	if hoverTween != null:
		hoverTween.kill()

	if clickTween != null:
		clickTween.kill()

	button.disabled = true

	clickTween = create_tween()

	# 先左右震几下
	clickTween.tween_property(
		visualRoot,
		"position",
		normalPosition + Vector2(-shakeAmount, 0.0),
		shakeTime
	)

	clickTween.tween_property(
		visualRoot,
		"position",
		normalPosition + Vector2(shakeAmount, 0.0),
		shakeTime
	)

	clickTween.tween_property(
		visualRoot,
		"position",
		normalPosition + Vector2(-shakeAmount * 0.5, 0.0),
		shakeTime
	)

	clickTween.tween_property(
		visualRoot,
		"position",
		normalPosition,
		shakeTime
	)

	# 准备 Click 图片
	clickTween.tween_callback(func() -> void:
		click.modulate = Color(
			click.modulate.r,
			click.modulate.g,
			click.modulate.b,
			1.0
		)

		click.scale = Vector2(
			normalClickScale.x * clickStartXScale,
			normalClickScale.y
		)
	)

	# 从中间横向展开到它自己原本的 Scale
	clickTween.tween_property(click, "scale", normalClickScale, clickTime
	).set_trans(Tween.TRANS_QUART).set_ease(Tween.EASE_OUT)

	# 动画结束后执行按钮功能
	clickTween.tween_callback(ExecuteButton)


func ExecuteButton() -> void:
	print("按钮功能触发")
