using Godot;
using System.Collections.Generic;

public partial class TutorialTrigger : Area2D
{
    [ExportGroup("教程")]
    [Export] public Godot.Collections.Array<string> messages = new();
    [Export] public bool oneShot = true;

    // static 不会随着重开关卡而清空；关闭游戏后才清空。
    private static HashSet<string> shownTutorials = new();
    private string tutorialKey;

    public override void _Ready()
    {
        // 用关卡文件和节点路径区分每个触发器，不需要手动填编号。
        Node level = this;
        while (level.GetParent() != GetTree().Root)
            level = level.GetParent();
        tutorialKey = level.SceneFilePath + ":" + level.GetPathTo(this);

        if (oneShot && shownTutorials.Contains(tutorialKey))
            SetDeferred(Area2D.PropertyName.Monitoring, false);

        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not Player || (oneShot && shownTutorials.Contains(tutorialKey)))
            return;

        TutorialUI tutorialUI = GetTree().GetFirstNodeInGroup("TutorialUI") as TutorialUI;
        if (tutorialUI == null)
        {
            GD.PrintErr("没有找到 TutorialUI");
            return;
        }

        // UI 忙着显示其他教程、或内容为空时，不算已经展示。
        if (!tutorialUI.ShowMessages(messages))
            return;

        if (oneShot)
        {
            shownTutorials.Add(tutorialKey);
            SetDeferred(Area2D.PropertyName.Monitoring, false);
        }
    }
}
