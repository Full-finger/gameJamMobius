using Godot;

public partial class LevelBounds : StaticBody2D
{
    [Export] public Player player;

    public override void _Ready()
    {
        Rect2 bounds = new();
        bool foundBackground = false;

        // 把本关所有 LevelBackground 组里的背景图片合成一个总范围。
        foreach (Node node in GetTree().GetNodesInGroup("LevelBackground"))
        {
            if (node is not Sprite2D sprite || !GetParent().IsAncestorOf(sprite))
                continue;

            Rect2 piece = sprite.GlobalTransform * sprite.GetRect();
            bounds = foundBackground ? bounds.Merge(piece) : piece;
            foundBackground = true;
        }

        if (!foundBackground)
        {
            GD.PushError("本关没有加入 LevelBackground 分组的背景图片。");
            return;
        }

        var camera = player.GetNode<LevelCamera>("Camera2D");
        camera.SetBackgroundBounds(bounds);

        // 两侧是无限高的边界，藤蔓把人甩高时也不能绕过墙顶。
        GetNode<CollisionShape2D>("WallLeft").GlobalPosition = new Vector2(camera.LimitLeft, 0);
        GetNode<CollisionShape2D>("WallRight").GlobalPosition = new Vector2(camera.LimitRight, 0);

        // 底部保留坠落玩法，掉出画面后走玩家原有的死亡与重开流程。
        player.deathY = camera.LimitBottom + 200f;
    }
}
