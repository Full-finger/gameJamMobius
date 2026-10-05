using Godot;

public partial class PauseMenu : CanvasLayer
{
	[Export] public Button restartButton;
	[Export] public Button menuButton;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Hide();
		restartButton.Pressed += Restart;
		menuButton.Pressed += BackToMenu;
	}

	public override void _Input(InputEvent @event)
	{
		// ui_cancel 默认包含 Esc。
		if (@event.IsActionPressed("ui_cancel"))
		{
			Visible = !Visible;
			GetTree().Paused = Visible;

			GetViewport().SetInputAsHandled();
		}
	}

	private void Restart()
	{
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}

	private void BackToMenu()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
	}
}