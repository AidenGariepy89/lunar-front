using Godot;
using Godot.Collections;

namespace Core;

public class Planet
{
    // Constants
    
    public static Vector2 EarthPosition = new Vector2(-4000, 0);
    public static Vector2 MarsPosition = new Vector2(4000, 0);

    public const float ShieldMaxHealth = 50;
    public const float ShieldRegenRate = 0.5f;

    // State Variables

    public Faction Faction = Faction.Earth;
    public float ShieldHealth = ShieldMaxHealth;
    public bool ShieldUp = true;

    public Array Serialize()
    {
        Array arr = new Array();

        arr.Add((int)Faction);
        arr.Add(ShieldHealth);
        arr.Add(ShieldUp);

        return arr;
    }

    public static Planet Deserialize(Array arr)
    {
        if (arr.Count != 3)
        {
            throw new System.Exception("ARRAY BAD!!!!");
        }

        var planet = new Planet();

        planet.Faction = (Faction)(int)arr[0];
        planet.ShieldHealth = (float)arr[1];
        planet.ShieldUp = (bool)arr[2];

        return planet;
    }

    public Planet Copy()
    {
        var planet = new Planet();

        planet.Faction = Faction;
        planet.ShieldHealth = ShieldHealth;
        planet.ShieldUp = ShieldUp;

        return planet;
    }

    public int Damage(int dmg)
    {
        int pointsEarned = 0;

        if (ShieldHealth > 0)
        {
            ShieldHealth -= dmg;

            if (ShieldHealth <= 0)
            {
                pointsEarned = (int)(0 - ShieldHealth);
                ShieldHealth = 0;
                ShieldUp = false;
            }
        }
        else
        {
            pointsEarned = dmg;
        }

        return pointsEarned;
    }
}