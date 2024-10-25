using Godot;
using Godot.Collections;
using Core;

namespace Server;

public partial class PlanetServer : Area2D
{
    public Planet Data;

    Timer _regenTimer;
    Planet _oldData = null;

    Server _server;

    public void Initialize(Faction faction, Server server)
    {
        _server = server;

        _regenTimer = GetNode<Timer>("Regen");
        _regenTimer.Timeout += Regen;
        _regenTimer.Start();

        Data = new Planet();
        Data.Faction = faction;

        if (faction == Faction.Earth)
        {
            Position = Planet.EarthPosition;
        }
        else
        {
            Position = Planet.MarsPosition;
        }

        AreaEntered += CollisionDetected;
    }

    public bool NeedsSync()
    {
        if (_oldData == null)
        {
            _oldData = Data.Copy();
            return true;
        }

        bool needsSync = _oldData.ShieldHealth != Data.ShieldHealth
            || _oldData.ShieldUp != Data.ShieldUp;

        if (needsSync)
        {
            _oldData = Data.Copy();
        }

        return needsSync;
    }

    void CollisionDetected(Area2D other)
    {
        if (other is ScoutBullet)
        {
            BulletHit(other as ScoutBullet);
            return;
        }

        if (other is ScoutServer)
        {
            ScoutHit(other as ScoutServer);
            return;
        }
    }

    void BulletHit(ScoutBullet bullet)
    {
        if (bullet.Faction != Data.Faction)
        {
            int score = Data.Damage(ScoutBullet.Damage);

            if (Data.Faction == Faction.Earth)
            {
                _server.Mars.Data.Score += score;
            }
            else
            {
                _server.Earth.Data.Score += score;
            }
        }

        _server.MainRef.Rpc(
            Main.MethodName.PlanetBulletHit,
            _server.Earth.Data.Score,
            _server.Mars.Data.Score,
            bullet.BulletId,
            Data.Serialize()
        );

        bullet.QueueFree();
    }

    void ScoutHit(ScoutServer scout)
    {
        if (!Data.ShieldUp)
        {
            int score = Data.Damage(Scout.KamikazeDamage);

            if (Data.Faction == Faction.Earth)
            {
                _server.Mars.Data.Score += score;
            }
            else
            {
                _server.Earth.Data.Score += score;
            }
        }

        scout.Data.Health = 0;
        scout.Data.CurrentState = Scout.State.Dead;
        scout.Die();

        _server.MainRef.Rpc(
            Main.MethodName.PlanetScoutHit,
            _server.Earth.Data.Score,
            _server.Mars.Data.Score,
            scout.Data.Serialize(),
            Data.Serialize()
        );
    }

    void Regen()
    {
        Data.ShieldHealth += Planet.ShieldRegenRate;

        if (Data.ShieldHealth >= Planet.ShieldMaxHealth)
        {
            Data.ShieldHealth = Planet.ShieldMaxHealth;
            Data.ShieldUp = true;
            return;
        }
    }
}
