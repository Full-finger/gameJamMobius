using Godot;

public partial class NormalEndingArea : Area2D
{
	[Export(PropertyHint.File, "*.tscn")]
	public string endingScene =
		"res://scenes/normal_ending.tscn";

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player)
			return;

		GetTree().ChangeSceneToFile(endingScene);
	}
}