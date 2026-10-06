using Godot;
using System.Threading.Tasks;

public partial class TransitionManager : CanvasLayer
{
    public static TransitionManager Instance { get; private set; }

    private ColorRect blackScreen;

    private bool transitioning = false;

    public override void _Ready()
    {
        Instance = this;

        // 即使游戏暂停也能播放转场
        ProcessMode = ProcessModeEnum.Always;

        // 保证压在所有 HUD / UI / 游戏画面上
        Layer = 999;

        // =========================
        // 创建黑色全屏遮罩
        // =========================

        blackScreen = new ColorRect();

        blackScreen.Color = Colors.Black;

        AddChild(blackScreen);

        blackScreen.SetAnchorsAndOffsetsPreset(
            Control.LayoutPreset.FullRect
        );

        // 游戏刚启动时先黑屏
        blackScreen.Modulate =
            new Color(1f, 1f, 1f, 1f);

        // 转场期间挡住鼠标
        blackScreen.MouseFilter =
            Control.MouseFilterEnum.Stop;

        // 第一次进入游戏时：
        // 黑 → 亮
        InitialFadeIn();
    }


    // =========================================================
    // 游戏启动时淡入
    // =========================================================

    private async void InitialFadeIn()
    {
        transitioning = true;

        await FadeTo(0f, 0.5f);

        blackScreen.MouseFilter =
            Control.MouseFilterEnum.Ignore;

        transitioning = false;
    }


    // =========================================================
    // 对外使用：切换场景
    // =========================================================

    public async void TransitionTo(
        string scenePath,
        float fadeOutTime = 0.4f,
        float fadeInTime = 0.4f
    )
    {
        if (transitioning)
            return;

        transitioning = true;

        // 转场期间不允许玩家乱点东西
        blackScreen.MouseFilter =
            Control.MouseFilterEnum.Stop;


        // =========================
        // 1. 当前画面 → 黑
        // =========================

        await FadeTo(
            1f,
            fadeOutTime
        );


        // =========================
        // 2. 黑屏时切场景
        // =========================

        // 防止从暂停菜单切场景以后，
        // 新场景还保持暂停
        GetTree().Paused = false;

        Error error =
            GetTree().ChangeSceneToFile(scenePath);

        if (error != Error.Ok)
        {
            GD.PrintErr(
                $"场景切换失败：{scenePath}"
            );

            await FadeTo(0f, fadeInTime);

            blackScreen.MouseFilter =
                Control.MouseFilterEnum.Ignore;

            transitioning = false;

            return;
        }


        // 等新场景真正进入 SceneTree
        await ToSignal(
            GetTree(),
            SceneTree.SignalName.ProcessFrame
        );


        // =========================
        // 3. 黑 → 新场景
        // =========================

        await FadeTo(
            0f,
            fadeInTime
        );


        blackScreen.MouseFilter =
            Control.MouseFilterEnum.Ignore;

        transitioning = false;
    }


    // =========================================================
    // 重开当前场景
    // =========================================================

    public void ReloadCurrentScene()
    {
        if (GetTree().CurrentScene == null)
            return;

        string scenePath =
            GetTree().CurrentScene.SceneFilePath;

        TransitionTo(scenePath);
    }


    // =========================================================
    // 实际 Tween
    // =========================================================

    private async Task FadeTo(
        float alpha,
        float duration
    )
    {
        Tween tween = CreateTween();

        tween.TweenProperty(
            blackScreen,
            "modulate:a",
            alpha,
            duration
        )
        .SetTrans(Tween.TransitionType.Quad)
        .SetEase(Tween.EaseType.InOut);

        await ToSignal(
            tween,
            Tween.SignalName.Finished
        );
    }
}