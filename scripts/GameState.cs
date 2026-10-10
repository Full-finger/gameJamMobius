using Godot;

public partial class GameState : Node
{
    public static GameState Instance { get; private set; }

    public bool Level1Collected { get; private set; } = false;
    public bool Level2Collected { get; private set; } = false;
    public bool Level3Collected { get; private set; } = false;

    // 只暂存当前这次挑战捡到的道具；过关前不计入结局。
    private int pendingLevel;
    private Node pendingScene;

    public bool AllCollected =>
        Level1Collected &&
        Level2Collected &&
        Level3Collected;

    public override void _Ready()
    {
        Instance = this;
    }

    public void Collect(int level)
    {
        Node scene = GetTree().CurrentScene;
        if (pendingScene != scene)
        {
            DiscardPendingCollection();
            pendingScene = scene;
            // 死亡重开、手动重开、返回菜单，都会离开当前场景。
            scene.TreeExiting += DiscardPendingCollection;
        }

        pendingLevel = level;
    }

    // 只有成功到达本关终点才调用。
    public void ConfirmLevelCollection()
    {
        switch (pendingLevel)
        {
            case 1:
                Level1Collected = true;
                break;

            case 2:
                Level2Collected = true;
                break;

            case 3:
                Level3Collected = true;
                break;
        }

        pendingLevel = 0;

        GD.Print(
            $"收集状态：{Level1Collected}, " +
            $"{Level2Collected}, " +
            $"{Level3Collected}"
        );
    }

    private void DiscardPendingCollection()
    {
        pendingLevel = 0;
        pendingScene = null;
    }

    public bool IsCollected(int level)
    {
        return level switch
        {
            1 => Level1Collected,
            2 => Level2Collected,
            3 => Level3Collected,
            _ => false
        };
    }

    // 只有开始一局全新游戏时调用
    public void ResetAll()
    {
        DiscardPendingCollection();
        Level1Collected = false;
        Level2Collected = false;
        Level3Collected = false;
    }
}