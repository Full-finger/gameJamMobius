using Godot;
using System;

public partial class Leaves : StaticBody2D
{
	// Called when the node enters the scene tree for the first time.
	private bool triggered = false;

    public override void _Ready()
    {
        GetNode<Area2D>("StepArea").BodyEntered += OnBodyEntered;
    }

    private async void OnBodyEntered(Node2D body)
    {
        if (triggered || body is not Player player)
            return;

        // 从下面向上跳过时，不开始倒计时。
        if (player.Velocity.Y < 0f)
            return;

        triggered = true;

        // 等两秒；暂停游戏时，倒计时也暂停。
        await ToSignal(
            GetTree().CreateTimer(2.0, processAlways: false),
            SceneTreeTimer.SignalName.Timeout
        );

        QueueFree();
    }
}
