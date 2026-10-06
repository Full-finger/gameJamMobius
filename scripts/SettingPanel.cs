using Godot;

public partial class SettingPanel : Control
{
	[ExportGroup("设置滑杆")]
	[Export] public HSlider volumeSlider;
	[Export] public HSlider brightnessSlider;

	public override void _Ready()
	{
		// 设置范围
		volumeSlider.MinValue = 0;
		volumeSlider.MaxValue = 1;
		volumeSlider.Step = 0.01;

		brightnessSlider.MinValue = 0;
		brightnessSlider.MaxValue = 1;
		brightnessSlider.Step = 0.01;


		// 读取全局设置
		volumeSlider.Value =
			SettingsManager.Instance.Volume;

		brightnessSlider.Value =
			SettingsManager.Instance.Brightness;


		// 监听拖动
		volumeSlider.ValueChanged += OnVolumeChanged;

		brightnessSlider.ValueChanged += OnBrightnessChanged;
	}


	private void OnVolumeChanged(double value)
	{
		SettingsManager.Instance.SetVolume(
			(float)value
		);
	}


	private void OnBrightnessChanged(double value)
	{
		SettingsManager.Instance.SetBrightness(
			(float)value
		);
	}
}