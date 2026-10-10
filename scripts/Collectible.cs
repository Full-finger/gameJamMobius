using Godot;

public partial class Collectible : Area2D
{
    [Export(PropertyHint.Range, "1,3,1")]
    public int level = 1;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;

   
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not Player player || !player.IsPhysicsProcessing())
            return;

        GameState.Instance.Collect(level);

        QueueFree();
    }
}