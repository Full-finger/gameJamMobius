using Godot;

/// 蹦床：玩家碰到弹起区时获得一个向上的大速度。
public partial class Trampoline : Area2D
{
	[Export] public float launchSpeed = 1700f;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
			player.Launch(launchSpeed);
	}
}
