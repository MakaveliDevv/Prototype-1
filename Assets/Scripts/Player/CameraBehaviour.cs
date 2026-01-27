using UnityEngine;
using UnityEngine.InputSystem;

public class CameraBehaviour
{
    private readonly PlayerEntity playerEntity;
    private readonly float mouseSensivity_X;
    private readonly float mouseSensivity_Y;
    
    private readonly Camera camera;
    public Transform playerBody;

    private float pitch;
    private float headYaw;
    private readonly float bodyRotationSpeed;

    private readonly float maxHeadYaw = 70f;  
    private readonly float minPitch = -80f;
    private readonly float maxPitch = 80f;

    // ---- Crouch camera settings ----
    private readonly float standingCamLocalY;
    private readonly float crouchCamLocalY;
    private readonly float cameraCrouchOffset = 0.6f;      // how much lower the camera goes when crouching
    private readonly float cameraCrouchLerpSpeed = 10f;    // how fast the camera moves up/down

    public CameraBehaviour(PlayerEntity playerEntity) 
    { 
        this.playerEntity = playerEntity;
        playerBody = playerEntity.p_settings.playerBody;
        camera = playerEntity.camera;

        mouseSensivity_X = playerEntity.p_settings.mouseSensivity_X * 10f;
        mouseSensivity_Y = playerEntity.p_settings.mouseSensivity_Y * 10f;
        bodyRotationSpeed = playerEntity.p_settings.bodyRotationSpeed;

        // Store standing camera height and compute crouch height
        standingCamLocalY = camera.transform.localPosition.y;
        crouchCamLocalY = standingCamLocalY - cameraCrouchOffset;

        Start();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;  
    }

    // Update is called once per frame
    public void Update()
    {
        if (playerEntity.InputLocked) return;
        HandleMouseLook();
        HandleCrouchCamera();
    }
    
    private void HandleMouseLook()
    {
        Vector2 input = InputManager.LookInput;

        float mouseX = input.x * mouseSensivity_X * Time.deltaTime;
        float mouseY = input.y * mouseSensivity_Y * Time.deltaTime;

        // vertical
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // horizontal – head first
        headYaw += mouseX;

        // if we go beyond the head limit, rotate the body instead
        if (headYaw > maxHeadYaw)
        {
            float overflow = headYaw - maxHeadYaw;          // positive
            float angle = overflow * bodyRotationSpeed * Time.deltaTime;
            playerBody.Rotate(0f, angle, 0f);               // turn torso
            headYaw = maxHeadYaw;                           // keep head at limit
        }
        else if (headYaw < -maxHeadYaw)
        {
            float overflow = headYaw + maxHeadYaw;          // negative
            float angle = overflow * bodyRotationSpeed * Time.deltaTime;
            playerBody.Rotate(0f, angle, 0f);               // turn torso
            headYaw = -maxHeadYaw;
        }

        // apply local rotation (head relative to body)
        camera.transform.localRotation = Quaternion.Euler(pitch, headYaw, 0f);
    }

    private void HandleCrouchCamera()
    {
        // Make sure movement exists first
        if (playerEntity.movement == null)
            return;

        bool isCrouching = playerEntity.movement.IsCrouching;

        float targetY = isCrouching ? crouchCamLocalY : standingCamLocalY;

        Vector3 localPos = camera.transform.localPosition;
        localPos.y = Mathf.Lerp(
            localPos.y,
            targetY,
            Time.deltaTime * cameraCrouchLerpSpeed
        );

        camera.transform.localPosition = localPos;
    }

}

    // Old method    
    // private void HandleMouseLook()
    // {
    //     Vector2 input = InputManager.LookInput;

    //     yaw += (input.x * mouseSensivity_X) * Time.deltaTime;    
    //     pitch -= (input.y * mouseSensivity_Y) * Time.deltaTime;   

    //     // clamp vertical rotation
    //     pitch = Mathf.Clamp(pitch, -80f, 80f);

    //     // apply rotation
    //     camera.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    // }


    // Used with combination movement key input to rotate body
    // private void HandleMouseLook()
    // {
    //     Vector2 input = InputManager.LookInput;

    //     float mouseX = input.x * mouseSensivity_X * Time.deltaTime;
    //     float mouseY = input.y * mouseSensivity_Y * Time.deltaTime;

    //     pitch -= mouseY;
    //     pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

    //     headYaw += mouseX;
    //     headYaw = Mathf.Clamp(headYaw, -maxHeadYaw, maxHeadYaw);

    //     camera.transform.localRotation = Quaternion.Euler(pitch, headYaw, 0f);
    // }