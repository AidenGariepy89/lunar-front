using Godot;
using System;

public partial class GravityWell : Node2D
{
    [Export]
    public float AttractionRadius = 200;
    [Export]
    public float SurfaceRadius = 150;
    [Export]
    public float Mass = 10.0f;

    CollisionShape2D _attractionWell;
    CollisionShape2D _surfaceWell;

    public override void _Ready()
    {
        _attractionWell = GetNode<CollisionShape2D>("AttractionArea/CollisionShape2D");
        if (_attractionWell.Shape is not CircleShape2D) {
            throw new Exception();
        }
        ((CircleShape2D)_attractionWell.Shape).Radius = AttractionRadius;

        _surfaceWell = GetNode<CollisionShape2D>("SurfaceArea/CollisionShape2D");
        if (_surfaceWell.Shape is not CircleShape2D) {
            throw new Exception();
        }
        ((CircleShape2D)_surfaceWell.Shape).Radius = SurfaceRadius;
    }

    public override void _Draw()
    {
        Color inner = Colors.Gray;
        Color outer = Colors.Yellow;

        inner.A = 0.5f;
        outer.A = 0.2f;

        DrawCircle(Vector2.Zero, SurfaceRadius, inner);
        DrawCircle(Vector2.Zero, AttractionRadius, outer);
    }

    public float Attraction()
    {
        return (4.0f / 3.0f) * Mathf.Pi * SurfaceRadius * SurfaceRadius * SurfaceRadius * Mass / 3;
    }
}
