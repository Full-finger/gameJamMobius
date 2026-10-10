extends CanvasLayer

# Compatibility 渲染器（Web 平台）的 2D 画面整体比 Forward+ 偏亮
# （Compatibility 在 sRGB 空间混合，Forward+ 走线性 + HDR 2D）。
# 实测 level.tscn 画面平均亮度 0.164 vs Forward+ 0.108，主菜单同样偏亮。
# 这里加一层全屏黑罩把画面乘回接近 Forward+ 的观感。
# 桌面（Forward+）不做任何处理。

const DARKEN_ALPHA := 0.34


func _ready() -> void:
	if RenderingServer.get_current_rendering_method() != "gl_compatibility":
		return

	layer = 60

	var rect := ColorRect.new()
	rect.color = Color(0.0, 0.0, 0.0, DARKEN_ALPHA)
	rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(rect)
	rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
