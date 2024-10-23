using Godot;

public struct InputAction
{
    public enum InputType
    {
        ThrustForward,
        ThrustBackward,
        ThrustRightward,
        ThrustLeftward,
        Mouse,
    }

    public InputType Type;
    public float Time;
    public Variant Value;
}
