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

    NetworkObject _networkObject;

    public override void _Ready()
    {
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
    public void ReceiveSync(Array<Array> syncData)
    {
        _networkObject.ReceiveSync(syncData);
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
}
