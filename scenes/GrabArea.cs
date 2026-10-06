using Godot;

public partial class GrabArea : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            player.GrabVine(GetParent<RigidBody2D>());
        }
    }
}