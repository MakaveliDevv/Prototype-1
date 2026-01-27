// using UnityEngine;

// public class PlayerInteraction
// {
//     private readonly Camera camera;
//     private LayerMask interactableMask;
//     private readonly PlayerEntity entity;

//     private RaycastHit hit;
//     private bool hitInteractable = false;
//     private readonly float raycastDistance;
//     private readonly float distanceThreshold;

//     private IInteractable interactable;
//     // private float distanceFromFragment;

//     private MemoryFragmentTrigger activeFragment;


//     public PlayerInteraction(PlayerEntity entity) 
//     {
//         camera = entity.camera;
//         interactableMask = entity.p_settings.interactableMask;
//         raycastDistance = entity.p_settings.raycastDistance;
//         distanceThreshold = entity.p_settings.distanceThreshold;
//         this.entity = entity;
//     }

//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     public void Start()
//     {
        
//     }

//     // Update is called once per frame
//     public void Update()
//     {
//         RayCasting();   
//         CheckIfAllowedToInteract();

//         // if(interactable is MemoryFragmentTrigger f)
//         // {
//         //     if(f.isFocused && entity.charController.enabled)
//         //         entity.charController.enabled = false;
            
//         //     if(!f.isFocused && !entity.charController.enabled)
//         //         entity.charController.enabled = true;
//         // }
//     }

//     private void RayCasting() 
//     {
//         hitInteractable = Physics.Raycast(
//             camera.transform.position,
//             camera.transform.forward,
//             out hit,
//             raycastDistance,
//             interactableMask
//         );

//         if (hitInteractable)
//         {
//             Debug.DrawRay(camera.transform.position,
//                         camera.transform.forward * hit.distance,
//                         Color.green);

//             if (hit.collider.TryGetComponent<IInteractable>(out var i))
//             {
//                 if (interactable != i)
//                     Debug.Log($"Now looking at new interactable: {hit.collider.gameObject.name}");
                
//                 interactable = i;
//                 // CheckIfAllowedToInteract();
//             }
//             else
//             {
//                 // Hit something on the interactable layer, but no component
//                 if (interactable != null)
//                     Debug.Log("Hit non-interactable object, clearing reference.");

//                 interactable = null;
//             }
//         }
//         else
//         {
//             if (interactable != null)
//                 Debug.Log("No interactable hit, clearing reference.");

//             interactable = null;
//         }
//     }

//     private void CheckIfAllowedToInteract()
//     {
//         if (hitInteractable && interactable is MemoryFragmentTrigger fragment)
//         {
//             float distance = CalculatedDistance();

//             if (distance <= distanceThreshold)
//             {
//                 if (activeFragment != fragment)
//                 {
//                     HideActiveUI();
//                     activeFragment = fragment;
//                 }

//                 if (!fragment.Interacting)
//                     fragment.OnInteractUI(true);

//                 if (InputManager.InteractPressed)
//                     fragment.Interact();

//                 return;
//             }
//         }

//         HideActiveUI();
//     }
    
//     private float CalculatedDistance()
//     {
//         Vector3 playerPos = entity.p_settings.playerBody.transform.position;
//         Vector3 targetPos = hit.collider.transform.position;

//         // Horizontal distance (X/Z) only
//         float distance = Vector2.Distance(
//             new Vector2(playerPos.x, playerPos.z),
//             new Vector2(targetPos.x, targetPos.z)
//         );

//         Debug.Log($"[INTERACTION DEBUG] Looking at {hit.collider.name} | " +
//                   $"distance = {distance:F2} | threshold = {distanceThreshold}");

//         return distance;
//     }


//     private void HideActiveUI()
//     {
//         if (activeFragment == null)
//             return;

//         activeFragment.IsInteracting(false);
//         activeFragment.OnInteractUI(false);
//         activeFragment = null;
//     }
    
//     public GameObject CastedObject()
//     {
//         return hitInteractable && hit.collider != null ? hit.collider.gameObject : null;
//     } 
// }


using System.Collections;
using UnityEngine;

public class PlayerInteraction
{
    private readonly Camera camera;
    private readonly PlayerEntity entity;

    private RaycastHit hit;
    private bool hitInteractable;

    private IInteractable interactable;

    // UI prompt owner (what we last showed E-interact on)
    private MemoryFragmentTrigger activeInteraction;

    // Current interaction (sequence running / focused)
    private MemoryFragmentTrigger currentInteraction;
    private Coroutine interactionRoutine;

    // Button-down detection
    private bool prevInteractPressed;
    private bool prevCancelInteractPressed;
    private bool prevCollectPressed;

    public PlayerInteraction(PlayerEntity entity)
    {
        this.entity = entity;
        camera = entity.camera;
    }

    public void Update()
    {
        RayCasting();

        if (currentInteraction != null)
        {
            if (InteractCancelDown())
                EndInteraction();

            return;
        }

        UpdatePromptAndEligibility();

        // Pressed interact while prompt is active => call OnInteract (interface) then start sequence
        if (activeInteraction != null && InteractDown() && interactionRoutine == null)
        {
            activeInteraction.OnInteract();
            
            if (activeInteraction.Interacting)
                interactionRoutine = entity.StartCoroutine(InteractionSequence(activeInteraction));    
        }           
    }

    private void RayCasting()
    {
        hitInteractable = Physics.Raycast(
            camera.transform.position,
            camera.transform.forward,
            out hit,
            entity.p_settings.raycastDistance,
            entity.p_settings.interactableMask
        );

        if (hitInteractable)
        {
            Debug.DrawRay(camera.transform.position, camera.transform.forward * hit.distance, Color.green);

            if (hit.collider.TryGetComponent<IInteractable>(out var i))
            {
                interactable = i;
            }
            else
            {
                interactable = null;
            }
        }
        else
        {
            interactable = null;
        }
    }

