using Godot;

public partial class Leaves : StaticBody2D
{
    private bool triggered = false;

    [ExportGroup("节点")]
    [Export] public CanvasItem leafVisual;
    [Export] public CollisionShape2D collision;
    [Export] public Area2D stepArea;
    [Export] public GpuParticles2D leafParticles;

    [ExportGroup("时间")]
    [Export] public float disappearDelay = 2f;
    [Export] public float particleWaitTime = 1f;


    public override void _Ready()
    {
        stepArea.BodyEntered += OnBodyEntered;

        // 确保刚开始不会喷粒子
        leafParticles.Emitting = false;
    }


    private async void OnBodyEntered(Node2D body)
    {
        if (triggered || body is not Player player)
            return;


        // 从下面向上穿过时不触发
        if (player.Velocity.Y < 0f)
            return;


        triggered = true;


        // ==========================================
        // 1. 等两秒
        // 暂停游戏时这个 Timer 也暂停
        // ==========================================

        await ToSignal(
            GetTree().CreateTimer(
                disappearDelay,
                processAlways: false
            ),
            SceneTreeTimer.SignalName.Timeout
        );


        // ==========================================
        // 2. 树叶本体消失
        // ==========================================

        leafVisual.Visible = false;

        collision.SetDeferred(
            CollisionShape2D.PropertyName.Disabled,
            true
        );

        stepArea.SetDeferred(
            Area2D.PropertyName.Monitoring,
            false
        );


        // ==========================================
        // 3. 播放叶片散落粒子
        // ==========================================

        leafParticles.Restart();
        leafParticles.Emitting = true;


        // ==========================================
        // 4. 给粒子一点时间播完
        // ==========================================

        await ToSignal(
            GetTree().CreateTimer(
                particleWaitTime,
                processAlways: false
            ),
            SceneTreeTimer.SignalName.Timeout
        );


        // ==========================================
        // 5. 整个树叶节点删除
        // ==========================================

        QueueFree();
    }
}