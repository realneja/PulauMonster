// Pulau Monster · P3 Animasi, Kamera & Interaksi · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Satu script satu tugas: hanya membaca PlayerMovement lalu mengisi parameter Animator.
// Tantangan tugas: parameter Speed membuat animasi jalan 1,5 kali lebih cepat saat lari.
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int Speed = Animator.StringToHash("Speed");

    [SerializeField] private PlayerMovement movement;
    [SerializeField] private float runAnimationMultiplier = 1.5f;

    private Animator animator;

    void Awake() => animator = GetComponent<Animator>();

    void Update()
    {
        animator.SetBool(IsMoving, movement.Velocity.sqrMagnitude > 0.01f);
        animator.SetFloat(MoveX, movement.Facing.x);
        animator.SetFloat(MoveY, movement.Facing.y);
        animator.SetFloat(Speed, movement.IsRunning ? runAnimationMultiplier : 1f);
    }
}
