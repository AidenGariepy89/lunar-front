using Godot.Collections;

namespace Core;

public interface NetworkObject
{
    void DeliverInput(Array input);
    void SpawnNewScout(Array scoutPacket);
    void SpawnScouts(Array<Array> scouts);
    void ReceiveSync(Array<Array> syncData);
}
