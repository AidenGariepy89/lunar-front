using Godot;

public partial class Map : Node2D
{
    public Vector2 TopLeft { get => _topLeft.Position; }
    public Vector2 BottomRight { get => _bottomRight.Position; }
    public float EarthSpawnLeft { get => _earthSpawnLeft.Position.X; }
    public float EarthSpawnRight { get => _earthSpawnRight.Position.X; }
    public float MarsSpawnLeft { get => _marsSpawnLeft.Position.X; }
    public float MarsSpawnRight { get => _marsSpawnRight.Position.X; }

    Node2D _topLeft;
    Node2D _bottomRight;
    Node2D _earthSpawnLeft;
    Node2D _earthSpawnRight;
    Node2D _marsSpawnLeft;
    Node2D _marsSpawnRight;

    public override void _Ready()
    {
        _topLeft = GetNode<Node2D>("TopLeft");
        _bottomRight = GetNode<Node2D>("BottomRight");
        _earthSpawnLeft = GetNode<Node2D>("EarthSpawnLeft");
        _earthSpawnRight = GetNode<Node2D>("EarthSpawnRight");
        _marsSpawnLeft = GetNode<Node2D>("MarsSpawnLeft");
        _marsSpawnRight = GetNode<Node2D>("MarsSpawnRight");
    }
}
