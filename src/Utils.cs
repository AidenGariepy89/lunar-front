using Godot;

public static class Utils
{
    public static bool InBounds(Vector2 v, Vector2 topLeft, Vector2 bottomRight)
    {
        return v.X >= topLeft.X && v.X <= bottomRight.X
            && v.Y >= topLeft.Y && v.Y <= bottomRight.Y;
    }
}
