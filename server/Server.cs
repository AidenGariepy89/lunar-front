using Godot;
using Godot.Collections;
using Core;

namespace Server;

public partial class Server : Node2D, NetworkObject
{
    [Export]
    public PackedScene ScoutScene;

    public Core.Main MainRef;

    Logger _log;

    Node2D _scouts;

    public override void _Ready()
    {
        _scouts = GetNode<Node2D>("Scouts");

        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;

        var peer = new ENetMultiplayerPeer();
        _log = new Logger(-1, "server");

        var err = peer.CreateServer(Constants.Port, Constants.MaxPlayers);
        if (err != Error.Ok)
        {
            throw new System.Exception(err.ToString());
        }

        Multiplayer.MultiplayerPeer = peer;

        _log.MultiplayerId = peer.GetUniqueId();
        _log.Line("Server running.");
    }

    /// Called when a new peer connects to the server.
    void PeerConnected(long id)
    {
        _log.Line($"Connected with {id}");

        Faction faction = FactionJoin();
        var scout = ScoutScene.Instantiate<ScoutServer>();
        scout.Initialize(id, faction);

        var existingScouts = new Array<Array>();

        foreach (var child in _scouts.GetChildren())
        {
            var existingScout = child as ScoutServer;
            if (existingScout == null)
            {
                continue;
            }

            existingScouts.Add(existingScout.Data.Serialize());
        }

        var newScoutData = scout.Data.Serialize();

        existingScouts.Add(newScoutData);

        _scouts.AddChild(scout);


        MainRef.Rpc(Core.Main.MethodName.SpawnNewScout, newScoutData);
        MainRef.RpcId(id, Core.Main.MethodName.SpawnScouts, existingScouts);
    }

    /// Every peer
    void PeerDisconnected(long id)
    {
        _log.Line($"Disconnected with {id}");
    }

    Faction FactionJoin()
    {
        var children = _scouts.GetChildren();
        int earthCount = 0;
        int marsCount = 0;

        foreach (var child in children)
        {
            var scout = child as ScoutServer;
            if (scout == null)
            {
                continue;
            }

            if (scout.Data.Faction == Faction.Mars)
            {
                marsCount++;
            }
            else
            {
                earthCount++;
            }
        }

        if (marsCount > earthCount)
        {
            return Faction.Mars;
        }

        return Faction.Earth;
    }

    public void SpawnNewScout(Array scoutPacket) { }
    public void SpawnScouts(Array<Array> scouts) { }
}
