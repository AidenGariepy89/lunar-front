using Godot;

public partial class Cam : Camera2D
{
    [Export]
    public Node2D Target = null;

    public override void _Process(double delta)
    {
        Vector2 mousePos = GetGlobalMousePosition();

        if (Target != null)
        {
            Position = (((mousePos + Target.Position) / 2.0f) + Target.Position) / 2.0f;
        }
    }
}
