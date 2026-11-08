// Pulau Monster · P2 Arsitektur & World Tilemap · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Data kecepatan Player dipisah dari logika (60 dan 96 piksel/detik dibagi PPU 16).
[CreateAssetMenu(fileName = "Cfg_Player", menuName = "Pulau Monster/Player Config")]
public class PlayerConfig : ScriptableObject
{
    [SerializeField] private float walkSpeed = 3.75f;
    [SerializeField] private float runSpeed = 6f;

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
}
