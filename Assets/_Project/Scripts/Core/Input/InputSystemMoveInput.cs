// Pulau Monster · P2 Arsitektur & World Tilemap · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;
using UnityEngine.InputSystem;

// Implementasi IMoveInput dari keyboard dan gamepad. FindAction sekali di constructor.
public class InputSystemMoveInput : IMoveInput
{
    private readonly InputAction move;
    private readonly InputAction sprint;

    public InputSystemMoveInput()
    {
        move = InputSystem.actions.FindAction("Move");
        sprint = InputSystem.actions.FindAction("Sprint");
    }

    public Vector2 Move => move.ReadValue<Vector2>();
    public bool IsRunning => sprint != null && sprint.IsPressed();
}
