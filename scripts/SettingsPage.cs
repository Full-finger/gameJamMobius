using Godot;

public partial class SettingsPage : CanvasLayer
{
    // 暂停页打开时填入；主菜单直接切场景时保持为空。
    public PauseMenu ReturnToPauseMenu;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Layer = 10;
        GetNode<Button_ReturnToMain>("Control/Return").BackRequested += GoBack;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            GetViewport().SetInputAsHandled();
            GoBack();
        }
    }

    private void GoBack()
    {
        if (ReturnToPauseMenu != null)
        {
            ReturnToPauseMenu.CloseSettings();
        }
        else
        {
            TransitionManager.Instance.TransitionTo("res://scenes/main_menu.tscn");
        }
    }
}
