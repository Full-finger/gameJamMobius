extends Control

# C# SettingPanel 的 GDScript 版（Web 导出用）

@export_group("设置滑杆")
@export var volumeSlider: HSlider
@export var brightnessSlider: HSlider


func _ready() -> void:
	# 设置范围
	volumeSlider.min_value = 0.0
	volumeSlider.max_value = 1.0
	volumeSlider.step = 0.01

	brightnessSlider.min_value = 0.0
	brightnessSlider.max_value = 1.0
	brightnessSlider.step = 0.01

	# 读取全局设置
	volumeSlider.value = SettingsManager.Volume
	brightnessSlider.value = SettingsManager.Brightness

	# 监听拖动
	volumeSlider.value_changed.connect(OnVolumeChanged)
	brightnessSlider.value_changed.connect(OnBrightnessChanged)


func OnVolumeChanged(value: float) -> void:
	SettingsManager.SetVolume(value)


func OnBrightnessChanged(value: float) -> void:
	SettingsManager.SetBrightness(value)
