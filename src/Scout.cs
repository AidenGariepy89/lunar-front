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

    [Export]
    public float ShootDelay;
    [Export]
    public float ShootBulletSpeed;

    public PackedScene BulletScene;
    public Texture2D SpriteForwardThrust;
    public Texture2D SpriteBackwardThrust;
    public Texture2D SpriteLeftwardThrust;
    public Texture2D SpriteRightwardThrust;

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

    Timer _shootTimer;
    bool _shooting = false;

    GpuParticles2D _backBoost;

    MultiplayerSynchronizer _multiplayer;

    public override void _Ready()
    {
        _velocity = Vector2.Zero;

        _debugValue = true;

        _shootTimer = GetNode<Timer>("ShootTimer");
        _shootTimer.OneShot = true;
        _shootTimer.WaitTime = ShootDelay;
        _shootTimer.Timeout += FireBullet;

        _backBoost = GetNode<GpuParticles2D>("BackBoost");

        if (Name == "Scout")
        {
            throw new ApplicationException("Cannot run scene alone.");
        }
        _multiplayer = GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer");
        _multiplayer.SetMultiplayerAuthority(int.Parse(Name));
    }

    public override void _Draw()
    {
        var offset = new Vector2(-16, -12);
        if (_thrustForward)
        {
            DrawTexture(SpriteForwardThrust, offset);
        }
        if (_thrustBackward)
        {
            DrawTexture(SpriteBackwardThrust, offset);
        }
        if (_thrustRight)
        {
            DrawTexture(SpriteRightwardThrust, offset);
        }
        if (_thrustLeft)
        {
            DrawTexture(SpriteLeftwardThrust, offset);
        }
    }

    public override void _Process(double delta)
    {
        // put this somewhere better
        if (_thrustForward && !_backBoost.Emitting)
        {
            _backBoost.Emitting = true;
        }
        else if (!_thrustForward && _backBoost.Emitting)
        {
            _backBoost.Emitting = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_multiplayer.GetMultiplayerAuthority() != Multiplayer.GetUniqueId())
        {
            return;
        }

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

        if (Input.IsActionPressed("shoot") && !_shooting)
        {
            _shootTimer.Start();
            _shooting = true;
        }
    }

    Vector2 InertialDampners(Vector2 inputThrust)
    {
        float thrustThreshold = 10.0f;

        bool isInputThrust = !inputThrust.IsZeroApprox();

        if (!isInputThrust && VelocityNearZero())
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

    bool VelocityNearZero()
    {
        return _velocity.X > -NearZero.X && _velocity.X < NearZero.X
            && _velocity.Y > -NearZero.Y && _velocity.Y < NearZero.Y;
    }

    void FireBullet()
    {
        var bullet1 = BulletScene.Instantiate<ScoutBullet>();
        bullet1.Position = Position;
        bullet1.Rotation = Rotation;
        bullet1.Velocity = (Vector2.Right * ShootBulletSpeed).Rotated(Rotation);
        bullet1.Velocity += _velocity;

        var bullet2 = BulletScene.Instantiate<ScoutBullet>();
        bullet2.Position = Position;
        bullet2.Rotation = Rotation;
        bullet2.Velocity = (Vector2.Right * ShootBulletSpeed).Rotated(Rotation);
        bullet2.Velocity += _velocity;

        GetTree().Root.GetChild(0).AddChild(bullet1);

        _shooting = false;
    }
}
