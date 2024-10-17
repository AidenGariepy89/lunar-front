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

    List<int> spawnedScouts;

    Cam cam;

    public override void _Ready()
    {
        spawnedScouts = new List<int>();
        cam = GetNode<Cam>("Camera2D");
    }

    public void SpawnScount(int id)
    {
        Scout scout = ScoutScene.Instantiate<Scout>();
        scout.Name = id.ToString();
        scout.BulletScene = BulletScene;
        scout.SpriteForwardThrust = SpriteForwardThrust;
        scout.SpriteBackwardThrust = SpriteBackwardThrust;
        scout.SpriteRightwardThrust = SpriteRightwardThrust;
        scout.SpriteLeftwardThrust = SpriteLeftwardThrust;

        if (id == Multiplayer.GetUniqueId())
        {
            cam.Target = scout;
        }

        spawnedScouts.Add(id);

        AddChild(scout);
    }

    public bool RemoveScout(int id)
    {
        var children = GetChildren();
        foreach (var child in children)
        {
            if (child.Name == id.ToString())
            {
                child.QueueFree();
                spawnedScouts.Remove(id);
                return true;
            }
        }

        return false;
    }
}
