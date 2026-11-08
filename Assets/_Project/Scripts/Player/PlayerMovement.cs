// Pulau Monster · P1 Setup Proyek & Gerak Karakter (versi 5, P3) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Update membaca input, FixedUpdate menggerakkan Rigidbody2D. Hanya mengenal IMoveInput (DI tahap 2).
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    [SerializeField] private Rigidbody2D rb;

    private IMoveInput input;
    private Vector2 velocity;

    public Vector2 Velocity => velocity;
    public Vector2 Facing { get; private set; } = Vector2.down;
    public bool IsRunning { get; private set; }

    public void Construct(IMoveInput moveInput) => input = moveInput;

    void Update()
    {
        if (input == null || config == null) return;
        Vector2 move = input.Move;
        if (move.sqrMagnitude > 1f) move.Normalize();
        IsRunning = input.IsRunning && move.sqrMagnitude > 0.01f;
        float speed = input.IsRunning ? config.RunSpeed : config.WalkSpeed;
        velocity = move * speed;
        if (move.sqrMagnitude > 0.01f) Facing = ToCardinal(move);
    }

    void FixedUpdate()
    {
        if (rb != null) rb.linearVelocity = velocity;
    }

    // Pindah seketika (muat save, kalah battle) sambil mengatur arah hadap.
    public void Teleport(Vector2 position, Vector2 facing)
    {
        velocity = Vector2.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = position;
        }
        transform.position = position;
        if (facing.sqrMagnitude > 0.01f) Facing = ToCardinal(facing);
    }

    private static Vector2 ToCardinal(Vector2 v) =>
        Mathf.Abs(v.x) > Mathf.Abs(v.y)
            ? new Vector2(Mathf.Sign(v.x), 0f)
            : new Vector2(0f, Mathf.Sign(v.y));
}
