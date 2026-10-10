using Godot;

public partial class Button_ReturnToMain : TextureButton
{
    [Signal]
    public delegate void BackRequestedEventHandler();

    private bool returning;

    public override void _Ready()
    {
        PivotOffset = Size / 2f;
        Resized += () => PivotOffset = Size / 2f;
        Pressed += PlayReturnAnimation;
    }

    // 点击和 Esc 共用这条流程，动画播完再通知设置页返回。
    public async void PlayReturnAnimation()
    {
        if (returning)
            return;

        returning = true;
        Disabled = true;
        float startRotation = Rotation;

        Tween tween = CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.SetTrans(Tween.TransitionType.Sine);
        tween.SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(this, "rotation", startRotation + Mathf.DegToRad(4f), 0.04f);
        tween.TweenProperty(this, "rotation", startRotation - Mathf.DegToRad(4f), 0.06f);
        tween.TweenProperty(this, "rotation", startRotation + Mathf.DegToRad(2f), 0.05f);
        tween.TweenProperty(this, "rotation", startRotation, 0.05f);

        await ToSignal(tween, Tween.SignalName.Finished);
        EmitSignal(SignalName.BackRequested);
    }
}
