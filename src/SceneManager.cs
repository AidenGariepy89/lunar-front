using Godot;
using System;
using System.Collections.ObjectModel;
using System.Linq;

public partial class SceneManager : Node2D
{
	[Export]
	private PackedScene playerObject;
	// Called when the node enters the scene tree for the first time.

	private Collection<ObjectInfo> instantiatedPlayers = new Collection<ObjectInfo>();

	// Seems to only be called once, when the server begins the game
	// Thus I'll use it to set spawn points
	public override void _Ready()
	{
		if (!Multiplayer.IsServer()) {
			GD.Print("Client is running this!");
		}

		foreach (var player in GameManager.playerInfos) {
			Scout currentPlayer = playerObject.Instantiate<Scout>();
			currentPlayer.Name = player.Name;
			AddChild(currentPlayer);
			foreach (Node2D spawnpoint in GetTree().GetNodesInGroup("TeamSpawns")) {
				if (spawnpoint.Name == player.team.ToString()) { // Compares the spawnpoints "0" and "1" to the team, which should be hopefully 0 or 1
					player.Position = spawnpoint.GlobalPosition;
					currentPlayer.GlobalPosition = player.Position;
				}
			}
			if (!instantiatedPlayers.Contains(player)) {
				instantiatedPlayers.Add(player);
			}
		}
	}
	public void ReloadGameObjects()
	{
		// Functionally reaload every player
		// Basically unloads every object instanced from objectInfos, then reinstances them
		GD.Print("Client called reload game objects!");
		foreach (var obj in instantiatedPlayers) {
			// first, unload all
			// perhaps an inefficient way to go through and remove it, we can improve if we have time
			foreach (Node2D child in GetChildren()) {
				if (child.Name == obj.Name) {
					RemoveChild(child);
					child.QueueFree(); // Frees the memory kinda
				}
			}
		}
		instantiatedPlayers.Clear();
		// Reinstance each player in another loop, just in case (i don't know if maybe getchildren would get the newly instanciated ones to if we did it all in one)
		foreach (var player in GameManager.playerInfos) {
			// Its pretty similar to ready
			Scout currentPlayer = playerObject.Instantiate<Scout>();
			currentPlayer.Name = player.Name;
			AddChild(currentPlayer);
			foreach (Node2D spawnpoint in GetTree().GetNodesInGroup("TeamSpawns")) {
				if (spawnpoint.Name == player.team.ToString()) { // Compares the spawnpoints "0" and "1" to the team, which should be hopefully 0 or 1
					currentPlayer.GlobalPosition = player.Position;
				}
			}
			if (!instantiatedPlayers.Contains(player)) { // It should always not contain player, since we cleared instantiated nodes earlier
				instantiatedPlayers.Add(player);
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// This node originally has 5 children, so compare the number of instantiatedPlayers to children - 5
		if (instantiatedPlayers.Count != GameManager.playerInfos.Count) {
			ReloadGameObjects();
			GD.Print(Multiplayer.GetUniqueId() + " just reloaded!");
		}
	}
}
