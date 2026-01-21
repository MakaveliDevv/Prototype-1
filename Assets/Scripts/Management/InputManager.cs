using UnityEngine;
using UnityEngine.InputSystem;

public static class InputManager
{
    private static InputAction move, look, crouch, interact;

    public static Vector2 MoveInput { get; private set; }
    public static Vector2 LookInput { get; private set; }

    public static bool CrouchPressed { get; private set; }
    public static bool InteractPressed { get; private set; }

    public static void Initialize(InputActionAsset asset) 
    {
        var map = asset.FindActionMap("Player");
        move = map.FindAction("Move");
        look = map.FindAction("Look");
        crouch = map.FindAction("Crouch");
        interact = map.FindAction("Interact");

        Enable();

        crouch.performed += ctx => CrouchPressed = true;
        crouch.canceled += ctx => CrouchPressed = false;

        interact.performed += ctx => InteractPressed = true;
        interact.canceled += ctx => InteractPressed = false;        
    }

    private static void Enable() 
    {
       move?.Enable();
       look?.Enable();
       crouch?.Enable();
       interact?.Enable();
    }

    private static void Disable() 
    {
        move?.Disable();
        look?.Disable();
        crouch?.Disable();
        interact?.Disable();
    }

    public static void UpdateStickAndMoueInput() 
    {
        MoveInput = move.ReadValue<Vector2>();
        LookInput = look.ReadValue<Vector2>();
    }
}
