using Godot;

public partial class LevelCamera : Camera2D
{
    private Vector2 normalZoom;

    public override void _Ready()
    {
        normalZoom = Zoom;
        GetViewport().SizeChanged += FitInsideBackground;
        FitInsideBackground();
    }

    // 每关的 Bounds 根据该关背景设置限制，同一个 Player 可以用于不同大小的地图。
    public void SetBackgroundBounds(Rect2 bounds)
    {
        LimitLeft = Mathf.CeilToInt(bounds.Position.X);
        LimitTop = Mathf.CeilToInt(bounds.Position.Y);
        LimitRight = Mathf.FloorToInt(bounds.End.X);
        LimitBottom = Mathf.FloorToInt(bounds.End.Y);
        FitInsideBackground();
    }

    private void FitInsideBackground()
    {
        Vector2 screen = GetViewportRect().Size;
        float width = LimitRight - LimitLeft;
        float height = LimitBottom - LimitTop;

        // 窗口太宽或太高时，适当拉近镜头，保证视野放得进背景。
        float multiplier = Mathf.Max(1f, Mathf.Max(
            screen.X / (width * normalZoom.X),
            screen.Y / (height * normalZoom.Y)));
        Zoom = normalZoom * multiplier;
        ResetSmoothing();
        ForceUpdateScroll();
    }

    public override void _ExitTree()
    {
        GetViewport().SizeChanged -= FitInsideBackground;
    }
}
