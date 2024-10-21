using System.Collections.Generic;
using Godot;

public partial class Game : Node2D
{
    [Export]
    public PackedScene ScoutScene;
    [Export]
    public PackedScene BulletScene;
    [Export]
    public Texture2D SpriteForwardThrust;
    [Export]
    public Texture2D SpriteBackwardThrust;
    [Export]
    public Texture2D SpriteLeftwardThrust;
    [Export]
    public Texture2D SpriteRightwardThrust;

    public Main MainRef;

    List<long> _spawnedScouts;

    Node2D _bullets;
    Node2D _scouts;
    Cam _cam;

    Logger _log;

    public override void _Ready()
    {
        _spawnedScouts = new List<long>();
        _log = new Logger(Multiplayer.GetUniqueId(), "game");
    }

    public void Instantiate(Main main)
    {
        MainRef = main;
        _cam = GetNode<Cam>("Camera2D");
        _bullets = GetNode<Node2D>("Bullets");
        _scouts = GetNode<Node2D>("Scouts");
    }

    public void SpawnScout(long id, Faction faction)
    {
        Scout scout = ScoutScene.Instantiate<Scout>();
        scout.Name = id.ToString();
        scout.Faction = faction;
        scout.Instantiate(id, this);

        if (id == Multiplayer.GetUniqueId())
        {
            _cam.Target = scout;
        }

        _spawnedScouts.Add(id);

        _scouts.AddChild(scout);

        _log.Line($"Player {id} joined in faction {scout.Faction}");
    }

    public bool RemoveScout(long id)
    {
        var children = _scouts.GetChildren();
        foreach (var child in children)
        {
            if (child.Name == id.ToString())
            {
                child.QueueFree();
                _spawnedScouts.Remove(id);
                return true;
            }
        }

        return false;
    }

    public void SpawnScoutBullet(
        Vector2 position,
        Vector2 velocity,
        float rotation,
        Faction faction
    )
    {
        var bullet = BulletScene.Instantiate<ScoutBullet>();
        bullet.Position = position;
        bullet.Velocity = velocity;
        bullet.Rotation = rotation;
        bullet.Faction = faction;

        _bullets.AddChild(bullet);
    }

    public Vector2 RequestRespawnCoords()
    {
        return Vector2.Zero;
    }

    public void PlayerDied(long id)
    {
        var children = _scouts.GetChildren();
        foreach (var child in children)
        {
            if (child is not Scout)
            {
                continue;
            }

            var scout = child as Scout;

            if (scout.MultiplayerID == id)
            {
                // ignore this warning
                scout.Die();
                break;
            }
        }
    }
}
