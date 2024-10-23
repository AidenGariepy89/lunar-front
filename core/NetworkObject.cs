using Godot;
using Godot.Collections;

namespace Core;

public interface NetworkObject
{
    void SpawnNewScout(Array scoutPacket);
    void SpawnScouts(Array<Array> scouts);
}
