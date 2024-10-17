using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MultiplayerScript : Node2D
{
	[Export]
	private int PORT_NUMBER = 17253;
	[Export]
	private string address = "127.0.0.1";

	private ENetMultiplayerPeer peer;
 
	private List<int> currentPlayers = new List<int> {}; // No current players to start, obv

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
			// If it's a client, load main menu scene and attach the JoinGame method to the join button
			var menuScene = ResourceLoader.Load<PackedScene>("res://scenes/MainMenu.tscn").Instantiate<Control>();
			menuScene.Name = "MenuScene"; // Mostly so I can delete it later
			GetTree().Root.AddChild(menuScene);
			var button = menuScene.GetChild<Button>(0); // I assume this works, since menuScene is added to the scene as a child?
			// Pretty sure index 0 is the button, but not 100%
			button.ButtonUp += JoinGame;
		}
	}

	private void HostGame() {
		peer = new ENetMultiplayerPeer();
		var status = peer.CreateServer(PORT_NUMBER, 40); // Max players = 40 is arbitrary
		if (status != Error.Ok) {
			GD.Print("Could not establish server! Error: " + status.ToString());
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		// Starts the game
		var scene = ResourceLoader.Load<PackedScene>("res://scenes/main.tscn").Instantiate<Node2D>(); // Assuming main is the main scene
		GetTree().Root.AddChild(scene);
		// Not using an Rpc, since that would either
		// A: Restart the game for all connected clients, or
		// B: Just call only on the server anyway :/
	}

	private void JoinGame() {
		// Very similar to host
		peer = new ENetMultiplayerPeer();
		var status = peer.CreateClient(address, PORT_NUMBER);
		if (status != Error.Ok) {
			GD.Print("Could not establish client! Error: " + status.ToString());
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;

		// Here, we need to instantiate the player's scout

		// Loading other players will be taken care of in PeerConnected and ConnectedToServer
		// which should be called as soon as this peer is made
	}

	// Runs on all peers
	private void PeerConnected(long id) {
		// I would like this to confirm the existence of all players and add the new player to all old player's games,
		// also adding all the old player's to the new player's game

		Rpc("SendPlayerInfo", id);
		// Then we enter the game:
		var scene = ResourceLoader.Load<PackedScene>("res://scenes/main.tscn").Instantiate<Node2D>();
		GetTree().Root.AddChild(scene);
	}

	// Runs on all peers
	private void PeerDisconnected(long id) {
		// I would like this to remove the existence of the peer from all players' games
		Rpc("RemovePlayerInfo", id);
	}

	// Runs on just the client that connected
	// In the tutorial code, the sendplayerdata was RpcId'd only to the server
	// However, I don't understand why thats done; thus, i will rpc the connected clients data to all other clients
	private void ConnectedToServer() {
		GD.Print("Connection with server successful!");
		// We need to get the data from the server about all the current players
	}

	// Runs on just the client
	private void ConnectionFailed() {
		GD.Print("Connection to server failed!");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void SendPlayerData(int id) {
		// Runs on every client
		// Add the player to every client's list of players
		if (!currentPlayers.Contains(id)) {
			currentPlayers.Add(id);
			// Also instantiate the new scout using the method in the game node

		} else {
			throw new Exception("What in the world! A client tried to join with an ID already in use!");
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RemovePlayerInfo(int id) {
		// Runs on every client
		// Remove the player from every client's list of players
		if (currentPlayers.Contains(id)) {
			currentPlayers.Remove(id);
			// Also remove the instantiated scout using the method in the game node
			
		} else {
			throw new Exception("What in the world! A client tried to leave with an ID that wasn't in the game in the first place!");
		}
	}
}