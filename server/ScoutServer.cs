using Godot;
using Core;

namespace Server;

public partial class ScoutServer : Area2D
{
    public Scout Data;

    public void Initialize(long multiplayerId, Faction faction)
    {
        Data = new Scout();
        Data.MultiplayerID = multiplayerId;
        Data.Faction = faction;
        // ...

        Position = Data.Position;
        Rotation = Data.Rotation;
    }
}
