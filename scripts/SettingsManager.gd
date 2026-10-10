extends CanvasLayer

# C# SettingsManager 的 GDScript 版（Web 导出用）
# autoload 单例：直接用 SettingsManager.XXX 访问

const SavePath := "user://settings.cfg"

# 当前设置
var Volume := 1.0
var Brightness := 1.0

var brightnessOverlay: ColorRect


func _ready() -> void:
	# 保证亮度遮罩永远在最上层
	layer = 100

	CreateBrightnessOverlay()

	LoadSettings()

	ApplyVolume()
	ApplyBrightness()


# =========================================================
# 创建全屏亮度遮罩
# =========================================================

func CreateBrightnessOverlay() -> void:
	brightnessOverlay = ColorRect.new()

	add_child(brightnessOverlay)

	brightnessOverlay.set_anchors_preset(
		Control.PRESET_FULL_RECT
	)

	brightnessOverlay.offset_left = 0
	brightnessOverlay.offset_top = 0
	brightnessOverlay.offset_right = 0
	brightnessOverlay.offset_bottom = 0

	# 黑色遮罩
	brightnessOverlay.color = Color.BLACK

	# 绝对不能挡住鼠标
	brightnessOverlay.mouse_filter = Control.MOUSE_FILTER_IGNORE


# =========================================================
# 设置音量
# =========================================================

func SetVolume(value: float) -> void:
	Volume = clampf(value, 0.0, 1.0)

	ApplyVolume()

	SaveSettings()


func ApplyVolume() -> void:
	var masterBus := AudioServer.get_bus_index("Master")

	if masterBus == -1:
		return

	AudioServer.set_bus_volume_linear(
		masterBus,
		Volume
	)


# =========================================================
# 设置亮度
# =========================================================

func SetBrightness(value: float) -> void:
	Brightness = clampf(value, 0.0, 1.0)

	ApplyBrightness()

	SaveSettings()


func ApplyBrightness() -> void:
	if brightnessOverlay == null:
		return

	# Brightness = 1 完全没有黑色遮罩
	# Brightness = 0 最暗时黑色透明度 80%

	var darkness := (1.0 - Brightness) * 0.8

	brightnessOverlay.color = Color(0.0, 0.0, 0.0, darkness)


# =========================================================
# 保存设置
# =========================================================

func SaveSettings() -> void:
	var config := ConfigFile.new()

	config.set_value("settings", "volume", Volume)
	config.set_value("settings", "brightness", Brightness)

	config.save(SavePath)


# =========================================================
# 读取设置
# =========================================================

func LoadSettings() -> void:
	var config := ConfigFile.new()

	var error := config.load(SavePath)

	if error != OK:
		return

	Volume = float(config.get_value("settings", "volume", 1.0))
	Brightness = float(config.get_value("settings", "brightness", 1.0))
