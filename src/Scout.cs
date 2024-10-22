using Godot;
using System;
using System.Threading.Tasks;

public enum Faction
{
    Earth,
    Mars,
}

public partial class Scout : Area2D
{
    [Signal]
    public delegate void ScoutDiedEventHandler();

    enum State
    {
        Alive,
        Dead,
    }

    [Export]
    public Texture2D SpriteForwardThrust;
    [Export]
    public Texture2D SpriteBackwardThrust;
    [Export]
    public Texture2D SpriteLeftwardThrust;
    [Export]
    public Texture2D SpriteRightwardThrust;

    [Export]
    public PackedScene ExplosionScene;

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

    [Export]
    public bool ThrustForward = false;
    [Export]
    public bool ThrustBackward = false;
    [Export]
    public bool ThrustRight = false;
    [Export]
    public bool ThrustLeft = false;

    [Export]
    public int MaxHealth = 3;

    [Export]
    public double RespawnDelay = 5.0;

    [Export]
    /// Set to export so it can be tracked by MultiplayerSynchronizer
    public int Health { get => _health; set => SetHealth(value); }

    public long MultiplayerID;
    public Faction Faction = Faction.Earth;

    static Vector2 NearZero = Vector2.One * 5.0f;

    GpuParticles2D _backBoost;
    MultiplayerSynchronizer _multiplayer;
    Sprite2D _sprite;
    Timer _shootTimer;
    Node2D _bulletSpawnLeft;
    Node2D _bulletSpawnRight;

    Vector2 _velocity;
    int _health;
    bool _shooting = false;
    bool _alternatingFireLeft = false;
    State _state = State.Dead;

    Game _game = null;

    public override void _Ready()
    {
        _health = MaxHealth;
        _velocity = Vector2.Zero;
        AreaEntered += BulletHit;
    }

    public void Instantiate(long id, Game game)
    {
        MultiplayerID = id;

        _game = game;

        ScoutDied += () =>
        {
            _game.MainRef.RpcPlayerDied(MultiplayerID);
        };

        _shootTimer = GetNode<Timer>("ShootTimer");
        _shootTimer.OneShot = true;
        _shootTimer.WaitTime = ShootDelay;
        _shootTimer.Timeout += FireBullet;

        _bulletSpawnLeft = GetNode<Node2D>("BulletSpawnLeft");
        _bulletSpawnRight = GetNode<Node2D>("BulletSpawnRight");

        _backBoost = GetNode<GpuParticles2D>("BackBoost");

        _sprite = GetNode<Sprite2D>("Sprite2D");

        if (Name == "Scout")
        {
            throw new ApplicationException("Cannot run scene alone.");
        }
        _multiplayer = GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer");
        _multiplayer.SetMultiplayerAuthority((int)id);

        Reset();
    }

    public async void SetHealth(int value)
    {
        _health = value;
        if (_health <= 0)
        {
            _health = 0;

            EmitSignal(SignalName.ScoutDied);

            await Die();

            var respawn = _game.RequestRespawnCoords();
            Position = respawn;
        }
    }

    public async Task Die()
    {
        _state = State.Dead;
        _sprite.Visible = false;
        _backBoost.Emitting = false;

        var explosion = ExplosionScene.Instantiate<ScoutExplosion>();
        AddChild(explosion);

        await ToSignal(GetTree().CreateTimer(RespawnDelay), SceneTreeTimer.SignalName.Timeout);

        Reset();
    }

    public override void _Draw()
    {
        if (_state != State.Alive)
        {
            return;
        }

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
        if (_state != State.Alive)
        {
            return;
        }

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

        if (_state == State.Alive)
        {
            Vector2 _mousePosition = GetGlobalMousePosition();
            Vector2 r = _mousePosition - Position;

            if (r != Vector2.Zero)
            {
                Rotation = r.Angle();
            }
        }

        Vector2 thrust = Vector2.Zero;

        if (_state == State.Alive)
        {
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
        }

        Vector2 acceleration = InertialDampners(thrust.Rotated(Rotation));

        _velocity += acceleration * dt;

        Position += _velocity * dt;
        Position = Position.Clamp(_game.TopLeft, _game.BottomRight);

        if (_state == State.Alive)
        {
            if (Input.IsActionPressed("shoot") && !_shooting)
            {
                _shootTimer.Start();
                _shooting = true;
            }

            if (Input.IsActionJustPressed("debug"))
            {
                Health = 0;
            }
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
        if (_alternatingFireLeft)
        {
            _game.MainRef.RpcSendNewBullet(
                _bulletSpawnLeft.GlobalPosition,
                (Vector2.Right * ShootBulletSpeed).Rotated(Rotation) + _velocity,
                Rotation,
                Faction
            );
        }
        else
        {
            _game.MainRef.RpcSendNewBullet(
                _bulletSpawnRight.GlobalPosition,
                (Vector2.Right * ShootBulletSpeed).Rotated(Rotation) + _velocity,
                Rotation,
                Faction
            );
        }
        _alternatingFireLeft = !_alternatingFireLeft;
        _shooting = false;
    }

    void Reset()
    {
        Health = MaxHealth;
        _state = State.Alive;
        _velocity = Vector2.Zero;
        _sprite.Visible = true;
    }

    void BulletHit(Area2D other)
    {
        if (MultiplayerID != Multiplayer.GetUniqueId())
        {
            return;
        }

        if (other is not ScoutBullet)
        {
            return;
        }

        if (_state == State.Dead)
        {
            return;
        }

        var bullet = other as ScoutBullet;

        if (bullet.Faction == Faction)
        {
            return;
        }

        bullet.QueueFree();

        Health = 0;
    }
}
