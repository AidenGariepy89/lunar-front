using Godot;

public partial class ScoutBullet : Area2D
{
    [Export]
    public Texture2D EarthTexture;
    [Export]
    public Texture2D MarsTexture;

    public Vector2 Velocity = Vector2.Zero;
    public Faction Faction;

    Game _game;

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

    public void Instantiate(Game game)
    {
        _game = game;
    }

    public override void _Process(double delta)
    {
        if (!Utils.InBounds(Position, _game.TopLeft, _game.BottomRight))
        {
            QueueFree();
        }

        Position += Velocity * (float)delta;
    }
}
