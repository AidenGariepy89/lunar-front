using Godot;
using System;

public partial class Scout : CharacterBody2D
{
	public const float speed = 1.0f;
	public const float damping = 0.99f;
	[Export]
	public PackedScene Projectile;

	public override void _Ready()
	{
		//GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name));
		MultiplayerSynchronizer multiSynch;
		foreach (var obj in GetChildren()) {
			if (obj.Name == "MultiplayerSynchronizer") {
				multiSynch = (MultiplayerSynchronizer) obj; // Because GetChild wasn't working occasionally
				
				multiSynch.SetMultiplayerAuthority(int.Parse(Name));
			}
		}
		base._Ready();
	}
	public override void _Process(double delta)
	{
		if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId()) {
			// This only runs on clients processing their own scout, the rest runs on every scout
			Vector2 velocity = Velocity;

			bool forward = Input.IsActionPressed("ForwardDirection");
			bool left = Input.IsActionPressed("LeftDirection");
			bool right = Input.IsActionPressed("RightDirection");
			bool backwards = Input.IsActionPressed("BackDirection");
			bool shoot = Input.IsActionJustPressed("ShootInput");

			GetNode<Node2D>("RotationNode").LookAt(GetViewport().GetMousePosition());

			if (shoot) {
				Node2D proj = Projectile.Instantiate<Node2D>();
				proj.RotationDegrees = GetNode<Node2D>("RotationNode").RotationDegrees;
				proj.GlobalPosition = GetChild<Node2D>(1).GetChild<Node2D>(1).GlobalPosition;
				GetTree().Root.AddChild(proj);
			}

			velocity.Y -= speed * (forward ? 1 : 0) * damping;
			velocity.Y += speed * (backwards ? 1 : 0) * damping;
			velocity.X += speed * (right ? 1 : 0) * damping;
			velocity.X -= speed * (left ? 1 : 0) * damping;

			Velocity = velocity;
			MoveAndSlide();
		}
	}
}
