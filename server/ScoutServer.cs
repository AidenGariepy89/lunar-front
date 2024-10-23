using Godot;
using System.Collections.Generic;
using Core;

namespace Server;

public partial class ScoutServer : Area2D
{
    public Scout Data;

    Queue<InputAction> _inputQueue = new Queue<InputAction>();
    float _inputSimTime;

    Map _map;
    Scout _oldData = null;

    public void Initialize(long multiplayerId, Faction faction, Map map)
    {
        _map = map;

        Data = new Scout();
        Data.MultiplayerID = multiplayerId;
        Data.Faction = faction;
        // ...

        Position = Data.Position;
        Rotation = Data.Rotation;
    }

    public void UpdateInput(InputPacket packet)
    {
        _inputSimTime = 0.0f;
        foreach (var action in packet.Actions)
        {
            _inputQueue.Enqueue(action);
        }
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        HandleInputs(dt);

        Simulate(dt);
    }

    public bool NeedsSync()
    {
        if (_oldData == null)
        {
            _oldData = Data.Copy();
            return true;
        }

        bool needsSync = _oldData.Health != Data.Health
            || _oldData.CurrentState != Data.CurrentState
            || _oldData.Position != Data.Position
            || _oldData.Velocity != Data.Velocity
            || _oldData.Rotation != Data.Rotation
            || _oldData.ThrustForward != Data.ThrustForward
            || _oldData.ThrustBackward != Data.ThrustBackward
            || _oldData.ThrustRight != Data.ThrustRight
            || _oldData.ThrustLeft != Data.ThrustLeft;

        if (needsSync)
        {
            _oldData = Data.Copy();
        }

        return needsSync;
    }

    void HandleInputs(float dt)
    {
        _inputSimTime += dt;

        if (_inputQueue.Count == 0)
        {
            return;
        }

        InputAction currentInput = _inputQueue.Peek();

        // No messing with time, assuming instant application
        SetInput(currentInput.Type, currentInput.Value);
        _inputQueue.Dequeue();
    }

    void SetInput(InputAction.InputType input, Variant value)
    {
        switch (input)
        {
            case InputAction.InputType.ThrustForward:
                Data.Forward = (bool)value;
                break;
            case InputAction.InputType.ThrustBackward:
                Data.Backward = (bool)value;
                break;
            case InputAction.InputType.ThrustRightward:
                Data.Rightward = (bool)value;
                break;
            case InputAction.InputType.ThrustLeftward:
                Data.Leftward = (bool)value;
                break;
            case InputAction.InputType.Mouse:
                Data.Mouse = (Vector2)value;
                break;
        }
    }

    void Simulate(float dt)
    {
        Data.Process(dt, _map);

        Position = Data.Position;
        Rotation = Data.Rotation;
    }
}
