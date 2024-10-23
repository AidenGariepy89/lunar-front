using Godot;

public partial class ScoutShield : Sprite2D
{
    [Signal]
    public delegate void ShieldBrokenEventHandler();
    [Signal]
    public delegate void ShieldRegeneratedEventHandler();

    [Export]
    public Texture2D SpriteMars;

    [Export]
    public int MaxHealth = 100;

    public int Health
    {
        get => _health;
        set => Damage(value);
    }

    int _health;
    ScoutOld _scout;

    public void Initialize(ScoutOld scout)
    {
        _health = MaxHealth;

        _scout = scout;

        if (_scout.Faction == Core.Faction.Mars)
        {
            Texture = SpriteMars;
        }
    }

    /// Returns false if the shield is broken or damaged.
    public bool Damage(int damage)
    {
        if (_health == 0)
        {
            return false;
        }

        _health -= damage;

        if (_health <= 0)
        {
            _health = 0;
            EmitSignal(SignalName.ShieldBroken);
            return false;
        }

        GD.Print("current shield health: " + _health);

        _scout.Animator.Play("flash");

        return true;
    }
}
