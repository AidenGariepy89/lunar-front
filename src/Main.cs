using Godot;
using System;
using System.Linq;

public partial class Main : Node2D
{
    [Export]
    public PackedScene GameScene;

    ENetMultiplayerPeer peer;

    Lobby lobby = null;

    Game game = null;

    bool server = false;

    public override void _Ready()
    {
        peer = new ENetMultiplayerPeer();

        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;

        if (OS.GetCmdlineArgs().Contains("--server"))
        {
            Server();
            return;
        }

        lobby = GetNode<Lobby>("CanvasLayer/Lobby");
        lobby.Instantiate(this);
    }

    /// Client setup
    public void StartClient()
    {
        GD.Print("[client] setting up client.");

        var err = peer.CreateClient(Constants.Address, Constants.Port);
        if (err != Error.Ok)
        {
            throw new Exception(err.ToString());
        }

        peer.Host.Compress(ENetConnection.CompressionMode.None);

        Multiplayer.MultiplayerPeer = peer;

        GD.Print("[client] set up client.");
    }

    /// Server setup
    void Server()
    {
        GD.Print("[server] setting up server.");

        server = true;

        var err = peer.CreateServer(Constants.Port, Constants.MaxPlayers);
        if (err != Error.Ok)
        {
            throw new Exception(err.ToString());
        }

        peer.Host.Compress(ENetConnection.CompressionMode.None);

        Multiplayer.MultiplayerPeer = peer;

        GD.Print("[server] server set up.");
    }

    /// Every peer
    void PeerConnected(long id)
    {
        GD.Print($"[peer] peer connected to {id}.");

        if (game == null || id == Constants.ServerId)
        {
            return;
        }

        game.SpawnScout(id);
    }

    /// Every peer
    void PeerDisconnected(long id)
    {
        GD.Print("[peer] peer disconnected.");

        if (server)
        {
            return;
        }

        game.RemoveScout(id);
    }

    /// Client function
    void ConnectedToServer()
    {
        GD.Print("[client] connected to server!");

        game = GameScene.Instantiate<Game>();
        game.Instantiate(this);
        lobby.Visible = false;
        AddChild(game);

        game.SpawnScout(Multiplayer.GetUniqueId());
    }

    /// Client function
    void ConnectionFailed()
    {
        GD.Print("[client] connection failed");
    }

    public void RpcSendNewBullet(Vector2 position, Vector2 velocity)
    {
        Rpc(MethodName.SendNewBullet, position, velocity);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    void SendNewBullet(Vector2 position, Vector2 velocity)
    {
        if (server)
        {
            return;
        }

        game.SpawnScoutBullet(position, velocity);
    }
}
