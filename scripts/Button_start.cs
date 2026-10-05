using Godot;

public partial class Button_start : Button
{
    public override void _Ready()
    {
        Pressed += StartGame;
    }

    private void StartGame()
    {
        GetTree().ChangeSceneToFile("res://scenes/level.tscn");
    }
}
