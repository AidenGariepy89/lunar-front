using Godot;
using System;
using System.Data.Common;
using System.Linq;

public partial class MultiplayerScript : Control
{
	[Export]
	private int PORT_NUMBER = 17253;
	[Export]
	private string address = "127.0.0.1";

	private double deltaSum = 0.0;

	private ENetMultiplayerPeer peer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		if (OS.GetCmdlineArgs().Contains("--server")) { // When you run the server with --headless as well it doesn't pop up a window
			HostGame();
		} else {
			// Otherwise, show the main menu
		}
	}

	// Runs when connection failed
	// Only runs on client
	private void ConnectionFailed()
	{
		GD.Print("Client's connection failed ;(");
	}

	// Runs when this client is connected to server
	// Only runs on client
	private void ConnectedToServer()
	{
		GD.Print("Connected to server!");
		string name = Multiplayer.GetUniqueId().ToString();
		// This Rpc sends it only to the multiplayer peer with an id of 1, i.e. the server
		RpcId(1, "SendPlayerInfo", name, Multiplayer.GetUniqueId());
		
		if (!Multiplayer.IsServer()) {
			EnterGame(false);
		}
	}

	// Runs when peer is disconnected, id is the id of the player
	// Runs on all peers
	private void PeerDisconnected(long id)
	{
		GD.Print("A peer disconnected! " + id.ToString());
		ObjectInfo objectInfToRemove = new ObjectInfo(); // Would be very bad if this actually stayed like this
		foreach (var obj in GameManager.playerInfos) {
			if (obj.Id == id) {
				objectInfToRemove = obj; // Luckily, this loop should find the object 100% of the time. Maybe still not good practice tho
			}
		}
		GameManager.playerInfos.Remove(objectInfToRemove);
		// The tutorial has you QueueFree here, but since I have a mechanism to reload nodes based on objectInfos, I can do that in the reload function
	}

	// Runs when peer is connected, id is id of the player
	// Runs on all peers
	private void PeerConnected(long id)
	{
		GD.Print("A new peer connected! " + id.ToString());
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Mostly for debug
	}

	private void HostGame() {
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(PORT_NUMBER, 50); // 50 is the max clients
		if (error != Error.Ok) {
			GD.Print("Server setup threw an error: " + error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder); // This is important
		Multiplayer.MultiplayerPeer = peer; // Connect to your own server, maybe take this out to do headless?
		// This is important to count the server as part of the game, or else it won't get rpc calls
		// To call an Rpc...
		Rpc("EnterGame", true);
	}

	public void _on_host_button_down() {
		HostGame();
		GD.Print("Started game");
		SendPlayerInfo("1", 1);
	}
	public void _on_join_button_down() {
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateClient(address, PORT_NUMBER);
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
	}

	// TransferModeEnum.Reliable == TCP (I think, check tho) --- this is the default, so we don't have to include it in the args
	// TransferModeEnum.Unreliable == I don'r entirely know yet
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void EnterGame(bool isHost) {
		var scene = ResourceLoader.Load<PackedScene>("res://scenes/lobbyScene.tscn").Instantiate<Node2D>();
		GetTree().Root.AddChild(scene);
		this.Hide();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void SendPlayerInfo(string name, int id) {
		GD.Print("Sent player info from " + name);
		ObjectInfo playerInfo = new ObjectInfo(){
			Name=name,
			Id=id,
			team = Convert.ToInt32(id > (2^32 / 2)) // This is totally random, fix later
		};
		if (!GameManager.playerInfos.Contains(playerInfo)) {
			GameManager.playerInfos.Add(playerInfo);
		}
		if (Multiplayer.IsServer()) {
			foreach (ObjectInfo i in GameManager.playerInfos) {
				Rpc("SendPlayerInfo", i.Name, i.Id);
				GD.Print("Sending info from player: " + i.Name + " to all clients");
				// The problem with this loop is not looping over all players properly
			}
		}
	}
}
