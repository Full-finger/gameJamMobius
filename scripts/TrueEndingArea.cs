using Godot;

public partial class TrueEndingArea : Area2D
{
    [Export(PropertyHint.File, "*.tscn")]
    public string trueEndingScene =
        "res://scenes/true_ending.tscn";

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;

        bool unlocked = GameState.Instance.AllCollected;

        Visible = unlocked;
        Monitoring = unlocked;
        Monitorable = unlocked;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not Player)
            return;

        TransitionManager.Instance.TransitionTo(trueEndingScene);
    }
}