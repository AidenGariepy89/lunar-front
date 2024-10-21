using Godot;

public partial class Cam : Camera2D
{
    [Export]
    public Node2D Target = null;

    Vector2 boundaryTopLeft;
    Vector2 boundaryBottomRight;

    public override void _Ready()
    {
        boundaryTopLeft = new Vector2(-Constants.MapWidth/2, -Constants.MapHeight/2);
        boundaryBottomRight = new Vector2(Constants.MapWidth/2, Constants.MapHeight/2);
    }

    public override void _Process(double delta)
    {
        if (Target == null)
        {
            return;
        }

        Vector2 mousePos = Target.Position;

        if (GetWindow().HasFocus())
        {
            mousePos = GetGlobalMousePosition();
        }

        Vector2 halfView = GetViewportRect().Size * 0.5f;
        Position = (((mousePos + Target.Position) / 2.0f) + Target.Position) / 2.0f;
        Position = Position.Clamp(boundaryTopLeft + halfView, boundaryBottomRight - halfView);
    }
}
