using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace Core;

public struct InputPacket
{
    public long Id;
    public List<InputAction> Actions;

    public static InputPacket Deconstruct(Array data)
    {
        var packet = new InputPacket();

        packet.Id = (long)data[0];
        packet.Actions = new List<InputAction>();

        int idx = 1;
        while (idx + 2 < data.Count)
        {
            var action = new InputAction();

            action.Type = (InputAction.InputType)(int)data[idx];
            action.Time = (float)data[idx + 1];
            action.Value = data[idx + 2];

            packet.Actions.Add(action);

            idx += 3;
        }

        return packet;
    }

    public static Array Construct(long id, List<InputAction> actions)
    {
        var arr = new Array();

        arr.Add(id);

        foreach (var action in actions)
        {
            arr.Add((int)action.Type);
            arr.Add(action.Time);
            arr.Add(action.Value);
        }

        return arr;
    }
}

public struct BulletSpawnPacket
{
    public long BulletId;
    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;
    public Faction Faction;

    public static BulletSpawnPacket Deconstruct(Array data)
    {
        var packet = new BulletSpawnPacket();

        packet.BulletId = (long)data[0];
        packet.Position = (Vector2)data[1];
        packet.Velocity = (Vector2)data[2];
        packet.Rotation = (float)data[3];
        packet.Faction = (Faction)(int)data[4];

        return packet;
    }

    public static Array Construct(long bulletId, Vector2 position, Vector2 velocity, float rotation, Faction faction)
    {
        var arr = new Array();

        arr.Add(bulletId);
        arr.Add(position);
        arr.Add(velocity);
        arr.Add(rotation);
        arr.Add((int)faction);

        return arr;
    }
}

public struct BulletPacket
{
    public long BulletId;
    public Vector2 Position;

    public static BulletPacket Deconstruct(Array data)
    {
        var packet = new BulletPacket();

        packet.BulletId = (long)data[0];
        packet.Position = (Vector2)data[1];

        return packet;
    }
    
    public static Array Construct(long bulletId, Vector2 position)
    {
        var arr = new Array();

        arr.Add(bulletId);
        arr.Add(position);

        return arr;
    }
}
