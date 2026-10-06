using Godot;

public partial class endPoint : Area2D
{
    [Export] public CanvasLayer endUI;
    [Export] public string nextScenePath = "res://scenes/main_menu.tscn";

    public override void _Ready()
    {
        if (endUI != null)
            endUI.Hide();
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not Player)
            return;

    if (endUI != null)
{
    endUI.ProcessMode = ProcessModeEnum.Always;
    endUI.Show();

    // 禁止暂停菜单处理 Esc，避免玩家用 Esc 解除暂停。
    GetParent().GetNode<PauseMenu>("PauseMenu")
        .SetProcessInput(false);

    GetTree().Paused = true;
}
        else
            TransitionManager.Instance.TransitionTo(nextScenePath);
    }
}
