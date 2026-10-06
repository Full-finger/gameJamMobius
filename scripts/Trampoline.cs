using Godot;

// 落到荷叶上时自动弹起；从下方跳过时不触发。
public partial class Trampoline : Area2D
{
    [Export] public float launchSpeed = 1700f;

    public override void _PhysicsProcess(double delta)
    {
        foreach (Node2D body in GetOverlappingBodies())
        {
            if (body is not Player player || !player.IsPhysicsProcessing() || player.Velocity.Y < 0f)
                continue;

            var playerShape = player.GetNode<CollisionShape2D>("CollisionShape2D");
            float feetY = playerShape.GlobalPosition.Y
                + ((RectangleShape2D)playerShape.Shape).Size.Y * playerShape.GlobalScale.Y * 0.5f;
            var areaShape = GetNode<CollisionShape2D>("CollisionShape2D");
            float areaBottom = areaShape.GlobalPosition.Y
                + ((RectangleShape2D)areaShape.Shape).Size.Y * areaShape.GlobalScale.Y * 0.5f;

            // 脚已低于弹跳区时属于侧面擦过，不要把玩家突然弹上去。
            if (feetY <= areaBottom + 4f)
                player.Launch(launchSpeed);
        }
    }
}
