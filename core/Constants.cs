using Godot;

namespace Core;

public static class Constants
{
    public const string Address = "127.0.0.1";
    public const int Port = 6969;
    public const int MaxPlayers = 40;

    public const long ServerId = 1;
    public const float RespawnDelay = 3.0f;
    //public const int ScoutHealth = 3;

    public const float MapSector = 128 * 4;
    public const float MapWidth = 20 * MapSector; // Asuuming each MapSector is actually one of the green squares in-game
    public const float MapHeight = 17 * MapSector;
    public const float SpawnZoneWidth = 2 * MapSector;
    public const float SpawnZoneHeight = 2 * MapSector;
    public static readonly float[] EarthSpawn = { -8 * MapSector,  0 * MapSector}; // Where the spawn zone starts
    public static readonly float[] MarsSpawn = { 8 * MapSector, 0 * MapSector };
}
