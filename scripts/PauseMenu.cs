using Godot;

public partial class PauseMenu : CanvasLayer
{
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			Toggle();
			GetViewport().SetInputAsHandled();
		}
	}

	public override void _Ready()
	{
		GetNode<Button>("%RestartButton").Pressed += OnRestartPressed;
		GetNode<Button>("%MenuButton").Pressed += OnMenuPressed;
	}

	private void Toggle()
	{
		bool show = !Visible;
		Visible = show;
		GetTree().Paused = show;
	}

	private void Close()
	{
		Visible = false;
		GetTree().Paused = false;
	}

	private void OnRestartPressed()
	{
		Close();
		GetTree().ReloadCurrentScene();
	}

	private void OnMenuPressed()
	{
		Close();
		GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
	}
}
