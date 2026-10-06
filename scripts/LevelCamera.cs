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
