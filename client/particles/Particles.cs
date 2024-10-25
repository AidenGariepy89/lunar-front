using Godot;

public partial class Particles : GpuParticles2D
{
    public override void _Ready()
    {
        OneShot = true;
        Emitting = true;

        double seconds = Lifetime * 1.2;

        GetTree().CreateTimer(seconds).Timeout += () => QueueFree();
    }
}
