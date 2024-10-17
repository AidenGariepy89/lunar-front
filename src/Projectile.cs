using Godot;
using System;

public partial class Projectile : CharacterBody2D
{
	public const float Speed = 300.0f;
	private Vector2 direction = new Vector2();

	public double traveled = 0.0;

	public override void _Ready()
	{
		direction = new Vector2(1, 0).Rotated(Rotation);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		velocity = Speed * direction;

		Velocity = velocity;
		traveled += velocity.Length() * delta;
		if (traveled >= 1000f) {
			foreach (var child in GetTree().Root.GetChildren()) {
				if (child.Name == Name) {
					RemoveChild(child);
					child.QueueFree(); // Frees the memory kinda
				}
			}
		} // This just deletes itself if its traveled long enough
		MoveAndSlide();
	}
}
