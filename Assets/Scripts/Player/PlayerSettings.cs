using UnityEngine;

[System.Serializable]
public class PlayerSettings 
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float gravity = 9.81f;
    public float jumpHeight = 1.5f;
    public float groundDistance = .3f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1.0f;     // how tall character is while crouched
    public float standingHeight = 2.0f;   // normal height
    public float crouchSpeed = 2.0f;      // walkSpeed while crouched
    public float crouchTransitionSpeed = 6f;

    [Header("Camera Look Settings")]
    public float mouseSensivity_X = 120f;
    public float mouseSensivity_Y = 120f;
    public float bodyRotationSpeed = 10f;
    public bool invert;

    [Header("Player Interaction Settings")]
    public float StandOffDistance = 2.25f;   // how far in front of object you stand
    public float AutoMoveSpeed = 1.5f;       // move speed for auto approach
    public float BodyTurnSpeed = 540f;       // degrees/sec
    public float CameraTurnSpeed = 10f;      // slerp speed
    public float CameraMinFocusTime = 0.25f; // minimum focus blend time


    [Header("Other")]
    public float raycastDistance = 200f;
    public float distanceThreshold = 4f;

    public Transform playerBody;
    public Transform groundCheck;
    public LayerMask groundMask;
    public LayerMask interactableMask;
}