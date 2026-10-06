using Godot;

public partial class MenuButton : Control
{
	[ExportGroup("节点")]
	[Export] public Button button;
	[Export] public Control visualRoot;
	[Export] public TextureRect hover;
	[Export] public TextureRect click;

	[ExportGroup("Hover")]
	[Export] public float hoverRotation = -1.5f;
	[Export] public float hoverScale = 1.02f;
	[Export] public float hoverTime = 0.12f;

	[ExportGroup("点击震动")]
	[Export] public float shakeAmount = 5f;
	[Export] public float shakeTime = 0.025f;

	[ExportGroup("Click图案")]
	[Export] public float clickStartXScale = 0.1f;
	[Export] public float clickTime = 0.22f;

	private Tween hoverTween;
	private Tween clickTween;

	// 原始状态
	private Vector2 normalPosition;
	private Vector2 normalVisualScale;
	private Vector2 normalClickScale;
	private float normalRotation;

	public override void _Ready()
	{
		// 记录 Inspector 里原本设置好的值
		normalPosition = visualRoot.Position;
		normalVisualScale = visualRoot.Scale;
		normalClickScale = click.Scale;
		normalRotation = visualRoot.RotationDegrees;

		// VisualRoot 围绕中心缩放/旋转
		visualRoot.PivotOffset = visualRoot.Size / 2f;

		// Click 围绕自身中心横向展开
		click.PivotOffset = click.Size / 2f;

		// Hover 只隐藏，不改变它的 Scale
		hover.Modulate = new Color(
			hover.Modulate.R,
			hover.Modulate.G,
			hover.Modulate.B,
			0f
		);

		// Click 初始隐藏
		click.Modulate = new Color(
			click.Modulate.R,
			click.Modulate.G,
			click.Modulate.B,
			0f
		);

		// Click 保持原 Y Scale，只把 X 压窄
		click.Scale = new Vector2(
			normalClickScale.X * clickStartXScale,
			normalClickScale.Y
		);

		button.MouseEntered += OnMouseEntered;
		button.MouseExited += OnMouseExited;
		button.Pressed += OnPressed;
	}

	private void OnMouseEntered()
	{
		if (clickTween != null && clickTween.IsRunning())
			return;

		hoverTween?.Kill();

		hoverTween = CreateTween();
		hoverTween.SetParallel(true);

		// 在原本角度基础上轻微歪斜
		hoverTween.TweenProperty(
			visualRoot,
			"rotation_degrees",
			normalRotation + hoverRotation,
			hoverTime
		)
		.SetTrans(Tween.TransitionType.Quad)
		.SetEase(Tween.EaseType.Out);

		// 在原本大小基础上轻微放大
		hoverTween.TweenProperty(
			visualRoot,
			"scale",
			normalVisualScale * hoverScale,
			hoverTime
		)
		.SetTrans(Tween.TransitionType.Quad)
		.SetEase(Tween.EaseType.Out);

		// Hover 只淡入
		hoverTween.TweenProperty(
			hover,
			"modulate:a",
			1f,
			hoverTime
		)
		.SetTrans(Tween.TransitionType.Quad)
		.SetEase(Tween.EaseType.Out);
	}

	private void OnMouseExited()
	{
		if (clickTween != null && clickTween.IsRunning())
			return;

		hoverTween?.Kill();

		hoverTween = CreateTween();
		hoverTween.SetParallel(true);

		// 恢复原旋转
		hoverTween.TweenProperty(
			visualRoot,
			"rotation_degrees",
			normalRotation,
			hoverTime
		)
		.SetTrans(Tween.TransitionType.Quad)
		.SetEase(Tween.EaseType.Out);

		// 恢复原 Scale
		hoverTween.TweenProperty(
			visualRoot,
			"scale",
			normalVisualScale,
			hoverTime
		)
		.SetTrans(Tween.TransitionType.Quad)
		.SetEase(Tween.EaseType.Out);

		// Hover 淡出
		hoverTween.TweenProperty(
			hover,
			"modulate:a",
			0f,
			hoverTime
		)
		.SetTrans(Tween.TransitionType.Quad)
		.SetEase(Tween.EaseType.Out);
	}

	private void OnPressed()
	{
		PlayClick();
	}

	private void PlayClick()
	{
		hoverTween?.Kill();
		clickTween?.Kill();

		button.Disabled = true;

		clickTween = CreateTween();

		// 先左右震几下
		clickTween.TweenProperty(
			visualRoot,
			"position",
			normalPosition + new Vector2(-shakeAmount, 0f),
			shakeTime
		);

		clickTween.TweenProperty(
			visualRoot,
			"position",
			normalPosition + new Vector2(shakeAmount, 0f),
			shakeTime
		);

		clickTween.TweenProperty(
			visualRoot,
			"position",
			normalPosition + new Vector2(-shakeAmount * 0.5f, 0f),
			shakeTime
		);

		clickTween.TweenProperty(
			visualRoot,
			"position",
			normalPosition,
			shakeTime
		);

		// 准备 Click 图片
		clickTween.TweenCallback(
			Callable.From(() =>
			{
				click.Modulate = new Color(
					click.Modulate.R,
					click.Modulate.G,
					click.Modulate.B,
					1f
				);

				click.Scale = new Vector2(
					normalClickScale.X * clickStartXScale,
					normalClickScale.Y
				);
			})
		);

		// 从中间横向展开到它自己原本的 Scale
		clickTween.TweenProperty(
			click,
			"scale",
			normalClickScale,
			clickTime
		)
		.SetTrans(Tween.TransitionType.Quart)
		.SetEase(Tween.EaseType.Out);

		// 动画结束后执行按钮功能
		clickTween.TweenCallback(
			Callable.From(ExecuteButton)
		);
	}

	private void ExecuteButton()
	{
		GD.Print("按钮功能触发");

		// 例如：
		// TransitionManager.Instance.TransitionTo("res://Scenes/Game.tscn");
	}
}