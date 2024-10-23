using Godot;

namespace Core;

public partial class Cam : Camera2D
{
    [Export]
    public Node2D Target = null;

    Vector2 _boundTopLeft;
    Vector2 _boundBottomRight;

    public void Instantiate(Vector2 boundTopLeft, Vector2 boundBottomRight)
    {
        _boundTopLeft = boundTopLeft;
        _boundBottomRight = boundBottomRight;
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
        Vector2 clamp = Position;
        Position = Position.Clamp(_boundTopLeft + halfView, _boundBottomRight - halfView);
    }
}
