using Godot;

public partial class Button_start : Button
{
    public override void _Ready()
    {
        Pressed += StartGame;
    }

    private async void StartGame()
    {
        // 防止这 0.7 秒里重复点击
        Disabled = true;

        // 等待 0.7 秒，让动画播完
        await ToSignal(
            GetTree().CreateTimer(0.7),
            SceneTreeTimer.SignalName.Timeout
        );
        TransitionManager.Instance.TransitionTo("res://scenes/JUMPSCENES/CutScene_IN.tscn");
    }
}