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

    [Header("Other")]
    public float raycastDistance = 100f;
    public float distanceThreshold = 2f;

    public Transform playerBody;
    public Transform groundCheck;
    public LayerMask groundMask;
    public LayerMask interactableMask;
}