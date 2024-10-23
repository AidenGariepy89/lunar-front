using Godot;

public struct InputAction
{
    public enum InputType
    {
        ThrustForward,
        ThrustBackward,
        ThrustRightward,
        ThrustLeftward,
        Shooting,
        Mouse,
    }

    public InputType Type;
    public float Time;
    public Variant Value;
}
