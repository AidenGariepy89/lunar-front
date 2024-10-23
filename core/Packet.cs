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
