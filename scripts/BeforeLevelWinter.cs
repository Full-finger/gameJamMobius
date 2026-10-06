using Godot;

public partial class BeforeLevelWinter : Control
{
	[ExportGroup("三个填充图")]
	[Export] public TextureRect item1;
	[Export] public TextureRect item2;
	[Export] public TextureRect item3;

	[ExportGroup("时间")]
	[Export] public float interval = 1f;

	[ExportGroup("下一场景")]
	[Export(PropertyHint.File, "*.tscn")]
	public string nextScene =
		"res://scenes/final_level.tscn";

	public override void _Ready()
	{
		item1.Visible = false;
		item2.Visible = false;
		item3.Visible = false;

		PlaySequence();
	}

	private async void PlaySequence()
	{
		// =========================
		// 第一件
		// =========================

		if (GameState.Instance.Level1Collected)
		{
			ShowItem(item1);
		}

		await ToSignal(
			GetTree().CreateTimer(interval),
			SceneTreeTimer.SignalName.Timeout
		);


		// =========================
		// 第二件
		// =========================

		if (GameState.Instance.Level2Collected)
		{
			ShowItem(item2);
		}

		await ToSignal(
			GetTree().CreateTimer(interval),
			SceneTreeTimer.SignalName.Timeout
		);


		// =========================
		// 第三件
		// =========================

		if (GameState.Instance.Level3Collected)
		{
			ShowItem(item3);
		}

		await ToSignal(
			GetTree().CreateTimer(interval),
			SceneTreeTimer.SignalName.Timeout
		);


		// =========================
		// 进入最终关
		// =========================

		TransitionManager.Instance.TransitionTo(nextScene);
	}


	private void ShowItem(TextureRect item)
	{
		item.Visible = true;

		// 稍微做一个很简单的出现效果
		Vector2 originalScale = item.Scale;

		item.Scale = originalScale * 0.7f;

		item.Modulate = new Color(
			item.Modulate.R,
			item.Modulate.G,
			item.Modulate.B,
			0f
		);

		Tween tween = CreateTween();
		tween.SetParallel(true);

		tween.TweenProperty(
			item,
			"scale",
			originalScale,
			0.2f
		)
		.SetTrans(Tween.TransitionType.Back)
		.SetEase(Tween.EaseType.Out);

		tween.TweenProperty(
			item,
			"modulate:a",
			1f,
			0.15f
		);
	}
}