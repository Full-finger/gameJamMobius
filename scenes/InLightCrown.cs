using Godot;
using System;

public partial class InLightCrown : TextureRect
{
	[Export] public Player player;

	public override void _Process(double delta)
	{
		if (player == null)
			return;

		Visible = Mathf.IsEqualApprox(player.LifeRatio, 1f);
	}
}
