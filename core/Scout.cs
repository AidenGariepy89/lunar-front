using Godot;
using Godot.Collections;

namespace Core;

public enum Faction
{
    Earth,
    Mars,
}

public class Scout
{
    // Constants

    public const float ThrustMain = 600;
    public const float ThrustForwardAxis = 300;
    public const float ThrustSideAxis = 300;
    public const float CorrectionThrust = 200;
    public const float MaxSpeed = 1000;

    public const int MaxHealth = 3;
    public const double RespawnDelay = 5.0;

    // Visual Variables

    public bool ThrustForward = false;
    public bool ThrustBackward = false;
    public bool ThrustRight = false;
    public bool ThrustLeft = false;

    // State Variables

    public long MultiplayerID;
    public Faction Faction = Faction.Earth;
    public float Health = MaxHealth;

    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;

    public Scout()
    {
        Position = Vector2.Zero;
        Velocity = Vector2.Zero;
        Rotation = 0.0f;
    }

    public Array Serialize()
    {
        Array arr = new Array();

        // State
        arr.Add(MultiplayerID);
        arr.Add((int)Faction);
        arr.Add(Health);
        arr.Add(Position);
        arr.Add(Velocity);
        arr.Add(Rotation);

        // Visual
        arr.Add(ThrustForward);
        arr.Add(ThrustBackward);
        arr.Add(ThrustRight);
        arr.Add(ThrustLeft);

        return arr;
    }

    public static Scout Deserialize(Array arr)
    {
        if (arr.Count != 10)
        {
            throw new System.Exception("ARRAY BAD!!!!");
        }

        var scout = new Scout();

        // State
        scout.MultiplayerID = (long)arr[0];
        scout.Faction = (Faction)(int)arr[1];
        scout.Health = (int)arr[2];
        scout.Position = (Vector2)arr[3];
        scout.Velocity = (Vector2)arr[4];
        scout.Rotation = (float)arr[5];

        // Visual
        scout.ThrustForward = (bool)arr[6];
        scout.ThrustBackward = (bool)arr[7];
        scout.ThrustRight = (bool)arr[8];
        scout.ThrustLeft = (bool)arr[9];

        return scout;
    }
}
