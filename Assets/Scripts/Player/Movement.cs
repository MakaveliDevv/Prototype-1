using UnityEngine;
using UnityEngine.InputSystem;

public class Movement
{
    private PlayerEntity playerEntity;
    private CharacterController controller;

    // movement settings
    private float walkSpeed;
    private float gravity;
    private float jumpHeight;
    // public float bodyTurnSpeed = 120f;

    // ground check
    private float groundDistance;

    private Vector3 velocity;
    private bool isGrounded;

    // Crouch Settings 
    private bool isCrouching = false;
    private float crouchHeight;     // how tall character is while crouched
    private float standingHeight;   // normal height
    private float crouchSpeed;      // walkSpeed while crouched
    private float crouchTransitionSpeed;

    // Remember original controller center so we don't mess it up
    private float originalCenterY;

    public bool IsCrouching => isCrouching;

    public Movement(PlayerEntity playerEntity) 
    {
        // Components
        this.playerEntity = playerEntity;
        this.controller = playerEntity.controller;

        // Movement Settings
        this.walkSpeed = playerEntity.p_settings.walkSpeed;
        this.gravity = playerEntity.p_settings.gravity;
        
        // Crouch Settings
        this.crouchHeight = playerEntity.p_settings.crouchHeight;
        this.standingHeight = playerEntity.p_settings.standingHeight;
        this.crouchSpeed = playerEntity.p_settings.crouchSpeed;
        this.crouchTransitionSpeed = playerEntity.p_settings.crouchTransitionSpeed;
        
        originalCenterY = controller.center.y;
        controller.height = standingHeight;

        // Jump Settings
        this.jumpHeight = playerEntity.p_settings.jumpHeight;
        this.groundDistance = playerEntity.p_settings.groundDistance;

        // Start();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    public void Update()
    {
        HandleGroundCheck();
        ApplyGravity();
        HandleMovement();
        HandleCrouch();
    }

    private void HandleGroundCheck() 
    {
        isGrounded = Physics.CheckSphere(playerEntity.p_settings.groundCheck.position, groundDistance, playerEntity.p_settings.groundMask);

        if(isGrounded && velocity.y < 0)
            velocity.y = -2f; 
    }
    
    private void ApplyGravity() 
    {
        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMovement() 
    {
        Vector2 input = InputManager.MoveInput;
        float x = input.x;
        float z = input.y;

        Vector3 move = controller.transform.right * x + controller.transform.forward * z;
        // controller.Move(move * walkSpeed * Time.deltaTime);
        float currentSpeed = isCrouching ? crouchSpeed : walkSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        bool wantsToCrouch = InputManager.CrouchPressed;

        if (wantsToCrouch)
        {
            // Only trigger once when we start holding the key
            if (!isCrouching)
            {
                StartCrouch();
            }
        }
        else
        {
            // Key is released – try to stand up if we are crouching
            if (isCrouching)
            {
                TryStopCrouch();
            }
        }

        // Smooth height transition
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        
        // Smoothly move towards target height
        float newHeight = Mathf.Lerp(
            controller.height,
            targetHeight,
            Time.deltaTime * crouchTransitionSpeed
        );


        // Snap when very close to avoid tiny "breathing" jitter
        if (Mathf.Abs(newHeight - targetHeight) < 0.01f)
            newHeight = targetHeight;

        controller.height = newHeight;
        // controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        // Keep center fixed at the original value so the controller
        // doesn't slide up/down through the ground
        Vector3 c = controller.center;
        c.y = originalCenterY;
        controller.center = c;

        // Keep controller centered so we don't float/sink
        controller.center = new Vector3(0f, 0f, 0f);
    }


    private void StartCrouch()
    {
        isCrouching = true;
    }

    private void TryStopCrouch()
    {
        // How much taller we want to be when standing up
        float growAmount = standingHeight - controller.height;

        // If for some reason we're already at or above standing height, just stand
        if (growAmount <= 0f)
        {
            isCrouching = false;
            Debug.Log("Already at standing height, uncrouching.");
            return;
        }

        // We cast from just above the feet, using the CharacterController radius
        // to check if there is space above the player to grow to standing height.
        // Vector3 origin = controller.transform.position + Vector3.up * controller.radius;
        Vector3 origin = controller.transform.position + Vector3.up * originalCenterY;
        float checkDistance = growAmount + 0.05f; // small safety margin

        bool blocked = Physics.SphereCast(
            origin,
            controller.radius * 0.95f,        // slightly smaller than radius so we don't clip walls
            Vector3.up,
            out _,
            checkDistance,
            ~0,                               // check against everything
            QueryTriggerInteraction.Ignore
        );

        if (blocked)
        {
            Debug.Log("Blocked from standing – obstacle above the player.");
            return;
        }

        // Safe to stand
        isCrouching = false;
        Debug.Log("Stopped Crouching...");
    }
}





    // OLD CODE: USED FOR BODY ROTATION WHILE PRESSING MOVEMENT KEYS
    // private void HandleMovement() 
    // {
    //     Vector2 input = InputManager.MoveInput;
    //     float x = input.x;   // A/D
    //     float z = input.y;   // W/S

    //     // bool leftDown  = Mouse.current?.leftButton.isPressed  ?? false;
    //     // bool rightDown = Mouse.current?.rightButton.isPressed ?? false;
    //     // bool bodyTurnMode = leftDown && rightDown;
        
        

    //     if (bodyTurnMode)
    //     {
    //         // TURN BODY INSTEAD OF MOVE
    //         // Use A/D (x) to rotate around Y-axis
    //         float turnInput = x; // -1 = left (A), +1 = right (D)

    //         if (Mathf.Abs(turnInput) > 0.01f)
    //         {
    //             float angle = turnInput * bodyTurnSpeed * Time.deltaTime;
    //             body.Rotate(0f, angle, 0f);
    //         }

    //         // Do NOT move the controller horizontally in this mode
    //         return;
    //     }

    //     // NORMAL MOVEMENT MODE (no body-turning)
    //     Vector3 move = controller.transform.right * x + controller.transform.forward * z;
    //     controller.Move(move * walkSpeed * Time.deltaTime);
    // }

