using Godot;

public partial class Button_exit : Button
{
    public override void _Ready()
    {
        Pressed += QuitGame;
    }

    private void QuitGame()
    {
        GetTree().Quit();
    }
}