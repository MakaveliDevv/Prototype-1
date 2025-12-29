using UnityEngine;

public class PlayerInteraction
{
    private Camera camera;
    private LayerMask interactableMask;
    private PlayerEntity entity;

    private RaycastHit hit;
    private bool hitInteractable = false;
    private float raycastDistance;
    private float distanceThreshold;

    private IInteractable interactable;

    public PlayerInteraction(PlayerEntity entity) 
    {
        this.camera = entity.camera;
        this.interactableMask = entity.p_settings.interactableMask;
        this.raycastDistance = entity.p_settings.raycastDistance;
        this.distanceThreshold = entity.p_settings.distanceThreshold;
        this.entity = entity;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
        RayCasting();   
    }

    // private void RayCasting() 
    // {
    //     hitInteractable = Physics.Raycast(camera.transform.position, camera.transform.TransformDirection(Vector3.forward), out hit, raycastDistance, interactableMask);

    //     if(hitInteractable) 
    //     {
    //         Debug.DrawRay(camera.transform.position, camera.transform.TransformDirection(Vector3.forward) * hit.distance, Color.green);

    //         if(interactable == null) 
    //         {
    //             // Fetch Interactable component
    //             if(hit.collider.gameObject.TryGetComponent<IInteractable>(out var i)) 
    //             {
    //                 interactable = i;

    //                 // Debug.Log(interactable.gameObject.name);
    //             } 
    //             else 
    //             {
    //                 Debug.Log("No Interactable component found...");
    //             }
    //         }
    //     }
    //     else 
    //     {
    //         interactable = null;
    //     }
    // }

    // private void RayCasting() 
    // {
    //     hitInteractable = Physics.Raycast(
    //         camera.transform.position,
    //         camera.transform.forward,
    //         out hit,
    //         raycastDistance,
    //         interactableMask
    //     );

    //     if (hitInteractable)
    //     {
    //         Debug.DrawRay(camera.transform.position, camera.transform.forward * hit.distance, Color.green);

    //         if (!hit.collider.gameObject.TryGetComponent<IInteractable>(out interactable))
    //         {
    //             interactable = null;
    //         }

    //         CheckIfAllowedToInteract(); 
    //     }
    //     else
    //     {
    //         interactable = null;
    //     }
    // }

        private void RayCasting() 
        {
            hitInteractable = Physics.Raycast(
                camera.transform.position,
                camera.transform.forward,
                out hit,
                raycastDistance,
                interactableMask
            );

            if (hitInteractable)
            {
                Debug.DrawRay(camera.transform.position,
                            camera.transform.forward * hit.distance,
                            Color.green);

                if (hit.collider.TryGetComponent<IInteractable>(out var i))
                {
                    if (interactable != i)
                    {
                        Debug.Log($"Now looking at new interactable: {hit.collider.gameObject.name}");
                    }
                    
                    interactable = i;
                    CheckIfAllowedToInteract();
                }
                else
                {
                    // Hit something on the interactable layer, but no component
                    if (interactable != null)
                        Debug.Log("Hit non-interactable object, clearing reference.");

                    interactable = null;
                }
            }
            else
            {
                if (interactable != null)
                    Debug.Log("No interactable hit, clearing reference.");

                interactable = null;
            }
        }


    
    private void CheckIfAllowedToInteract() 
    {
        if (!hitInteractable || interactable == null)
            return;

        Vector3 playerPos = entity.p_settings.playerBody.transform.position;
        Vector3 targetPos = hit.collider.transform.position;

        // Horizontal distance (X/Z) only
        float horizontalDistance = Vector2.Distance(
            new Vector2(playerPos.x, playerPos.z),
            new Vector2(targetPos.x, targetPos.z)
        );

        Debug.Log($"[INTERACTION DEBUG] Looking at {hit.collider.name} | " +
                  $"distance = {horizontalDistance:F2} | threshold = {distanceThreshold}");

        if (horizontalDistance <= distanceThreshold)
        {
            Debug.Log("[INTERACTION DEBUG] Distance OK, calling Interact()");
            interactable.Interact();
        }
    } 

    // private void CheckIfAllowedToInteract() 
    // {
    //     if (!hitInteractable || interactable == null)
    //         return;

    //     Vector3 playerPos = entity.p_settings.playerBody.transform.position;
    //     Vector3 targetPos = hit.collider.gameObject.transform.position;

    //     float horizontalDistance = Vector2.Distance(
    //         new Vector2(playerPos.x, playerPos.z),
    //         new Vector2(targetPos.x, targetPos.z)
    //     );

    //     if (horizontalDistance <= distanceThreshold)
    //     {
    //         interactable.Interact();
    //     }
    // }

    // private void CheckIfAllowedToInteract() 
    // {
    //     if(/*hitInteractable &&*/
    //     Vector2.Distance(entity.p_settings.playerBody.transform.position, hit.collider.gameObject.transform.position) <= distanceThreshold) 
    //     {
    //         interactable.Interact();
    //     } 
    // } 

    public GameObject CastedObject() => hit.collider.gameObject;
   
    // private GameObject RayCasting() 
    // {
    //     hitInteractable = Physics.Raycast(camera.transform.position, camera.transform.TransformDirection(Vector3.forward), out hit, raycastDistance, interactableMask);

    //     if(hitInteractable) 
    //     {
    //         Debug.DrawRay(camera.transform.position, camera.transform.TransformDirection(Vector3.forward) * hit.distance, Color.green);
    //         return hit.collider.gameObject;
    //     }
    //     else 
    //     {
    //         Debug.DrawRay(camera.transform.position, camera.transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
    //         return null;
    //     }
    // }
}