using Godot;
using Godot.Collections;
using Core;

namespace Client;

public partial class Client : Node2D, NetworkObject
{
    [Export]
    public PackedScene ScoutScene;

    public Core.Main MainRef;

    Logger _log;

    Button _button;
    Node2D _scouts;

    ScoutClient _playerScout = null;

    public override void _Ready()
    {
        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;

        _button = GetNode<Button>("CanvasLayer/Button");
        _button.Pressed += EstablishConnection;

        _log = new Logger(-1, "client");

        _scouts = GetNode<Node2D>("Scouts");
    }

    public override void _Process(double delta)
    {
    }

    void EstablishConnection()
    {
        var peer = new ENetMultiplayerPeer();
        var err = peer.CreateClient(Constants.Address, Constants.Port);
        if (err != Error.Ok)
        {
            throw new System.Exception(err.ToString());
        }

        Multiplayer.MultiplayerPeer = peer;

        _log.MultiplayerId = peer.GetUniqueId();
        _log.Line("Established client.");
    }

    /// Cartesian product of all peers
    void PeerConnected(long id)
    {
        _log.Line($"Connected with {id}");
    }

    /// Cartesian product of all peers
    void PeerDisconnected(long id)
    {
        _log.Line($"Disconnected with {id}");
    }

    void ConnectedToServer()
    {
        _log.Line("Connected to server!");

        RemoveChild(GetChild(0));
    }

    void ConnectionFailed()
    {
        _log.Line("Connection failed!");
    }

    /// Called when current peer is in game and new peer has joined.
    public void SpawnNewScout(Array scoutPacket)
    {
        var scout = Scout.Deserialize(scoutPacket);
        
        if (scout.MultiplayerID == Multiplayer.GetUniqueId())
        {
            return;
        }

        var scoutScene = ScoutScene.Instantiate<ScoutClient>();
        scoutScene.Initialize(scout);

        _scouts.AddChild(scoutScene);

        _log.Line("Spawned newly joined scout");
    }

    /// Called when current peer is joining for the first time.
    public void SpawnScouts(Array<Array> scouts)
    {
        _log.Line($"SpawnScouts - {scouts.Count}");

        foreach (var scout in scouts)
        {
            var scoutData = Scout.Deserialize(scout);
            
            var scoutScene = ScoutScene.Instantiate<ScoutClient>();
            scoutScene.Initialize(scoutData);

            if (scoutData.MultiplayerID == Multiplayer.GetUniqueId())
            {
                _playerScout = scoutScene;
            }

            _scouts.AddChild(scoutScene);

            _log.Line("Spawned scout");
        }
    }
}
