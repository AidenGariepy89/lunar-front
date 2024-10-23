using Godot;

public partial class Map : Node2D
{
    public Vector2 TopLeft { get => _topLeft.Position; }
    public Vector2 BottomRight { get => _bottomRight.Position; }

    Node2D _topLeft;
    Node2D _bottomRight;

    public override void _Ready()
    {
        _topLeft = GetNode<Node2D>("TopLeft");
        _bottomRight = GetNode<Node2D>("BottomRight");
    }
}
