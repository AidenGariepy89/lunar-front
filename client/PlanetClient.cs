using Godot;
using Core;

namespace Client;

public partial class PlanetClient : Node2D
{
    [Export]
    public PackedScene HitParticles;

    [Export]
    public Texture2D SpriteMars;
    [Export]
    public ShaderMaterial EarthShield;
    [Export]
    public ShaderMaterial MarsShield;

    public Planet Data;

    Sprite2D _sprite;
    AnimationPlayer _animator;
    AudioStreamPlayer2D _hitAudio;
    AudioStreamPlayer2D _shieldAudio;

    public void Initialize(Planet data)
    {
        Data = data;

        _sprite = GetNode<Sprite2D>("Sprite");
        _animator = GetNode<AnimationPlayer>("Animator");

        _hitAudio = GetNode<AudioStreamPlayer2D>("HitSound");
        _shieldAudio = GetNode<AudioStreamPlayer2D>("ShieldSound");

        if (Data.Faction == Faction.Earth)
        {
            Position = Planet.EarthPosition;
            _sprite.Material = EarthShield;
        }
        else
        {
            Position = Planet.MarsPosition;
            _sprite.Texture = SpriteMars;
            _sprite.Material = MarsShield;
        }
    }

    public void Sync(Planet data)
    {
        if (data.ShieldUp != Data.ShieldUp)
        {
            (_sprite.Material as ShaderMaterial).SetShaderParameter("enabled", data.ShieldUp);
            _shieldAudio.Play();
        }

        Data = data;
        // QueueRedraw();
    }

    public void HitAnimation(float direction, Vector2 position)
    {
        _hitAudio.Play();

        if (Data.ShieldUp)
        {
            if (_animator.IsPlaying())
            {
                _animator.Stop();
            }

            _animator.Play("hit_flash");

            return;
        }

        var particles = HitParticles.Instantiate<Particles>();
        particles.Position = position;
        particles.Rotation = direction;
        GetTree().Root.GetChild(0).AddChild(particles);
    }

    // public override void _Draw()
    // {
    //     DrawString(ThemeDB.FallbackFont, new Vector2(0, -70), $"{(int)Data.ShieldHealth} / {Planet.ShieldMaxHealth}", fontSize: 36, modulate: Colors.White);
    // }
}
