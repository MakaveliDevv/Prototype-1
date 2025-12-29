using UnityEngine;
using UnityEngine.InputSystem;

public static class InputManager
{
    private static InputAction move;
    private static InputAction look;
    private static InputAction crouch;

    public static Vector2 MoveInput { get; private set; }
    public static Vector2 LookInput { get; private set; }

    public static bool CrouchPressed { get; private set; }

    public static void Initialize(InputActionAsset asset) 
    {
        var map = asset.FindActionMap("Player");
        move = map.FindAction("Move");
        look = map.FindAction("Look");
        crouch = map.FindAction("Crouch");

        Enable();

        crouch.performed += ctx => CrouchPressed = true;
        crouch.canceled += ctx => CrouchPressed = false;        
    }

    private static void Enable() 
    {
       move?.Enable();
       look?.Enable();
       crouch?.Enable();
    }

    private static void Disable() 
    {
        move?.Disable();
        look?.Disable();
        crouch?.Disable();
    }

    public static void UpdateStickAndMoueInput() 
    {
        MoveInput = move.ReadValue<Vector2>();
        LookInput = look.ReadValue<Vector2>();
    }
}
