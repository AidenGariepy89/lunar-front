using Godot;
using Godot.Collections;
using Core;

namespace Server;

public partial class Server : Node2D, NetworkObject
{
    [Export]
    public PackedScene ScoutScene;

    public Core.Main MainRef;
    public long NextBulletId = 1;

    Logger _log;

    Timer _syncTimer;

    long _currentSeqNum = 0;

    public override void _Ready()
    {
        _syncTimer = GetNode<Timer>("SyncTimer");
        _syncTimer.WaitTime = 0.0625f;
        _syncTimer.Timeout += SyncTimeout;

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

        _syncTimer.Start();
    }

    /// Called when a new peer connects to the server.
    void PeerConnected(long id)
    {
        _log.Line($"Connected with {id}");

        Faction faction = FactionJoin();
        var scout = ScoutScene.Instantiate<ScoutServer>();
        scout.Name = id.ToString();

        Vector2 spawnPosition = Vector2.Zero;
        // This is the simplest way to do spawn points, and works since the server is the authority
        RandomNumberGenerator rng = new RandomNumberGenerator();
        if (faction == Faction.Earth) {
            spawnPosition.X = rng.RandfRange(Constants.EarthSpawn[0], Constants.EarthSpawn[0] + Constants.SpawnZoneWidth);
            spawnPosition.Y = rng.RandfRange(Constants.EarthSpawn[1], Constants.EarthSpawn[1] + Constants.SpawnZoneHeight);
        } if (faction == Faction.Mars) {
            spawnPosition.X = rng.RandfRange(Constants.MarsSpawn[0], Constants.MarsSpawn[0] + Constants.SpawnZoneWidth);
            spawnPosition.Y = rng.RandfRange(Constants.MarsSpawn[1], Constants.MarsSpawn[1] + Constants.SpawnZoneHeight);
        }
        scout.Position = spawnPosition;
        //_log.Line($"Spawned {id} at {spawnPosition.X}, {spawnPosition.Y}");
        scout.Initialize(id, faction, this);

        var existingScouts = new Array<Godot.Collections.Array>();

        foreach (var child in MainRef.Scouts.GetChildren())
        {
            var existingScout = child as ScoutServer;
            if (existingScout == null)
            {
                continue;
            }

            existingScouts.Add(existingScout.Data.Serialize());
        }

        var newScoutData = scout.Data.Serialize();

        existingScouts.Add(newScoutData); // If the newScout is added here, why is there an Rpc call to spawn the new scout *and* the existingScouts?

        MainRef.Scouts.AddChild(scout);

        MainRef.Rpc(Core.Main.MethodName.SpawnNewScout, newScoutData);
        MainRef.RpcId(id, Core.Main.MethodName.SpawnScouts, existingScouts);
    }

    /// Every peer
    void PeerDisconnected(long id)
    {
        _log.Line($"Disconnected with {id}");

        GetScoutById(id).QueueFree();
    }

    Faction FactionJoin()
    {
        var children = MainRef.Scouts.GetChildren();
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

        if (marsCount < earthCount)
        {
            return Faction.Mars;
        }

        return Faction.Earth;
    }

    public void DeliverInput(Array input)
    {
        var packet = InputPacket.Deconstruct(input);

        GetScoutById(packet.Id).UpdateInput(packet);
    }

    public void HitScout(Array scoutPacket, long BulletId) {
        _log.Line("HitScout was called on the server!");
    }

    public void SpawnNewScout(Array scoutPacket) { }
    public void SpawnScouts(Array<Array> scouts) { }
    public void ReceiveSync(long seqNum, Array<Array> syncData) { }

    void SyncTimeout()
    {
        var packet = new Array<Array>();

        foreach (var child in MainRef.Scouts.GetChildren())
        {
            var scout = child as ScoutServer;

            if (scout.NeedsSync())
            {
                packet.Add(scout.Data.Serialize());
            }
        }

        if (packet.Count == 0)
        {
            return;
        }

        MainRef.Rpc(Core.Main.MethodName.ReceiveSync, _currentSeqNum, packet);
        _currentSeqNum = (_currentSeqNum + 1) % long.MaxValue;
    }

    ScoutServer GetScoutById(long id)
    {
        string idStr = id.ToString();
        foreach (var child in MainRef.Scouts.GetChildren())
        {
            if (child.Name == idStr)
            {
                return child as ScoutServer;
            }
        }

        return null;
    }
}
