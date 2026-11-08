// Pulau Monster · P4 Data Monster & Monster Liar · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Pola decorator: membungkus input asli dan mengembalikan nol selama terkunci (battle, dialog, menu jeda).
public class InputGate : IMoveInput, IActionInput
{
    private readonly IMoveInput move;
    private readonly IActionInput action;
    private int locks;

    public InputGate(IMoveInput moveInput, IActionInput actionInput)
    {
        move = moveInput;
        action = actionInput;
    }

    public bool IsLocked => locks > 0;

    public void Lock() => locks++;
    public void Unlock() => locks = Mathf.Max(0, locks - 1);

    public Vector2 Move => IsLocked ? Vector2.zero : move.Move;
    public bool IsRunning => !IsLocked && move.IsRunning;
    public bool InteractPressed => !IsLocked && action.InteractPressed;
}
