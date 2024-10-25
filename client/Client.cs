using Godot;
using Godot.Collections;
using Core;

namespace Client;

public partial class Client : Node2D
{
    [Export]
    public PackedScene ScoutScene;
    [Export]
    public PackedScene TitleScene;
    [Export]
    public PackedScene HudScene;

    public Main MainRef;
    public PlanetClient Earth;
    public PlanetClient Mars;
    public Hud Hud = null;

    bool _connected = false;
    Logger _log;
    long _currentSeqNum = -1;

    Timer _inputTimer;
    InputCollector _inputCollector;
    AudioStreamPlayer _music;

    ScoutClient _playerScout = null;

    bool _audioMuted = false;

    TitleScreen _title = null;

    public override void _Ready()
    {
        Earth = GetNode<PlanetClient>("Earth");
        Earth.Visible = false;
        Mars = GetNode<PlanetClient>("Mars");
        Mars.Visible = false;

        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;

        _inputTimer = GetNode<Timer>("InputTimer");
        _inputTimer.Timeout += InputTimeout;

        _log = new Logger(-1, "client");
        _inputCollector = GetNode<InputCollector>("InputCollector");

        _music = GetNode<AudioStreamPlayer>("Music");

        SetupTitleScreen();
    }

    void EstablishConnection()
    {
        var address = _title.IPEdit.Text;

        var peer = new ENetMultiplayerPeer();
        var err = peer.CreateClient(address, Constants.Port);
        if (err != Error.Ok)
        {
            _log.Err(err.ToString());
            _title.ErrMsg.Visible = true;
            return;
        }

        Multiplayer.MultiplayerPeer = peer;

        _log.MultiplayerId = peer.GetUniqueId();
        _log.Line("Established client.");
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
            AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), _audioMuted);
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
    public void JoinGame(
        Array<Array> scouts,
        Array<Array> bullets,
        Array earth,
        Array mars
    )
    {
        _log.Line($"Joining game");

        MainRef.Map.Visible = true;

        Earth.Initialize(Planet.Deserialize(earth));
        Earth.Visible = true;
        Mars.Initialize(Planet.Deserialize(mars));
        Mars.Visible = true;

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

        foreach (var bullet in bullets)
        {
            MainRef.CreateBullet(bullet);
        }

        MainRef.Minimap.Initialize(MainRef);

        _title.QueueFree();
        Hud = HudScene.Instantiate<Hud>();
        Hud.Initialize(Earth.Data.Score, Mars.Data.Score);
        GetNode<CanvasLayer>("CanvasLayer").AddChild(Hud);

        _music.Play();

        _inputTimer.Start();
        _inputCollector.StartCollection();
        _connected = true;
    }

    public void ReceiveSync(
        long seqNum,
        Array<Array> scouts,
        Array<Array> bullets,
        Array earth,
        Array mars
    )
    {
        // If seqNum is a past seqNum and we are sure a wrap did not happen, then ignore
        if (seqNum <= _currentSeqNum && (seqNum > 1000 || _currentSeqNum < long.MaxValue - 1000))
        {
            return;
        }
        _currentSeqNum = seqNum;

        foreach (var data in scouts)
        {
            var scoutData = Scout.Deserialize(data);
            GetScoutById(scoutData.MultiplayerID).Sync(scoutData);
        }

        foreach (var data in bullets)
        {
            var bulletPacket = BulletPacket.Deconstruct(data);

            var bullet = MainRef.GetBulletById(bulletPacket.BulletId);
            if (bullet != null)
            {
                bullet.Position = bulletPacket.Position;
            }
        }

        Earth.Sync(Planet.Deserialize(earth));
        Mars.Sync(Planet.Deserialize(mars));
        Hud.Update(Earth.Data.Score, Mars.Data.Score);
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

        if (id != Constants.ServerId)
        {
            GetScoutById(id).QueueFree();

            return;
        }

        Multiplayer.MultiplayerPeer.Close();

        foreach (var scout in MainRef.Scouts.GetChildren()) {
            scout.QueueFree();
        }
        foreach (var bullet in MainRef.Bullets.GetChildren()) {
            bullet.QueueFree();
        }

        Hud.QueueFree();
        _music.Stop();
        MainRef.Map.Visible = false;
        Earth.Visible = false;
        Mars.Visible = false;
        _inputTimer.Stop();
        _inputCollector.FinishCollection();
        _connected = false;
        MainRef.Cam.Target = null;

        SetupTitleScreen();
        MainRef.Minimap.Visible = false;
    }

    void ConnectedToServer()
    {
        _log.Line("Connected to server!");
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
    public void HitScout(Array scoutPacket, long bulletId)
    {
        // Here, we want to start the explosion if dead
        // We also want to delete the bullet
        var scout = Scout.Deserialize(scoutPacket);
        var scoutObject = GetScoutById(scout.MultiplayerID);

        if (scoutObject.Data.CurrentState != scout.CurrentState
            && scout.CurrentState == Scout.State.Dead)
        {
            scoutObject.PlayExplosion();
        }
        else
        {
            scoutObject.PlayHit();
        }

        var bullet = MainRef.GetBulletById(bulletId);
        if (bullet != null)
        {
            bullet.QueueFree();
        }

        scoutObject.Sync(scout);
    }

    public void PlayerHitPlanet(Array scoutData, PlanetClient planet)
    {
        var scout = Scout.Deserialize(scoutData);
        var scoutClient = GetScoutById(scout.MultiplayerID);

        scoutClient.Sync(scout);
        scoutClient.PlayExplosion();

        planet.HitAnimation(scoutClient.Rotation, scoutClient.Position);
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

    void SetupTitleScreen()
    {
        _title = TitleScene.Instantiate<TitleScreen>();
        _title.Initialize();
        _title.JoinButton.Pressed += EstablishConnection;
        GetNode<CanvasLayer>("CanvasLayer").AddChild(_title);
    }
}
