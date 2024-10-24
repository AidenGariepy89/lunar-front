using Godot;
using Godot.Collections;
using Core;

namespace Server;

public partial class Server : Node2D
{
    [Export]
    public PackedScene ScoutScene;
    
    [Export]
    public float SpawnDistFromEdge = 100.0f;

    public Main MainRef;
    public long NextBulletId = 1;
    public PlanetServer Earth;
    public PlanetServer Mars;

    Logger _log;
    RandomNumberGenerator _rng;

    Timer _syncTimer;

    long _currentSeqNum = 0;

    public override void _Ready()
    {
        _syncTimer = GetNode<Timer>("SyncTimer");
        _syncTimer.WaitTime = 0.0625f;
        _syncTimer.Timeout += SyncTimeout;

        _rng = new RandomNumberGenerator();

        Earth = GetNode<PlanetServer>("Earth");
        Earth.Initialize(Faction.Earth, this);
        Mars = GetNode<PlanetServer>("Mars");
        Mars.Initialize(Faction.Mars, this);

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
        scout.Initialize(id, RequestSpawnPosition(faction), faction, this);

        var existingScouts = new Array<Array>();
        foreach (var child in MainRef.Scouts.GetChildren())
        {
            var existingScout = child as ScoutServer;
            if (existingScout == null)
            {
                continue;
            }

            existingScouts.Add(existingScout.Data.Serialize());
        }

        var existingBullets = new Array<Array>();
        foreach (var child in MainRef.Bullets.GetChildren())
        {
            var bullet = child as ScoutBullet;
            if (bullet == null)
            {
                continue;
            }

            existingBullets.Add(BulletSpawnPacket.Construct(
                bullet.BulletId,
                bullet.Position,
                bullet.Velocity,
                bullet.Rotation,
                bullet.Faction
            ));
        }

        var newScoutData = scout.Data.Serialize();

        existingScouts.Add(newScoutData);

        MainRef.Scouts.AddChild(scout);

        MainRef.Rpc(Main.MethodName.SpawnNewScout, newScoutData);
        MainRef.RpcId(
            id,
            Main.MethodName.JoinGame,
            existingScouts,
            existingBullets,
            Earth.Data.Serialize(),
            Mars.Data.Serialize()
        );
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

    public Vector2 RequestSpawnPosition(Faction faction)
    {
        Vector2 spawnPosition = Vector2.Zero;
        // This is the simplest way to do spawn points, and works since the server is the authority
        if (faction == Faction.Earth)
        {
            spawnPosition.X = _rng.RandfRange(MainRef.Map.EarthSpawnLeft, MainRef.Map.EarthSpawnRight);
            spawnPosition.Y = _rng.RandfRange(MainRef.Map.TopLeft.Y + SpawnDistFromEdge, MainRef.Map.BottomRight.Y - SpawnDistFromEdge);
        }
        else
        {
            spawnPosition.X = _rng.RandfRange(MainRef.Map.MarsSpawnLeft, MainRef.Map.MarsSpawnRight);
            spawnPosition.Y = _rng.RandfRange(MainRef.Map.TopLeft.Y + SpawnDistFromEdge, MainRef.Map.BottomRight.Y - SpawnDistFromEdge);
        }

        return spawnPosition;
    }

    void SyncTimeout()
    {
        var needsSync = false;

        var scoutPackets = new Array<Array>();
        foreach (var child in MainRef.Scouts.GetChildren())
        {
            var scout = child as ScoutServer;

            if (scout.NeedsSync())
            {
                scoutPackets.Add(scout.Data.Serialize());
            }
        }
        needsSync = needsSync || scoutPackets.Count != 0;

        var bulletPackets = new Array<Array>();
        foreach (var child in MainRef.Bullets.GetChildren())
        {
            var bullet = child as ScoutBullet;

            bulletPackets.Add(BulletPacket.Construct(bullet.BulletId, bullet.Position));
        }
        needsSync = needsSync || bulletPackets.Count != 0;

        needsSync = needsSync || Earth.NeedsSync();
        needsSync = needsSync || Mars.NeedsSync();

        if (!needsSync)
        {
            return;
        }

        MainRef.Rpc(
            Core.Main.MethodName.ReceiveSync,
            _currentSeqNum,
            scoutPackets,
            bulletPackets,
            Earth.Data.Serialize(),
            Mars.Data.Serialize()
        );
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
