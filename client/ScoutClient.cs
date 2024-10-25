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
    [Export]
    public PackedScene ExplosionScene;
    [Export]
    public PackedScene HitScene;

    public Scout Data;
    public bool IsPlayerScout = false;

    Sprite2D _sprite;
    AudioStreamPlayer2D _shootAudio;
    AudioStreamPlayer2D _explosionAudio;
    AudioStreamPlayer2D _hitAudio;

    Map _map;

    public void Initialize(Scout data, Map map)
    {
        _sprite = GetNode<Sprite2D>("Sprite");
        _shootAudio = GetNode<AudioStreamPlayer2D>("ShootSound");
        _explosionAudio = GetNode<AudioStreamPlayer2D>("ExplosionSound");
        _hitAudio = GetNode<AudioStreamPlayer2D>("HitSound");

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

        Visible = Data.CurrentState == Scout.State.Alive;
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

    public void ShotBullet()
    {
        _shootAudio.Play();
    }

    public void PlayHit()
    {
        var hit = HitScene.Instantiate<Particles>();
        hit.Position = Position;
        GetTree().Root.GetChild(0).AddChild(hit);

        _hitAudio.Play();
    }

    public void PlayExplosion()
    {
        // Here, we play the explosion animation
        // Instance the explosion scene
        var explosion = ExplosionScene.Instantiate<Particles>();
        explosion.Position = Position;
        explosion.Visible = true;
        GetTree().Root.GetChild(0).AddChild(explosion);

        _explosionAudio.Play();
    }
}
