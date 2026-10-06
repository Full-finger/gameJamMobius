using Godot;

public partial class HUD : CanvasLayer
{
	[Export] public Player player;
	[Export] public TextureProgressBar lifeBar;

	public override void _Ready()
	{
		lifeBar.MinValue = 0f;
		lifeBar.MaxValue = 1f;
		lifeBar.Value = 1f;
	}

	public override void _Process(double delta)
	{
		if (player == null)
			return;

		lifeBar.Value = player.LifeRatio;
	}
}
