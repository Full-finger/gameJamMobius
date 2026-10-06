using Godot;

public partial class GrabArea : Area2D
{
    public override void _PhysicsProcess(double delta)
    {
        // 跳离冷却结束后，即使仍在范围里也能再次抓住。
        foreach (Node2D body in GetOverlappingBodies())
        {
            if (body is Player player)
            {
                RigidBody2D target = GetParent<RigidBody2D>();
                Vector2 center = player.GetNode<CollisionShape2D>("CollisionShape2D").GlobalPosition;
                float distance = center.DistanceSquaredTo(target.GlobalPosition);
                bool nearest = true;
                foreach (Node node in target.GetParent().GetChildren())
                {
                    if (node is RigidBody2D other && other != target &&
                        other.GetNode<Area2D>("GrabArea").OverlapsBody(player) &&
                        center.DistanceSquaredTo(other.GlobalPosition) < distance)
                    {
                        nearest = false;
                        break;
                    }
                }
                if (nearest)
                    player.GrabVine(target);
            }
        }
    }
}
