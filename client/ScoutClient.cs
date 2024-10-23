using Godot;
using Core;

namespace Client;

public partial class ScoutClient : Area2D
{
    public Scout Data;
    public bool IsPlayerScout = false;

    Map _map;

    public void Initialize(Scout data, Map map)
    {
        _map = map;

        Data = data;

        Position = Data.Position;
        Rotation = Data.Rotation;
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
    }
}
