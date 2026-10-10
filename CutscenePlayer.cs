using Godot;

public partial class CutscenePlayer : Control
{
    [ExportGroup("节点")]
    [Export] public TextureRect cgDisplay;
    [Export] public Control dialogueBox;
    [Export] public RichTextLabel textLabel;
    [Export] public Control continueHint;

    [ExportGroup("剧情")]
    [Export]
    public Godot.Collections.Array<CutsceneStep> steps = new();

    [ExportGroup("打字机")]
    [Export] public float charactersPerSecond = 35f;

    [ExportGroup("下一场景")]
    [Export(PropertyHint.File, "*.tscn")]
    public string nextScene;

    [ExportGroup("结尾黑屏文字")]
    [Export(PropertyHint.MultilineText)]
    public string finalBlackoutText;


    private int currentStep = 0;

    private float characterCounter = 0f;

    private bool textFinished = false;

    private bool finished = false;

    private bool showingFinalBlackout = false;

    private ColorRect blackScreen;

    private Label finalTextLabel;


    public override void _Ready()
    {
        continueHint.Visible = false;

        if (steps.Count == 0)
        {
            FinishCutscene();
            return;
        }

        ShowStep(0);
    }


    // =========================================================
    // 显示某一步
    // =========================================================

    private void ShowStep(int index)
    {
        if (index < 0 || index >= steps.Count)
            return;

        CutsceneStep step = steps[index];

        // -----------------------------------------
        // CG
        // -----------------------------------------

        // 没填图片就继续保持上一张
      if (step.cg != null)
{
    cgDisplay.Texture = step.cg;

    cgDisplay.Modulate =
        new Color(1f, 1f, 1f, 0f);

    Tween tween = CreateTween();

    tween.TweenProperty(
        cgDisplay,
        "modulate:a",
        1f,
        0.25f
    )
    .SetTrans(Tween.TransitionType.Quad)
    .SetEase(Tween.EaseType.Out);
}

        // -----------------------------------------
        // 对话
        // -----------------------------------------

        textLabel.Text = step.text;

        characterCounter = 0f;
        textLabel.VisibleCharacters = 0;

        textFinished = false;
        continueHint.Visible = false;


        // 如果这一 Step 根本没文字
        // 就直接认为文字显示完成
        if (string.IsNullOrEmpty(step.text))
        {
            textFinished = true;
            continueHint.Visible = true;
        }
    }


    // =========================================================
    // 打字机
    // =========================================================

    public override void _Process(double delta)
    {
        if (finished || textFinished)
            return;

        characterCounter +=
            charactersPerSecond * (float)delta;

        int total = GetActiveCharacterCount();

        int visible =
            Mathf.Min(
                (int)characterCounter,
                total
            );

        SetActiveVisibleCharacters(visible);

        if (visible >= total)
        {
            textFinished = true;

            continueHint.Visible = true;
        }
    }


    // =========================================================
    // 键盘 / 鼠标继续
    // =========================================================

    public override void _Input(InputEvent @event)
    {
        if (finished)
            return;

        bool pressed = false;


        // 键盘
        if (@event is InputEventKey key)
        {
            if (key.Pressed && !key.Echo)
                pressed = true;
        }


        // 鼠标
        else if (@event is InputEventMouseButton mouse)
        {
            if (mouse.Pressed)
                pressed = true;
        }


        // 手柄
        else if (@event is InputEventJoypadButton joypad)
        {
            if (joypad.Pressed)
                pressed = true;
        }


        if (!pressed)
            return;


        GetViewport().SetInputAsHandled();


        // =====================================================
        // 当前文字还没打完：
        // 第一次点击直接全部显示
        // =====================================================

        if (!textFinished)
        {
            SetActiveVisibleCharacters(
                GetActiveCharacterCount()
            );

            textFinished = true;

            continueHint.Visible = true;

            return;
        }


        // =====================================================
        // 已经打完：
        // 下一 Step
        // =====================================================

        NextStep();
    }


    // =========================================================
    // 下一步
    // =========================================================

    private void NextStep()
    {
        currentStep++;

        if (currentStep >= steps.Count)
        {
            if (
                !showingFinalBlackout &&
                !string.IsNullOrEmpty(finalBlackoutText)
            )
            {
                ShowFinalBlackout();
                return;
            }

            FinishCutscene();
            return;
        }

        ShowStep(currentStep);
    }


    // =========================================================
    // 结尾黑屏
    // =========================================================

    private void ShowFinalBlackout()
    {
        showingFinalBlackout = true;

        cgDisplay.Hide();
        dialogueBox.Hide();

        blackScreen = new ColorRect();
        blackScreen.Color = Colors.Black;
        blackScreen.MouseFilter = Control.MouseFilterEnum.Ignore;
        blackScreen.ZIndex = 100;
        AddChild(blackScreen);
        blackScreen.SetAnchorsAndOffsetsPreset(
            Control.LayoutPreset.FullRect
        );

        finalTextLabel = new Label();
        finalTextLabel.Text = finalBlackoutText;
        finalTextLabel.HorizontalAlignment =
            HorizontalAlignment.Center;
        finalTextLabel.VerticalAlignment =
            VerticalAlignment.Center;
        finalTextLabel.AddThemeColorOverride(
            "font_color",
            Colors.White
        );
        finalTextLabel.AddThemeFontSizeOverride(
            "font_size",
            56
        );

        Font font = textLabel.GetThemeDefaultFont();
        if (font != null)
            finalTextLabel.AddThemeFontOverride("font", font);

        finalTextLabel.ZIndex = 101;
        finalTextLabel.MouseFilter =
            Control.MouseFilterEnum.Ignore;
        AddChild(finalTextLabel);
        finalTextLabel.SetAnchorsAndOffsetsPreset(
            Control.LayoutPreset.FullRect
        );

        characterCounter = 0f;
        finalTextLabel.VisibleCharacters = 0;
        textFinished = false;
        continueHint.Visible = false;
    }


    private int GetActiveCharacterCount()
    {
        if (showingFinalBlackout)
            return finalTextLabel.GetTotalCharacterCount();

        return textLabel.GetTotalCharacterCount();
    }


    private void SetActiveVisibleCharacters(int count)
    {
        if (showingFinalBlackout)
        {
            finalTextLabel.VisibleCharacters = count;
            return;
        }

        textLabel.VisibleCharacters = count;
    }


    // =========================================================
    // CG结束
    // =========================================================

    private void FinishCutscene()
    {
        if (finished)
            return;

        finished = true;

        // 使用你刚刚做的全局黑屏转场
        if (!string.IsNullOrEmpty(nextScene))
        {
            TransitionManager.Instance.TransitionTo(
                nextScene,
                0.6f,
                0.6f
            );
        }
    }
}
