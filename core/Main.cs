using System.Linq;
using Godot;
using Godot.Collections;

namespace Core;

/// Implements all RPCs and passes down to server or client.
public partial class Main : Node2D
{
    [Export]
    public PackedScene ServerScene;
    [Export]
    public PackedScene ClientScene;
    [Export]
    public PackedScene BulletScene;

    public int ScoreEarth = 0;
    public int ScoreMars = 0;

    public Map Map;
    public Cam Cam;
    public Minimap Minimap;

    public Node2D Scouts;
    public Node2D Bullets;

    Server.Server _server = null;
    Client.Client _client = null;

    public override void _Ready()
    {
        Scouts = GetNode<Node2D>("Scouts");
        Bullets = GetNode<Node2D>("Bullets");

        Map = GetNode<Map>("Map");
        Map.Visible = false;

        Cam = GetNode<Cam>("Cam");
        Cam.Instantiate(Map.TopLeft, Map.BottomRight);

        Minimap = GetNode<Minimap>("Cam/Minimap");

        if (OS.GetCmdlineArgs().Contains("--server"))
        {
            var server = ServerScene.Instantiate<Server.Server>();
            server.MainRef = this;
            _server = server;
            AddChild(server);
        }
        else
        {
            var client = ClientScene.Instantiate<Client.Client>();
            client.MainRef = this;
            _client = client;
            AddChild(client);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void DeliverInput(Array input)
    {
        if (_server == null)
        {
            return;
        }

        _server.DeliverInput(input);
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void ReceiveSync(
        long seqNum,
        Array<Array> scouts,
        Array<Array> bullets,
        Array earth,
        Array mars
    )
    {
        if (_client == null)
        {
            return;
        }

        _client.ReceiveSync(seqNum, scouts, bullets, earth, mars);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void SpawnNewScout(Array scoutPacket)
    {
        if (_client == null)
        {
            return;
        }

        _client.SpawnNewScout(scoutPacket);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void JoinGame(Array<Array> scouts, Array earth, Array mars)
    {
        if (_client == null)
        {
            return;
        }

        _client.JoinGame(scouts, earth, mars);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void SpawnNewBullet(long shotById, Array bulletPacket)
    {
        var data = BulletSpawnPacket.Deconstruct(bulletPacket);

        var bullet = BulletScene.Instantiate<ScoutBullet>();
        bullet.BulletId = data.BulletId;
        bullet.Position = data.Position;
        bullet.Velocity = data.Velocity;
        bullet.Rotation = data.Rotation;
        bullet.Faction = data.Faction;
        bullet.Initialize(Map);

        Bullets.AddChild(bullet);

        if (_client != null)
        {
            _client.BulletShot(shotById);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void HitScout(Array scoutPacket, long bulletId) {
        if (_client != null)
        {
            _client.HitScout(scoutPacket, bulletId);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void PlanetBulletHit(int earthScore, int marsScore, long bulletId, Array planetData)
    {
        ScoreEarth = earthScore;
        ScoreMars = marsScore;

        var bullet = GetBulletById(bulletId);
        if (bullet != null)
        {
            bullet.QueueFree();
        }

        if (_client != null)
        {
            // client stuff
        }
    }

    public ScoutBullet GetBulletById(long bulletId)
    {
        foreach (var child in Bullets.GetChildren())
        {
            var bullet = child as ScoutBullet;
            if (bullet.BulletId == bulletId)
            {
                return bullet;
            }
        }
        
        return null;
    }
}
