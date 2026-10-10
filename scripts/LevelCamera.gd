class_name LevelCamera
extends Camera2D

# C# LevelCamera 的 GDScript 版（Web 导出用）

var normalZoom: Vector2


func _ready() -> void:
	normalZoom = zoom

	get_viewport().size_changed.connect(FitInsideBackground)

	FitInsideBackground()


# 每关的 Bounds 根据该关背景设置限制，同一个 Player 可以用于不同大小的地图。
func SetBackgroundBounds(bounds: Rect2) -> void:
	limit_left = ceili(bounds.position.x)
	limit_top = ceili(bounds.position.y)
	limit_right = floori(bounds.end.x)
	limit_bottom = floori(bounds.end.y)

	FitInsideBackground()


func FitInsideBackground() -> void:
	var screen := get_viewport_rect().size
	var width := limit_right - limit_left
	var height := limit_bottom - limit_top

	# 窗口太宽或太高时，适当拉近镜头，保证视野放得进背景。
	var multiplier: float = maxf(
		1.0,
		maxf(screen.x / (width * normalZoom.x), screen.y / (height * normalZoom.y))
	)

	zoom = normalZoom * multiplier

	reset_smoothing()
	force_update_scroll()


func _exit_tree() -> void:
	get_viewport().size_changed.disconnect(FitInsideBackground)
