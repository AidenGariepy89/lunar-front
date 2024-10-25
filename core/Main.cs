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
            GetNode<Node2D>("Process").AddChild(server);
        }
        else
        {
            var client = ClientScene.Instantiate<Client.Client>();
            client.MainRef = this;
            _client = client;
            GetNode<Node2D>("Process").AddChild(client);
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
    public void JoinGame(
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

        _client.JoinGame(scouts, bullets, earth, mars);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void SpawnNewBullet(Array bulletSpawnPacket)
    {
        var bullet = CreateBullet(bulletSpawnPacket);

        if (_client != null)
        {
            _client.BulletShot(bullet.ShotById);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void HitScout(Array scoutPacket, long bulletId)
    {
        if (_client == null)
        {
            return;
        }

        _client.HitScout(scoutPacket, bulletId);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void PlanetBulletHit(int earthScore, int marsScore, long bulletId, Array planetData)
    {
        if (_client == null)
        {
            return;
        }

        _client.Earth.Data.Score = earthScore;
        _client.Mars.Data.Score = marsScore;
        _client.Hud.Update(_client.Earth.Data.Score, _client.Mars.Data.Score);

        var data = Planet.Deserialize(planetData);
        var planet = (data.Faction == Faction.Earth) ? _client.Earth : _client.Mars;
        planet.Sync(data);

        var bullet = GetBulletById(bulletId);

        planet.HitAnimation(bullet.Rotation, bullet.Position);

        if (bullet != null)
        {
            bullet.QueueFree();
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void PlanetScoutHit(int earthScore, int marsScore, Array scoutData, Array planetData)
    {
        if (_client == null)
        {
            return;
        }

        _client.Earth.Data.Score = earthScore;
        _client.Mars.Data.Score = marsScore;
        _client.Hud.Update(_client.Earth.Data.Score, _client.Mars.Data.Score);

        var data = Planet.Deserialize(planetData);
        var planet = (data.Faction == Faction.Earth) ? _client.Earth : _client.Mars;
        planet.Sync(data);

        _client.PlayerHitPlanet(scoutData, planet);
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

    public ScoutBullet CreateBullet(Array bulletSpawnPacket)
    {
        var data = BulletSpawnPacket.Deconstruct(bulletSpawnPacket);

        var bullet = BulletScene.Instantiate<ScoutBullet>();
        bullet.BulletId = data.BulletId;
        bullet.ShotById = data.ShotById;
        bullet.Position = data.Position;
        bullet.Velocity = data.Velocity;
        bullet.Rotation = data.Rotation;
        bullet.Faction = data.Faction;
        bullet.Initialize(Map);

        Bullets.AddChild(bullet);

        return bullet;
    }
}
