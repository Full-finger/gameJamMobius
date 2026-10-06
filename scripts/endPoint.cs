using Godot;

public partial class endPoint : Area2D
{
	[Export] public string nextScenePath = "res://scenes/main_menu.tscn";

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player)
		{
			GetTree().CallDeferred(
				SceneTree.MethodName.ChangeSceneToFile,
				nextScenePath
			);
		}
	}
}