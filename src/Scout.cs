using Godot;
using System;

public partial class Scout : Area2D
{
	[Export]
	public float GearOneThrustMain = 600;
	[Export]
	public float GearOneThrustForwardAxis = 300;
	[Export]
	public float GearOneThrustSideAxis = 300;
	[Export]
	public float GearOneCorrectionThrust = 200;
	[Export]
	public float GearOneMaxSpeed = 1000;

	static Vector2 NearZero = Vector2.One * 5.0f;

	Vector2 _velocity;
	bool _thrustForward = false;
	bool _thrustBackward = false;
	bool _thrustRight = false;
	bool _thrustLeft = false;

	GravityWell _gravityWell = null;

	Vector2 _displayVec = Vector2.Zero;
	Vector2 _displayVec2 = Vector2.Zero;
	Vector2 _displayVec3 = Vector2.Zero;
	Vector2 _displayVec4 = Vector2.Zero;

	bool _debugValue;

	public override void _Ready()
	{
		_velocity = Vector2.Zero;

		_debugValue = true;


		// Take this out of final commit, as it is on the list as Aiden's
		// Multiplayer code for the scout: assumes the name of this node == its multiplayer peer id
		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name));
		// This is all the multiplayer code that the scout really needs, as long as it has a MultiplayerSynchronizer child node
		// For now, the thrusters are not synced, since they aren't accessible to the multiplayer synchronizer
		// If they were made public or visible in the editor I guess, I'm not an expert in it though so for now I'm just gonna stick with position and rotation 
	}

	public override void _Draw()
	{
		if (_thrustForward)
		{
			DrawCircle(Vector2.Left * 30, 20, Colors.Yellow);
		}
		if (_thrustBackward)
		{
			DrawCircle(Vector2.Right * 20, 10, Colors.Yellow);
		}
		if (_thrustRight)
		{
			DrawCircle(Vector2.Up * 20, 10, Colors.Yellow);
		}
		if (_thrustLeft)
		{
			DrawCircle(Vector2.Down * 20, 10, Colors.Yellow);
		}

		DrawLine(Vector2.Zero, _displayVec.Rotated(-Rotation), Colors.Red);
		DrawLine(Vector2.Zero, _displayVec2.Rotated(-Rotation), Colors.Green);
		DrawLine(Vector2.Zero, _displayVec3.Rotated(-Rotation), Colors.Blue);
		DrawLine(Vector2.Zero, _displayVec4.Rotated(-Rotation), Colors.Cyan);
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;

		if (_debugValue && Input.IsActionJustPressed("debug"))
		{
			_debugValue = false;
		}
		else if (!_debugValue && Input.IsActionJustPressed("debug"))
		{
			_debugValue = true;
		}

		Vector2 _mousePosition = GetGlobalMousePosition();
		Vector2 r = _mousePosition - Position;

		if (r != Vector2.Zero)
		{
			Rotation = r.Angle();
		}

		Vector2 thrust = Vector2.Zero;

		_thrustForward = Input.IsActionPressed("thrust_forward");
		_thrustBackward = Input.IsActionPressed("thrust_backward");
		_thrustLeft = Input.IsActionPressed("thrust_left");
		_thrustRight = Input.IsActionPressed("thrust_right");
		if (_thrustForward)
		{
			thrust.X += GearOneThrustMain;
		}
		if (_thrustBackward)
		{
			thrust.X -= GearOneThrustForwardAxis;
		}
		if (_thrustRight)
		{
			thrust.Y += GearOneThrustSideAxis;
		}
		if (_thrustLeft)
		{
			thrust.Y -= GearOneThrustSideAxis;
		}

		Vector2 acceleration = thrust.Rotated(Rotation);
		if (_debugValue)
		{
			acceleration = InertialDampners(acceleration);
		}

		_velocity += acceleration * dt;

		Position += _velocity * dt;

		QueueRedraw();
	}

	Vector2 InertialDampners(Vector2 inputThrust)
	{
		float thrustThreshold = 10.0f;

		bool isInputThrust = !inputThrust.IsZeroApprox();

		if (!isInputThrust && _velocity < NearZero && _velocity > -NearZero)
		{
			_velocity = Vector2.Zero;
			return inputThrust;
		}

		Vector2 targetVelocity = Vector2.Zero;
		Vector2 correctionThrust;
		Vector2 thrustOnAxis;

		if (inputThrust.IsZeroApprox())
		{
			correctionThrust = (targetVelocity - _velocity).Normalized() * GearOneCorrectionThrust;
			thrustOnAxis = correctionThrust.Rotated(-Rotation);

			if (thrustOnAxis.X > thrustThreshold)
			{
				_thrustForward = true;
			}
			if (thrustOnAxis.X < -thrustThreshold)
			{
				_thrustBackward = true;
			}
			if (thrustOnAxis.Y > thrustThreshold)
			{
				_thrustRight = true;
			}
			if (thrustOnAxis.Y < -thrustThreshold)
			{
				_thrustLeft = true;
			}

			return correctionThrust;
		}

		targetVelocity = inputThrust.Normalized() * GearOneMaxSpeed;
		correctionThrust = targetVelocity - _velocity;
		thrustOnAxis = correctionThrust.Rotated(-Rotation);
		Vector2 actualThrust = inputThrust;

		if (thrustOnAxis.X > thrustThreshold)
		{
			if (!_thrustForward)
			{
				_thrustForward = true;
				actualThrust.X += GearOneThrustForwardAxis;
			}
		}
		if (thrustOnAxis.X < -thrustThreshold)
		{
			if (!_thrustBackward)
			{
				_thrustBackward = true;
				actualThrust.X -= GearOneThrustForwardAxis;
			}
		}
		if (thrustOnAxis.Y > thrustThreshold)
		{
			if (!_thrustRight)
			{
				_thrustRight = true;
				actualThrust.Y += GearOneThrustSideAxis;
			}
		}
		if (thrustOnAxis.Y < -thrustThreshold)
		{
			if (!_thrustLeft)
			{
				_thrustLeft = true;
				actualThrust.Y -= GearOneThrustSideAxis;
			}
		}

		return correctionThrust;
	}
}
