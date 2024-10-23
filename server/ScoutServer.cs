using Godot;
using System.Collections.Generic;
using Core;

namespace Server;

public partial class ScoutServer : Area2D
{
    public Scout Data;

    // Input
    public bool Forward = false;
    public bool Backward = false;
    public bool Rightward = false;
    public bool Leftward = false;
    public Vector2 Mouse = Vector2.Zero;

    Queue<InputAction> _inputQueue = new Queue<InputAction>();
    float _inputSimTime;

    Scout OldData = null;

    public void Initialize(long multiplayerId, Faction faction)
    {
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
        if (OldData == null)
        {
            OldData = Data.Copy();
            return true;
        }

        bool needsSync = OldData.Health != Data.Health
            || OldData.CurrentState != Data.CurrentState
            || OldData.Position != Data.Position
            || OldData.Velocity != Data.Velocity
            || OldData.Rotation != Data.Rotation
            || OldData.ThrustForward != Data.ThrustForward
            || OldData.ThrustBackward != Data.ThrustBackward
            || OldData.ThrustRight != Data.ThrustRight
            || OldData.ThrustLeft != Data.ThrustLeft;

        if (needsSync)
        {
            OldData = Data.Copy();
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

        if (currentInput.Time <= _inputSimTime)
        {
            SetInput(currentInput.Type, currentInput.Value);
            _inputQueue.Dequeue();
        }
    }

    void SetInput(InputAction.InputType input, Variant value)
    {
        switch (input)
        {
            case InputAction.InputType.ThrustForward:
                Forward = (bool)value;
                break;
            case InputAction.InputType.ThrustBackward:
                Backward = (bool)value;
                break;
            case InputAction.InputType.ThrustRightward:
                Rightward = (bool)value;
                break;
            case InputAction.InputType.ThrustLeftward:
                Leftward = (bool)value;
                break;
            case InputAction.InputType.Mouse:
                Mouse = (Vector2)value;
                break;
        }
    }

    void Simulate(float dt)
    {
        Data.Process(dt, Mouse, Forward, Backward, Rightward, Leftward);

        Position = Data.Position;
        Rotation = Data.Rotation;
    }
}
