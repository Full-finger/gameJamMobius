using Godot;

public partial class GameState : Node
{
    public static GameState Instance { get; private set; }

    public bool Level1Collected { get; private set; } = false;
    public bool Level2Collected { get; private set; } = false;
    public bool Level3Collected { get; private set; } = false;

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
        switch (level)
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

        GD.Print(
            $"收集状态：{Level1Collected}, " +
            $"{Level2Collected}, " +
            $"{Level3Collected}"
        );
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
        Level1Collected = false;
        Level2Collected = false;
        Level3Collected = false;
    }
}