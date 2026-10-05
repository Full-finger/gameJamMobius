using Godot;

public partial class Light : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
			player.isInLight = true;
	}

	private void OnBodyExited(Node2D body)
	{
		if (body is Player player)
			player.isInLight = false;
	}
}