using Godot;

public partial class ScoutBullet : Area2D
{
    [Export]
    public Texture2D EarthTexture;
    [Export]
    public Texture2D MarsTexture;

    public Vector2 Velocity = Vector2.Zero;
    public Faction Faction;

    public override void _Ready()
    {
        var sprite = GetNode<Sprite2D>("Sprite2D");
        if (Faction == Faction.Mars)
        {
            sprite.Texture = MarsTexture;
            return;
        }
        sprite.Texture = EarthTexture;
    }

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
