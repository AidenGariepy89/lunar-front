using Godot;
using Core;

namespace Client;

public partial class ScoutClient : Area2D
{
    public Scout Data;

    public void Initialize(Scout data)
    {
        Data = data;

        Position = Data.Position;
        Rotation = Data.Rotation;
    }
}
