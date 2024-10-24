using Godot;
using Godot.Collections;
using Core;

namespace Client;

public partial class Client : Node2D, NetworkObject
{
    [Export]
    public PackedScene ScoutScene;

    public Main MainRef;

    bool _connected = false;
    Logger _log;
    long _currentSeqNum = -1;

    Button _button;
    Timer _inputTimer;
    InputCollector _inputCollector;
    AudioStreamPlayer _music;

    ScoutClient _playerScout = null;

    bool _audioMuted = false;

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

        _music = GetNode<AudioStreamPlayer>("Music");
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

        MainRef.Map.Visible = true;
    }

    public override void _Process(double delta)
    {
        if (!_connected)
        {
            return;
        }

        float dt = (float)delta;

        _inputCollector.Update(dt);

        if (Input.IsActionJustPressed("debug"))
        {
            _audioMuted = !_audioMuted;
            AudioServer.SetBusMute(AudioServer.GetBusIndex("Music"), _audioMuted);
        }
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
        scoutScene.Initialize(scout, MainRef.Map);

        MainRef.Scouts.AddChild(scoutScene);

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
            scoutScene.Initialize(scoutData, MainRef.Map);

            if (scoutData.MultiplayerID == Multiplayer.GetUniqueId())
            {
                scoutScene.IsPlayerScout = true;
                MainRef.Cam.Target = scoutScene;
                _playerScout = scoutScene;
            }

            MainRef.Scouts.AddChild(scoutScene);

            _log.Line("Spawned scout");
        }
    }

    public void ReceiveSync(long seqNum, Array<Array> syncData)
    {
        // If seqNum is a past seqNum and we are sure a wrap did not happen, then ignore
        if (seqNum <= _currentSeqNum && (seqNum > 1000 || _currentSeqNum < long.MaxValue - 1000))
        {
            return;
        }
        _currentSeqNum = seqNum;

        foreach (var data in syncData)
        {
            var scoutData = Scout.Deserialize(data);
            GetScoutById(scoutData.MultiplayerID).Sync(scoutData);
        }
    }

    public void BulletShot(long shotById)
    {
        GetScoutById(shotById).ShotBullet();
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

        GetScoutById(id).QueueFree();
    }

    void ConnectedToServer()
    {
        _log.Line("Connected to server!");

        RemoveChild(GetChild(0));

        MainRef.Minimap.Initialize(MainRef);

        _music.Play();

        _inputTimer.Start();
        _inputCollector.StartCollection();
        _connected = true;
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

    // Mostly for visuals, since the respawn position and alive/dead state will be updated by the server
    public void HitScout(Array scoutPacket, long BulletId) {
        // Here, we want to start the explosion
        // We also want to delete the bullet
        var scout = Scout.Deserialize(scoutPacket);
        _log.Line($"Client was sent a death for id: {scout.MultiplayerID}!");
        var scoutObject = GetScoutById(scout.MultiplayerID);
        scoutObject.PlayExplosion();
        ScoutBullet bullet = GetBulletById(BulletId);
        if (bullet != null) {bullet.QueueFree();}
    }

    ScoutBullet GetBulletById(long id) {
        foreach (var child in MainRef.Bullets.GetChildren()) {
            if (child.Name == id.ToString()) {
                return child as ScoutBullet;
            }
        }
        return null;
    }

    ScoutClient GetScoutById(long id)
    {
        string idStr = id.ToString();
        foreach (var child in MainRef.Scouts.GetChildren())
        {
            if (child.Name == idStr)
            {
                return child as ScoutClient;
            }
        }

        return null;
    }
}
