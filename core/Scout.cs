using Godot;
using Godot.Collections;

namespace Core;

public enum Faction
{
    Earth,
    Mars,
}

public class Scout
{
    public enum State
    {
        Alive,
        Dead,
    }

    // Constants

    public const float ThrustMain = 600;
    public const float ThrustForwardAxis = 300;
    public const float ThrustSideAxis = 300;
    public const float CorrectionThrust = 200;
    public const float ShootDelay = 0.16f;
    public const float ShootBulletSpeed = 1100f;
    public const float MaxSpeed = 1000;

    public static Vector2 NearZero = Vector2.One * 5.0f;

    public const int MaxHealth = 3;
    public const double RespawnDelay = 5.0;

    // Input Variables
    public bool Forward = false;
    public bool Backward = false;
    public bool Rightward = false;
    public bool Leftward = false;
    public bool Shooting = false;
    public Vector2 Mouse = Vector2.Zero;

    // Visual Variables

    public bool ThrustForward = false;
    public bool ThrustBackward = false;
    public bool ThrustRight = false;
    public bool ThrustLeft = false;

    // State Variables

    public long MultiplayerID;
    public Faction Faction = Faction.Earth;
    public float Health = MaxHealth;
    public State CurrentState = State.Alive;

    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;

    public Scout()
    {
        Position = Vector2.Zero;
        Velocity = Vector2.Zero;
        Rotation = 0.0f;
    }
    
    public Scout Copy()
    {
        var copy = new Scout();

        copy.MultiplayerID = MultiplayerID;
        copy.Faction = Faction;
        copy.Health = Health;
        copy.CurrentState = CurrentState;
        copy.Position = Position;
        copy.Velocity = Velocity;
        copy.Rotation = Rotation;
        copy.ThrustForward = ThrustForward;
        copy.ThrustBackward = ThrustBackward;
        copy.ThrustRight = ThrustRight;
        copy.ThrustLeft = ThrustLeft;
        copy.Forward = Forward;
        copy.Backward = Backward;
        copy.Rightward = Rightward;
        copy.Leftward = Leftward;
        copy.Shooting = Shooting;
        copy.Mouse = Mouse;

        return copy;
    }

    public Array Serialize()
    {
        Array arr = new Array();

        // State
        arr.Add(MultiplayerID);
        arr.Add((int)Faction);
        arr.Add(Health);
        arr.Add(Position);
        arr.Add(Velocity);
        arr.Add(Rotation);

        // Visual
        arr.Add(ThrustForward);
        arr.Add(ThrustBackward);
        arr.Add(ThrustRight);
        arr.Add(ThrustLeft);

        // Input
        arr.Add(Forward);
        arr.Add(Backward);
        arr.Add(Rightward);
        arr.Add(Leftward);
        arr.Add(Shooting);
        arr.Add(Mouse);

        return arr;
    }

    public static Scout Deserialize(Array arr)
    {
        if (arr.Count != 16)
        {
            throw new System.Exception("ARRAY BAD!!!!");
        }

        var scout = new Scout();

        // State
        scout.MultiplayerID = (long)arr[0];
        scout.Faction = (Faction)(int)arr[1];
        scout.Health = (int)arr[2];
        scout.Position = (Vector2)arr[3];
        scout.Velocity = (Vector2)arr[4];
        scout.Rotation = (float)arr[5];

        // Visual
        scout.ThrustForward = (bool)arr[6];
        scout.ThrustBackward = (bool)arr[7];
        scout.ThrustRight = (bool)arr[8];
        scout.ThrustLeft = (bool)arr[9];

        // Input
        scout.Forward = (bool)arr[10];
        scout.Backward = (bool)arr[11];
        scout.Rightward = (bool)arr[12];
        scout.Leftward = (bool)arr[13];
        scout.Shooting = (bool)arr[14];
        scout.Mouse = (Vector2)arr[15];

        return scout;
    }

    public void Process(float dt, Map map)
    {
        if (CurrentState == State.Alive)
        {
            Vector2 r = Mouse - Position;

            if (r != Vector2.Zero)
            {
                Rotation = r.Angle();
            }
        }

        Vector2 thrust = Vector2.Zero;

        if (CurrentState == State.Alive)
        {
            ThrustForward = Forward;
            ThrustBackward = Backward;
            ThrustLeft = Leftward;
            ThrustRight = Rightward;
            if (ThrustForward)
            {
                thrust.X += ThrustMain;
            }
            if (ThrustBackward)
            {
                thrust.X -= ThrustForwardAxis;
            }
            if (ThrustRight)
            {
                thrust.Y += ThrustSideAxis;
            }
            if (ThrustLeft)
            {
                thrust.Y -= ThrustSideAxis;
            }
        }

        Vector2 acceleration = InertialDampners(thrust.Rotated(Rotation));

        Velocity += acceleration * dt;

        Position += Velocity * dt;
        Position = Position.Clamp(map.TopLeft, map.BottomRight);
    }

    public Vector2 InertialDampners(Vector2 inputThrust)
    {
        float thrustThreshold = 10.0f;

        bool isInputThrust = !inputThrust.IsZeroApprox();

        if (!isInputThrust && VelocityNearZero())
        {
            Velocity = Vector2.Zero;
            return inputThrust;
        }

        Vector2 targetVelocity = Vector2.Zero;
        Vector2 correctionThrust;
        Vector2 thrustOnAxis;

        if (inputThrust.IsZeroApprox())
        {
            correctionThrust = (targetVelocity - Velocity).Normalized() * CorrectionThrust;
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

        targetVelocity = inputThrust.Normalized() * MaxSpeed;
        correctionThrust = targetVelocity - Velocity;
        thrustOnAxis = correctionThrust.Rotated(-Rotation);
        Vector2 actualThrust = inputThrust;

        if (thrustOnAxis.X > thrustThreshold)
        {
            if (!ThrustForward)
            {
                ThrustForward = true;
                actualThrust.X += ThrustForwardAxis;
            }
        }
        if (thrustOnAxis.X < -thrustThreshold)
        {
            if (!ThrustBackward)
            {
                ThrustBackward = true;
                actualThrust.X -= ThrustForwardAxis;
            }
        }
        if (thrustOnAxis.Y > thrustThreshold)
        {
            if (!ThrustRight)
            {
                ThrustRight = true;
                actualThrust.Y += ThrustSideAxis;
            }
        }
        if (thrustOnAxis.Y < -thrustThreshold)
        {
            if (!ThrustLeft)
            {
                ThrustLeft = true;
                actualThrust.Y -= ThrustSideAxis;
            }
        }

        return correctionThrust;
    }
    
    public bool VelocityNearZero()
    {
        return Velocity.X > -NearZero.X && Velocity.X < NearZero.X
            && Velocity.Y > -NearZero.Y && Velocity.Y < NearZero.Y;
    }
}
