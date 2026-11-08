// Pulau Monster · P3 Animasi, Kamera & Interaksi · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine.InputSystem;

// Pasangan InputSystemMoveInput: class C# biasa, true tepat satu frame saat E ditekan.
public class InputSystemActionInput : IActionInput
{
    private readonly InputAction interact;

    public InputSystemActionInput()
    {
        interact = InputSystem.actions.FindAction("Interact");
    }

    public bool InteractPressed => interact.WasPressedThisFrame();
}