    private void UpdatePromptAndEligibility()
    {
        // If currently looking at a fragment, we can compute distance.
        if (hitInteractable && interactable is MemoryFragmentTrigger fragment && hit.collider != null)
        {
            float dist = CalculatedDistance(hit.collider.transform.position);

            if (dist <= entity.p_settings.distanceThreshold && !fragment.Interacting)
            {
                // Transfer prompt ownership cleanly
                if (activeInteraction != fragment)
                {
                    HidePrompt();
                    activeInteraction = fragment;
                    fragment.Interact();
                }

                return;
            }
        }

        HidePrompt();
    }

    private void HidePrompt()
    {
        if (activeInteraction == null)
            return;

        if (!activeInteraction.Interacting)
            activeInteraction.InteractUI(false);

        activeInteraction = null;
    }

    private float CalculatedDistance(Vector3 targetPos)
    {
        Vector3 playerPos = entity.p_settings.playerBody.position;

        // Horizontal (XZ) only
        return Vector2.Distance(
            new Vector2(playerPos.x, playerPos.z),
            new Vector2(targetPos.x, targetPos.z)
        );
    }

    private bool InteractDown()
    {
        bool pressed = InputManager.InteractPressed;
        bool down = pressed && !prevInteractPressed;
        prevInteractPressed = pressed;
        return down;
    }

    private bool InteractCancelDown()
    {
        bool pressed = InputManager.CancelInteractPressed;
        bool down = pressed && !prevCancelInteractPressed;
        prevCancelInteractPressed = pressed;
        return down;
    }

    private bool CollectDown()
    {
        bool pressed = InputManager.CollectPressed;
        bool down = pressed && !prevCollectPressed;
        prevCollectPressed = pressed;
        return down;
    }

    // ----------------- Interaction Sequence -----------------

    private IEnumerator InteractionSequence(MemoryFragmentTrigger fragment)
    {
        currentInteraction = fragment;

        // Lock input (requires you to implement SetInputLocked + respect it in Movement/CameraBehaviour)
        entity.SetInputLocked(true);

        // Hide prompt and mark interacting
        fragment.InteractUI(false);
        fragment.IsInteracting(true);

        // Make sure controller is enabled while we auto-move
        var controller = entity.charController;
        if (controller != null) controller.enabled = true;

        // 1) Move player to stand position in front of fragment
        Vector3 standPos = ComputeStandPosition(fragment, entity.p_settings.StandOffDistance);
        yield return MoveTo(controller, standPos, entity.p_settings.AutoMoveSpeed);

        // 2) Rotate body to face fragment center
        yield return RotateBodyTo(entity.p_settings.playerBody, FocusPoint(fragment), entity.p_settings.BodyTurnSpeed);

        // 3) Rotate camera to look at fragment center
        yield return FocusCameraTo(camera.transform, FocusPoint(fragment), entity.p_settings.CameraTurnSpeed, entity.p_settings.CameraMinFocusTime);

        // 4) Freeze movement
        if (controller != null) controller.enabled = false;

        // Mark focused (your flag)
        fragment.isFocused = true;

        // Stay in interaction mode until someone ends it (cancel later)
        while (fragment.Interacting)
        {
            if(CollectDown())
            {
                fragment.Collect();    
                
                // play some audio and show some message, use yield return


                // Cleanup
                fragment.isFocused = false;
                if (controller != null) controller.enabled = true;
                entity.SetInputLocked(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                currentInteraction = null;
                interactionRoutine = null;

                yield break;
            }
            
            yield return null;
        }

        // Cleanup
        fragment.isFocused = false;
        if (controller != null) controller.enabled = true;
        entity.SetInputLocked(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentInteraction = null;
        interactionRoutine = null;

        yield break;
    }

    private Vector3 FocusPoint(MemoryFragmentTrigger fragment)
    {
        var col = fragment.GetComponent<Collider>();
        return col != null ? col.bounds.center : fragment.transform.position;
    }

    private Vector3 ComputeStandPosition(MemoryFragmentTrigger fragment, float distance)
    {
        // "Front" is defined by fragment.transform.forward
        Vector3 focus = FocusPoint(fragment);
        Vector3 stand = focus - fragment.transform.forward * distance;

        // Keep current player height
        stand.y = entity.p_settings.playerBody.position.y;
        return stand;
    }

    private IEnumerator MoveTo(CharacterController controller, Vector3 target, float speed)
    {
        if (controller == null)
            yield break;

        while (true)
        {
            Vector3 pos = controller.transform.position;
            Vector3 delta = target - pos;
            delta.y = 0f;

            if (delta.sqrMagnitude < 0.01f)
                yield break;

            Vector3 step = delta.normalized * speed * Time.deltaTime;
            controller.Move(step);

            yield return null;
        }
    }

    private IEnumerator RotateBodyTo(Transform body, Vector3 lookTarget, float degreesPerSecond)
    {
        Vector3 dir = lookTarget - body.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            yield break;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

        while (Quaternion.Angle(body.rotation, targetRot) > 0.5f)
        {
            body.rotation = Quaternion.RotateTowards(body.rotation, targetRot, degreesPerSecond * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator FocusCameraTo(Transform cam, Vector3 lookTarget, float lerpSpeed, float minTime)
    {
        float t = 0f;

        while (t < minTime)
        {
            Quaternion targetRot = Quaternion.LookRotation((lookTarget - cam.position).normalized);
            cam.rotation = Quaternion.Slerp(cam.rotation, targetRot, Time.deltaTime * lerpSpeed);
            t += Time.deltaTime;
            yield return null;
        }

          Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void EndInteraction()
    {
        if (currentInteraction == null)
            return;

        currentInteraction.IsInteracting(false);
    }
}
