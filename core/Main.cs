using System.Linq;
using Godot;
using Godot.Collections;

namespace Core;

public partial class Main : Node2D
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

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    void SpawnNewScout(Array scoutPacket)
    {
        _networkObject.SpawnNewScout(scoutPacket);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void SpawnScouts(Array<Array> scouts)
    {
        _networkObject.SpawnScouts(scouts);
    }
}
