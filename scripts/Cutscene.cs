using Godot;

public partial class Cutscene : Control
{
	[Export] public AnimatedSprite2D animation;

	[Export(PropertyHint.File, "*.tscn")]
	public string nextScene;

	public override void _Ready()
	{
		animation.SpriteFrames.SetAnimationLoopMode(
			animation.Animation,
			SpriteFrames.LoopMode.None
		);

		animation.AnimationFinished += OnAnimationFinished;

		animation.Play();
	}

	private void OnAnimationFinished()
	{
		GetTree().ChangeSceneToFile(nextScene);
	}
}