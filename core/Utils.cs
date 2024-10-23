using Godot;

namespace Core;

public static class Utils
{
    public static bool InBounds(Vector2 v, Vector2 topLeft, Vector2 bottomRight)
    {
        return v.X >= topLeft.X && v.X <= bottomRight.X
            && v.Y >= topLeft.Y && v.Y <= bottomRight.Y;
    }

    public static bool VectorsDiffer(Vector2 v1, Vector2 v2, float margin)
    {
        return (v1.X < v2.X - margin || v1.X > v2.X + margin)
            || (v1.Y < v2.Y - margin || v1.Y > v2.Y + margin);
    }
}
