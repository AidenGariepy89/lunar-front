using Godot;

public partial class ScoutBullet : Area2D
{
    public Vector2 Velocity = Vector2.Zero;

    public override void _Process(double delta)
    {
        if (Position > Vector2.One * 4000.0f || Position < Vector2.One * -4000.0f)
        {
            QueueFree();
        }

        Position += Velocity * (float)delta;
    }
}
