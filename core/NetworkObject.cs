using Godot;
using Godot.Collections;

namespace Core;

public interface NetworkObject
{
    void DeliverInput(Array input);
    void ReceiveSync(long seqNum, Array<Array> syncData);
    void SpawnNewScout(Array scoutPacket);
    void SpawnScouts(Array<Array> scouts);
    public void HitScout(Array scoutPacket, long BulletId);
}
