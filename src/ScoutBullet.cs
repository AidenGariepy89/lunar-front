using Godot;

public class ScoutBulletData
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;
    public Faction Faction;
}

public partial class ScoutBullet : Area2D
{
    public Vector2 Velocity = Vector2.Zero;
    public Faction Faction;

    public override void _Process(double delta)
    {
        if (Position.X < -Constants.MapWidth / 2 || Position.X > Constants.MapWidth / 2
            || Position.Y < -Constants.MapHeight / 2 || Position.Y > Constants.MapHeight / 2)
        {
            QueueFree();
        }

        Position += Velocity * (float)delta;
    }
}
