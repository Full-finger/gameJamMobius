using Godot;

public partial class PauseMenu : CanvasLayer
{
	[Export] public Button restartButton;
	[Export] public Button menuButton;
	[Export] public Button settingsButton;
	private SettingsPage settingsWindow;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Hide();
		restartButton.Pressed += Restart;
		if (menuButton != null)
			menuButton.Pressed += BackToMenu;
		if (settingsButton != null)
			settingsButton.Pressed += OpenSettings;
	}

	public override void _Input(InputEvent @event)
	{
		// 设置打开时，Esc 由设置页处理，不能同时解除暂停。
		if (settingsWindow != null)
			return;
		// ui_cancel 默认包含 Esc。
		if (@event.IsActionPressed("ui_cancel"))
		{
			Visible = !Visible;
			GetTree().Paused = Visible;

			GetViewport().SetInputAsHandled();
		}
	}

	private void OpenSettings()
	{
		if (settingsWindow != null)
			return;
		settingsWindow = GD.Load<PackedScene>("res://scenes/settings.tscn").Instantiate<SettingsPage>();
		settingsWindow.ReturnToPauseMenu = this;
		GetTree().Paused = true;
		AddChild(settingsWindow);
		GetNode<Control>("Control").Hide();
	}

	public void CloseSettings()
	{
		settingsWindow.QueueFree();
		settingsWindow = null;
		GetNode<Control>("Control").Show();
		GetTree().Paused = true;
		settingsButton.GrabFocus();
	}

	private void Restart()
	{
		TransitionManager.Instance.ReloadCurrentScene();
	}

	private void BackToMenu()
	{
		TransitionManager.Instance.TransitionTo("res://scenes/main_menu.tscn");
	}
}
