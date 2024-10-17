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

    [Export]
    public bool ThrustForward = false;
    [Export]
    public bool ThrustBackward = false;
    [Export]
    public bool ThrustRight = false;
    [Export]
    public bool ThrustLeft = false;

    static Vector2 NearZero = Vector2.One * 5.0f;

    Vector2 _velocity;

    GravityWell _gravityWell = null;

    Vector2 _displayVec = Vector2.Zero;
    Vector2 _displayVec2 = Vector2.Zero;
    Vector2 _displayVec3 = Vector2.Zero;
    Vector2 _displayVec4 = Vector2.Zero;

    Timer _shootTimer;
    bool _shooting = false;

    GpuParticles2D _backBoost;

    MultiplayerSynchronizer _multiplayer;

    Game _game = null;

    public override void _Ready()
    {
        _velocity = Vector2.Zero;

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

    public void Instantiate(Game game)
    {
        _game = game;
    }

    public override void _Draw()
    {
        var offset = new Vector2(-16, -12);
        if (ThrustForward)
        {
            DrawTexture(SpriteForwardThrust, offset);
        }
        if (ThrustBackward)
        {
            DrawTexture(SpriteBackwardThrust, offset);
        }
        if (ThrustRight)
        {
            DrawTexture(SpriteRightwardThrust, offset);
        }
        if (ThrustLeft)
        {
            DrawTexture(SpriteLeftwardThrust, offset);
        }
    }

    public override void _Process(double delta)
    {
        // put this somewhere better
        if (ThrustForward && !_backBoost.Emitting)
        {
            _backBoost.Emitting = true;
        }
        else if (!ThrustForward && _backBoost.Emitting)
        {
            _backBoost.Emitting = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        QueueRedraw();

        if (_multiplayer.GetMultiplayerAuthority() != Multiplayer.GetUniqueId())
        {
            return;
        }

        float dt = (float)delta;

        Vector2 _mousePosition = GetGlobalMousePosition();
        Vector2 r = _mousePosition - Position;

        if (r != Vector2.Zero)
        {
            Rotation = r.Angle();
        }

        Vector2 thrust = Vector2.Zero;

        ThrustForward = Input.IsActionPressed("thrust_forward");
        ThrustBackward = Input.IsActionPressed("thrust_backward");
        ThrustLeft = Input.IsActionPressed("thrust_left");
        ThrustRight = Input.IsActionPressed("thrust_right");
        if (ThrustForward)
        {
            thrust.X += GearOneThrustMain;
        }
        if (ThrustBackward)
        {
            thrust.X -= GearOneThrustForwardAxis;
        }
        if (ThrustRight)
        {
            thrust.Y += GearOneThrustSideAxis;
        }
        if (ThrustLeft)
        {
            thrust.Y -= GearOneThrustSideAxis;
        }

        Vector2 acceleration = InertialDampners(thrust.Rotated(Rotation));

        _velocity += acceleration * dt;

        Position += _velocity * dt;

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
                ThrustForward = true;
            }
            if (thrustOnAxis.X < -thrustThreshold)
            {
                ThrustBackward = true;
            }
            if (thrustOnAxis.Y > thrustThreshold)
            {
                ThrustRight = true;
            }
            if (thrustOnAxis.Y < -thrustThreshold)
            {
                ThrustLeft = true;
            }

            return correctionThrust;
        }

        targetVelocity = inputThrust.Normalized() * GearOneMaxSpeed;
        correctionThrust = targetVelocity - _velocity;
        thrustOnAxis = correctionThrust.Rotated(-Rotation);
        Vector2 actualThrust = inputThrust;

        if (thrustOnAxis.X > thrustThreshold)
        {
            if (!ThrustForward)
            {
                ThrustForward = true;
                actualThrust.X += GearOneThrustForwardAxis;
            }
        }
        if (thrustOnAxis.X < -thrustThreshold)
        {
            if (!ThrustBackward)
            {
                ThrustBackward = true;
                actualThrust.X -= GearOneThrustForwardAxis;
            }
        }
        if (thrustOnAxis.Y > thrustThreshold)
        {
            if (!ThrustRight)
            {
                ThrustRight = true;
                actualThrust.Y += GearOneThrustSideAxis;
            }
        }
        if (thrustOnAxis.Y < -thrustThreshold)
        {
            if (!ThrustLeft)
            {
                ThrustLeft = true;
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
        _game.MainRef.RpcSendNewBullet(
            Position,
            (Vector2.Right * ShootBulletSpeed).Rotated(Rotation) + _velocity
        );

        _shooting = false;
    }
}
