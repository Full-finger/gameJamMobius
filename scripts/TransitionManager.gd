extends CanvasLayer

# C# TransitionManager 的 GDScript 版（Web 导出用）
# autoload 单例：直接用 TransitionManager.XXX 访问

var blackScreen: ColorRect

var transitioning := false


func _ready() -> void:
	# 即使游戏暂停也能播放转场
	process_mode = Node.PROCESS_MODE_ALWAYS

	# 保证压在所有 HUD / UI / 游戏画面上
	layer = 999

	# =========================
	# 创建黑色全屏遮罩
	# =========================

	blackScreen = ColorRect.new()

	blackScreen.color = Color.BLACK

	add_child(blackScreen)

	blackScreen.set_anchors_and_offsets_preset(
		Control.PRESET_FULL_RECT
	)

	# 游戏刚启动时先黑屏
	blackScreen.modulate = Color(1.0, 1.0, 1.0, 1.0)

	# 转场期间挡住鼠标
	blackScreen.mouse_filter = Control.MOUSE_FILTER_STOP

	# 第一次进入游戏时：黑 → 亮
	InitialFadeIn()


# =========================================================
# 游戏启动时淡入
# =========================================================

func InitialFadeIn() -> void:
	transitioning = true

	await FadeTo(0.0, 0.5)

	blackScreen.mouse_filter = Control.MOUSE_FILTER_IGNORE

	transitioning = false


# =========================================================
# 对外使用：切换场景
# =========================================================

func TransitionTo(
	scenePath: String,
	fadeOutTime := 0.4,
	fadeInTime := 0.4
) -> void:
	if transitioning:
		return

	transitioning = true

	# 转场期间不允许玩家乱点东西
	blackScreen.mouse_filter = Control.MOUSE_FILTER_STOP

	# =========================
	# 1. 当前画面 → 黑
	# =========================

	await FadeTo(1.0, fadeOutTime)

	# =========================
	# 2. 黑屏时切场景
	# =========================

	# 防止从暂停菜单切场景以后，新场景还保持暂停
	get_tree().paused = false

	var error := get_tree().change_scene_to_file(scenePath)

	if error != OK:
		push_error("场景切换失败：%s" % scenePath)

		await FadeTo(0.0, fadeInTime)

		blackScreen.mouse_filter = Control.MOUSE_FILTER_IGNORE

		transitioning = false

		return

	# 等新场景真正进入 SceneTree
	await get_tree().process_frame

	# =========================
	# 3. 黑 → 新场景
	# =========================

	await FadeTo(0.0, fadeInTime)

	blackScreen.mouse_filter = Control.MOUSE_FILTER_IGNORE

	transitioning = false


# =========================================================
# 重开当前场景
# =========================================================

func ReloadCurrentScene() -> void:
	if get_tree().current_scene == null:
		return

	var scenePath: String = get_tree().current_scene.scene_file_path

	TransitionTo(scenePath)


# =========================================================
# 实际 Tween
# =========================================================

func FadeTo(alpha: float, duration: float) -> void:
	var tween := create_tween()

	tween.tween_property(
		blackScreen,
		"modulate:a",
		alpha,
		duration
	).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN_OUT)

	await tween.finished
