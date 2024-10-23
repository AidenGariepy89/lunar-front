using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class Main : Node2D
{
    [Export]
    public PackedScene GameScene;

    public int ActivePlayers
    {
        get => EarthTeam.Count + MarsTeam.Count;
    }

    ENetMultiplayerPeer _peer;
    Lobby _lobby = null;
    Game _game = null;
    bool _server = false;

    List<long> EarthTeam = new List<long>();
    List<long> MarsTeam = new List<long>();

    Logger _log;

    public override void _Ready()
    {
        _peer = new ENetMultiplayerPeer();
        _log = new Logger(-1, "main");

        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;

        if (OS.GetCmdlineArgs().Contains("--server"))
        {
            Server();
            return;
        }

        _lobby = GetNode<Lobby>("CanvasLayer/Lobby");
        _lobby.Instantiate(this);
    }

    /// Client setup
    public void StartClient()
    {
        var err = _peer.CreateClient(Constants.Address, Constants.Port);
        if (err != Error.Ok)
        {
            throw new Exception(err.ToString());
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.None);

        Multiplayer.MultiplayerPeer = _peer;

        _log.MultiplayerId = _peer.GetUniqueId();
        _log.Line("Set up client.");
    }

    /// Server setup
    void Server()
    {
        _log.Line("Setting up server.");

        _server = true;

        var err = _peer.CreateServer(Constants.Port, Constants.MaxPlayers);
        if (err != Error.Ok)
        {
            throw new Exception(err.ToString());
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.None);

        Multiplayer.MultiplayerPeer = _peer;

        _log.MultiplayerId = _peer.GetUniqueId();
        _log.Line("Server set up.");
    }

    /// Every peer
    void PeerConnected(long id)
    {
        _log.Line($"Peer connected to {id}.");

        if (Multiplayer.GetUniqueId() == Constants.ServerId)
        {
            Core.Faction faction = FactionJoin(id);

            return;
        }
    }

    /// Every peer
    void PeerDisconnected(long id)
    {
        _log.Line("Peer disconnected.");

        if (_server)
        {
            return;
        }

        _game.RemoveScout(id);
    }

    /// Client function
    void ConnectedToServer()
    {
        _log.Line("Connected to server!");

        _game = GameScene.Instantiate<Game>();
        _game.Instantiate(this);
        _lobby.Visible = false;
        AddChild(_game);

        RpcId(Constants.ServerId, MethodName.RequestPlayers, Multiplayer.GetUniqueId());
    }

    /// Client function
    void ConnectionFailed()
    {
        _log.StdErr("Connection failed");
    }

    public void RpcSendNewBullet(
        Vector2 position,
        Vector2 velocity,
        float rotation,
        Core.Faction faction
    )
    {
        Rpc(MethodName.SendNewBullet, position, velocity, rotation, (int)faction);
    }

    Core.Faction FactionJoin(long id)
    {
        if (MarsTeam.Count < EarthTeam.Count)
        {
            MarsTeam.Add(id);
            return Core.Faction.Mars;
        }

        EarthTeam.Add(id);
        return Core.Faction.Earth;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    void SendNewBullet(
        Vector2 position,
        Vector2 velocity,
        float rotation,
        int faction
    )
    {
        if (_server)
        {
            return;
        }

        _game.SpawnScoutBullet(position, velocity, rotation, (Core.Faction)faction);
    }

    public void RpcPlayerDied(long id)
    {
        Rpc(MethodName.PlayerDied, id);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    void PlayerDied(long id)
    {
        if (_server)
        {
            return;
        }

        _game.PlayerDied(id);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    void RequestPlayers(long id)
    {
        _log.Line($"Requested players from {id}");

        var ids = new long[ActivePlayers];

        for (int i = 0; i < EarthTeam.Count; i++)
        {
            ids[i] = EarthTeam[i];
        }
        for (int i = 0; i < MarsTeam.Count; i++)
        {
            ids[i + EarthTeam.Count] = MarsTeam[i];
        }

        RpcId(id, MethodName.ReceiveExistingPlayers, ids, EarthTeam.Count);

        Core.Faction faction = Core.Faction.Earth;
        if (MarsTeam.Contains(id))
        {
            faction = Core.Faction.Mars;
        }
        else if (!EarthTeam.Contains(id))
        {
            throw new Exception("Crap");
        }

        Rpc(MethodName.SpawnScout, id, (int)faction);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    void ReceiveExistingPlayers(long[] ids, int factionCutoff)
    {
        for (int i = 0; i < ids.Length; i++)
        {
            if (i < factionCutoff)
            {
                _game.SpawnScout(ids[i], Core.Faction.Earth);
            }
            else
            {
                _game.SpawnScout(ids[i], Core.Faction.Mars);
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    void SpawnScout(long scoutId, int faction)
    {
        if (scoutId == Multiplayer.GetUniqueId())
        {
            return;
        }

        _log.Line($"Spawning scout with id {scoutId} and faction {(Core.Faction)faction}");

        _game.SpawnScout(scoutId, (Core.Faction)faction);
    }
}
