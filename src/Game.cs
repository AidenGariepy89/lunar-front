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
    Cam _cam;

    public override void _Ready()
    {
        _spawnedScouts = new List<long>();
        _cam = GetNode<Cam>("Camera2D");

        _bullets = GetNode<Node2D>("Bullets");
    }

    public void Instantiate(Main main)
    {
        MainRef = main;
    }

    public void SpawnScout(long id)
    {
        Scout scout = ScoutScene.Instantiate<Scout>();
        scout.Name = id.ToString();
        scout.BulletScene = BulletScene;
        scout.SpriteForwardThrust = SpriteForwardThrust;
        scout.SpriteBackwardThrust = SpriteBackwardThrust;
        scout.SpriteRightwardThrust = SpriteRightwardThrust;
        scout.SpriteLeftwardThrust = SpriteLeftwardThrust;

        scout.Instantiate(this);

        if (id == Multiplayer.GetUniqueId())
        {
            _cam.Target = scout;
        }

        _spawnedScouts.Add(id);

        AddChild(scout);
    }

    public bool RemoveScout(long id)
    {
        var children = GetChildren();
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

    public void SpawnScoutBullet(Vector2 position, Vector2 velocity)
    {
        var bullet = BulletScene.Instantiate<ScoutBullet>();
        bullet.Position = position;
        bullet.Velocity = velocity;
        bullet.Rotation = velocity.Angle();

        _bullets.AddChild(bullet);
    }
}
