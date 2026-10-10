class_name Button_ReturnToMain
extends TextureButton

# C# Button_ReturnToMain 的 GDScript 版（Web 导出用）

signal BackRequested

var returning := false


func _ready() -> void:
	pivot_offset = size / 2.0
	resized.connect(func() -> void: pivot_offset = size / 2.0)
	pressed.connect(PlayReturnAnimation)


# 点击和 Esc 共用这条流程，动画播完再通知设置页返回。
func PlayReturnAnimation() -> void:
	if returning:
		return

	returning = true
	disabled = true

	var startRotation := rotation

	var tween := create_tween()
	tween.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
	tween.set_trans(Tween.TRANS_SINE)
	tween.set_ease(Tween.EASE_IN_OUT)

	tween.tween_property(self, "rotation", startRotation + deg_to_rad(4.0), 0.04)
	tween.tween_property(self, "rotation", startRotation - deg_to_rad(4.0), 0.06)
	tween.tween_property(self, "rotation", startRotation + deg_to_rad(2.0), 0.05)
	tween.tween_property(self, "rotation", startRotation, 0.05)

	await tween.finished

	BackRequested.emit()
