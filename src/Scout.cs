using Godot;
using System;

public partial class Scout : CharacterBody2D
{
    public override void _Process(double delta)
    {
        GD.Print("Hello there");
    }
}
