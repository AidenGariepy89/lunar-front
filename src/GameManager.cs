using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;

public partial class GameManager : Node
{
	// Called when the node enters the scene tree for the first time.
	public static List<ObjectInfo> playerInfos = new List<ObjectInfo>();
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
