using Godot;

public partial class ScoutBullet : Area2D
{
    [Export]
    public Texture2D EarthTexture;
    [Export]
    public Texture2D MarsTexture;

    [Export]
    public float Timeout = 5.0f;
    [Export]
    public float FadeoutTime = 1.0f;

    public Vector2 Velocity = Vector2.Zero;
    public Core.Faction Faction;

    Game _game;
    Timer _timer;

    bool _fading = false;

    public void Instantiate(Game game)
    {
        _game = game;

        _timer = GetNode<Timer>("Timer");
        _timer.Autostart = true;
        _timer.OneShot = true;
        _timer.WaitTime = (Timeout > FadeoutTime) ? Timeout - FadeoutTime : 0;
        _timer.Timeout += TimedOut;

        var sprite = GetNode<Sprite2D>("Sprite2D");
        if (Faction == Core.Faction.Mars)
        {
            sprite.Texture = MarsTexture;
            return;
        }
        sprite.Texture = EarthTexture;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (!Utils.InBounds(Position, _game.TopLeft, _game.BottomRight))
        {
            QueueFree();
        }

        Position += Velocity * dt;

        if (_fading)
        {
            if (Modulate.A <= 0)
            {
                _fading = false;
                QueueFree();
            }

            Color mod = Modulate;
            mod.A -= dt / FadeoutTime;
            Modulate = mod;
        }
    }

    void TimedOut()
    {
        _fading = true;
        SetCollisionLayerValue(1, false);
    }
}
