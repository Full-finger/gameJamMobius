using Godot;

public partial class Button_ReturnToMain : Button
{
	[Signal]
    public delegate void BackRequestedEventHandler();
		public override void _Ready()
	{
		 ProcessMode = ProcessModeEnum.Inherit;
		Pressed += StartGame;
	}

	private void StartGame()
	{
		EmitSignal(SignalName.BackRequested);
	}
}
