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
        }

        if (other is ScoutServer)
        {
            ScoutHit(other as ScoutServer);
        }
    }

    void BulletHit(ScoutBullet bullet)
    {
        int score = Data.Damage(ScoutBullet.Damage);

        if (Data.Faction == Faction.Earth)
        {
            _server.MainRef.ScoreMars += score;
        }
        else
        {
            _server.MainRef.ScoreEarth += score;
        }

        _server.MainRef.Rpc(
            Main.MethodName.PlanetBulletHit,
            _server.MainRef.ScoreEarth,
            _server.MainRef.ScoreMars,
            bullet.BulletId,
            Data.Serialize()
        );

        bullet.QueueFree();
    }

    void ScoutHit(ScoutServer scout)
    {
    }
}
