// Pulau Monster · P2 Arsitektur & World Tilemap · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Kontrak input gerak: PlayerMovement hanya mengenal interface ini (DI tahap 2).
public interface IMoveInput
{
    Vector2 Move { get; }
    bool IsRunning { get; }
}
