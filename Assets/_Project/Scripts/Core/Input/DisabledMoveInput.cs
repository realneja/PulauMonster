// Pulau Monster · P2 Tugas Kelompok · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Input kosong: Player diam tanpa mengubah PlayerMovement.
public class DisabledMoveInput : IMoveInput
{
    public Vector2 Move => Vector2.zero;
    public bool IsRunning => false;
}
