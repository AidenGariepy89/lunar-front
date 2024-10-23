using Godot;
using Core;

namespace Client;

public partial class ScoutClient : Area2D
{
    [Export]
    public Texture2D SpriteTeamMars;
    [Export]
    public Texture2D SpriteForwardThrust;
    [Export]
    public Texture2D SpriteBackwardThrust;
    [Export]
    public Texture2D SpriteLeftwardThrust;
    [Export]
    public Texture2D SpriteRightwardThrust;

    public Scout Data;
    public bool IsPlayerScout = false;

    Sprite2D _sprite;

    Map _map;

    public void Initialize(Scout data, Map map)
    {
        _sprite = GetNode<Sprite2D>("Sprite");

        _map = map;

        Data = data;

        Position = Data.Position;
        Rotation = Data.Rotation;

        if (Data.Faction == Faction.Mars)
        {
            _sprite.Texture = SpriteTeamMars;
        }
    }

    public void Sync(Scout data)
    {
        Data = data;

        Position = Data.Position;
        Rotation = Data.Rotation;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (Data.MultiplayerID == Multiplayer.GetUniqueId())
        {
            Data.Forward = Input.IsActionPressed("thrust_forward");
            Data.Backward = Input.IsActionPressed("thrust_backward");
            Data.Rightward = Input.IsActionPressed("thrust_right");
            Data.Leftward = Input.IsActionPressed("thrust_left");
            Data.Mouse = GetGlobalMousePosition();
        }

        Data.Process(dt, _map);

        Position = Data.Position;
        Rotation = Data.Rotation;

        QueueRedraw();
    }

    public override void _Draw()
    {
        if (Data.CurrentState != Scout.State.Alive)
        {
            return;
        }

        var offset = new Vector2(-16, -12);
        if (Data.ThrustForward)
        {
            DrawTexture(SpriteForwardThrust, offset);
        }
        if (Data.ThrustBackward)
        {
            DrawTexture(SpriteBackwardThrust, offset);
        }
        if (Data.ThrustRight)
        {
            DrawTexture(SpriteRightwardThrust, offset);
        }
        if (Data.ThrustLeft)
        {
            DrawTexture(SpriteLeftwardThrust, offset);
        }
    }
}
