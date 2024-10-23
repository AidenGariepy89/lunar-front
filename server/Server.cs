using Godot;
using System;

namespace Server;

public partial class Server : Node2D
{
    Logger _log;

    public override void _Ready()
    {
        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;

        var peer = new ENetMultiplayerPeer();
        _log = new Logger(-1, "server");

        var err = peer.CreateServer(Constants.Port, Constants.MaxPlayers);
        if (err != Error.Ok)
        {
            throw new Exception(err.ToString());
        }

        Multiplayer.MultiplayerPeer = peer;

        _log.MultiplayerId = peer.GetUniqueId();
        _log.Line("Server running.");
    }

    /// Every peer
    void PeerConnected(long id)
    {
    }

    /// Every peer
    void PeerDisconnected(long id)
    {
    }
}
