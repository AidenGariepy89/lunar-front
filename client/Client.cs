using Godot;
using Godot.Collections;
using Core;

namespace Client;

public partial class Client : Node2D, NetworkObject
{
    [Export]
    public PackedScene ScoutScene;

    public Core.Main MainRef;

    bool _connected = false;
    Logger _log;

    Button _button;
    Node2D _scouts;
    Timer _inputTimer;
    InputCollector _inputCollector;

    ScoutClient _playerScout = null;

    public override void _Ready()
    {
        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;

        _button = GetNode<Button>("CanvasLayer/Button");
        _button.Pressed += EstablishConnection;

        _inputTimer = GetNode<Timer>("InputTimer");
        _inputTimer.Timeout += InputTimeout;

        _log = new Logger(-1, "client");
        _inputCollector = GetNode<InputCollector>("InputCollector");

        _scouts = GetNode<Node2D>("Scouts");
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

        _inputTimer.Start();
        _inputCollector.StartCollection();

        _connected = true;
    }

    public override void _Process(double delta)
    {
        if (!_connected)
        {
            return;
        }

        float dt = (float)delta;

        _inputCollector.Update(dt);
    }

    public void DeliverInput(Array input) { }

    /// Called when current peer is in game and new peer has joined.
    public void SpawnNewScout(Array scoutPacket)
    {
        var scout = Scout.Deserialize(scoutPacket);
        
        if (scout.MultiplayerID == Multiplayer.GetUniqueId())
        {
            return;
        }

        var scoutScene = ScoutScene.Instantiate<ScoutClient>();
        scoutScene.Name = scout.MultiplayerID.ToString();
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
            scoutScene.Name = scoutData.MultiplayerID.ToString();
            scoutScene.Initialize(scoutData);

            if (scoutData.MultiplayerID == Multiplayer.GetUniqueId())
            {
                _playerScout = scoutScene;
            }

            _scouts.AddChild(scoutScene);

            _log.Line("Spawned scout");
        }
    }

    public void ReceiveSync(Array<Array> syncData)
    {
        foreach (var data in syncData)
        {
            var scoutData = Scout.Deserialize(data);
            GetScoutById(scoutData.MultiplayerID).Sync(scoutData);
        }
        _log.Line("Synced");
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

    void InputTimeout()
    {
        var actions = _inputCollector.FinishCollection();

        if (actions.Count != 0)
        {
            var packet = InputPacket.Construct(Multiplayer.GetUniqueId(), actions);
            MainRef.RpcId(Constants.ServerId, Core.Main.MethodName.DeliverInput, packet);
        }

        _inputCollector.StartCollection();
    }

    ScoutClient GetScoutById(long id)
    {
        string idStr = id.ToString();
        foreach (var child in _scouts.GetChildren())
        {
            if (child.Name == idStr)
            {
                return child as ScoutClient;
            }
        }

        return null;
    }
}
