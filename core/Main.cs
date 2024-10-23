using System.Linq;
using Godot;
using Godot.Collections;

namespace Core;

/// Implements all RPCs and passes down to server or client.
public partial class Main : Node2D, NetworkObject
{
    [Export]
    public PackedScene ServerScene;
    [Export]
    public PackedScene ClientScene;
    [Export]
    public PackedScene BulletScene;

    public Map Map;
    public Cam Cam;

    public Node2D Scouts;
    public Node2D Bullets;

    NetworkObject _networkObject;

    public override void _Ready()
    {
        Scouts = GetNode<Node2D>("Scouts");
        Bullets = GetNode<Node2D>("Bullets");

        Map = GetNode<Map>("Map");
        Map.Visible = false;

        Cam = GetNode<Cam>("Cam");
        Cam.Instantiate(Map.TopLeft, Map.BottomRight);

        if (OS.GetCmdlineArgs().Contains("--server"))
        {
            var server = ServerScene.Instantiate<Server.Server>();
            server.MainRef = this;
            _networkObject = server;
            AddChild(server);
        }
        else
        {
            var client = ClientScene.Instantiate<Client.Client>();
            client.MainRef = this;
            _networkObject = client;
            AddChild(client);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void DeliverInput(Array input)
    {
        _networkObject.DeliverInput(input);
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void ReceiveSync(long seqNum, Array<Array> syncData)
    {
        _networkObject.ReceiveSync(seqNum, syncData);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void SpawnNewScout(Array scoutPacket)
    {
        _networkObject.SpawnNewScout(scoutPacket);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void SpawnScouts(Array<Array> scouts)
    {
        _networkObject.SpawnScouts(scouts);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void SpawnNewBullet(Array bulletPacket)
    {
        var data = BulletPacket.Deconstruct(bulletPacket);

        var bullet = BulletScene.Instantiate<ScoutBullet>();
        bullet.BulletId = data.BulletId;
        bullet.Position = data.Position;
        bullet.Velocity = data.Velocity;
        bullet.Rotation = data.Rotation;
        bullet.Faction = data.Faction;
        bullet.Initialize(Map);

        Bullets.AddChild(bullet);
    }
}
