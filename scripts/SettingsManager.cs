using Godot;

public partial class SettingsManager : CanvasLayer
{
    public static SettingsManager Instance { get; private set; }

    // 当前设置
    public float Volume { get; private set; } = 1f;
    public float Brightness { get; private set; } = 1f;

    private ColorRect brightnessOverlay;

    private const string SavePath = "user://settings.cfg";

    public override void _Ready()
    {
        Instance = this;

        // 保证亮度遮罩永远在最上层
        Layer = 100;

        CreateBrightnessOverlay();

        LoadSettings();

        ApplyVolume();
        ApplyBrightness();
    }


    // =========================================================
    // 创建全屏亮度遮罩
    // =========================================================

    private void CreateBrightnessOverlay()
    {
        brightnessOverlay = new ColorRect();

        AddChild(brightnessOverlay);

        brightnessOverlay.SetAnchorsPreset(
            Control.LayoutPreset.FullRect
        );

        brightnessOverlay.OffsetLeft = 0;
        brightnessOverlay.OffsetTop = 0;
        brightnessOverlay.OffsetRight = 0;
        brightnessOverlay.OffsetBottom = 0;

        // 黑色遮罩
        brightnessOverlay.Color = Colors.Black;

        // 绝对不能挡住鼠标
        brightnessOverlay.MouseFilter =
            Control.MouseFilterEnum.Ignore;
    }


    // =========================================================
    // 设置音量
    // =========================================================

    public void SetVolume(float value)
    {
        Volume = Mathf.Clamp(value, 0f, 1f);

        ApplyVolume();

        SaveSettings();
    }

    private void ApplyVolume()
    {
        int masterBus = AudioServer.GetBusIndex("Master");

        if (masterBus == -1)
            return;

        AudioServer.SetBusVolumeLinear(
            masterBus,
            Volume
        );
    }


    // =========================================================
    // 设置亮度
    // =========================================================

    public void SetBrightness(float value)
    {
        Brightness = Mathf.Clamp(value, 0f, 1f);

        ApplyBrightness();

        SaveSettings();
    }

    private void ApplyBrightness()
    {
        if (brightnessOverlay == null)
            return;

        // Brightness = 1
        // 完全没有黑色遮罩
        //
        // Brightness = 0
        // 最暗时黑色透明度 80%

        float darkness = (1f - Brightness) * 0.8f;

        brightnessOverlay.Color =
            new Color(0f, 0f, 0f, darkness);
    }


    // =========================================================
    // 保存设置
    // =========================================================

    private void SaveSettings()
    {
        ConfigFile config = new ConfigFile();

        config.SetValue(
            "settings",
            "volume",
            Volume
        );

        config.SetValue(
            "settings",
            "brightness",
            Brightness
        );

        config.Save(SavePath);
    }


    // =========================================================
    // 读取设置
    // =========================================================

    private void LoadSettings()
    {
        ConfigFile config = new ConfigFile();

        Error error = config.Load(SavePath);

        if (error != Error.Ok)
            return;

        Volume = (float)config.GetValue(
            "settings",
            "volume",
            1f
        );

        Brightness = (float)config.GetValue(
            "settings",
            "brightness",
            1f
        );
    }
}