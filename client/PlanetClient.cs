using Godot;
using Core;

namespace Client;

public partial class PlanetClient : Node2D
{
    [Export]
    public Texture2D SpriteMars;
    [Export]
    public ShaderMaterial EarthShield;
    [Export]
    public ShaderMaterial MarsShield;

    public Planet Data;

    Sprite2D _sprite;

    public void Initialize(Planet data)
    {
        Data = data;

        _sprite = GetNode<Sprite2D>("Sprite");

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
        }

        Data = data;
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawString(ThemeDB.FallbackFont, new Vector2(0, -70), $"{Data.ShieldHealth} / {Planet.ShieldMaxHealth}", fontSize: 36, modulate: Colors.White);
    }
}
