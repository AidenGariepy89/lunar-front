using Godot;
using Godot.Collections;

namespace Core;

public interface NetworkObject
{
    void DeliverInput(Array input);
    void ReceiveSync(long seqNum, Array<Array> scouts, Array earth, Array mars);
    void SpawnNewScout(Array scoutPacket);
    void HitScout(Array scoutPacket, long BulletId);
    void JoinGame(Array<Array> scouts, Array earth, Array mars);
}
