using UnityEngine;

public class PlayerInteraction
{
    private readonly Camera camera;
    private LayerMask interactableMask;
    private readonly PlayerEntity entity;

    private RaycastHit hit;
    private bool hitInteractable = false;
    private readonly float raycastDistance;
    private readonly float distanceThreshold;

    private IInteractable interactable;
    // private float distanceFromFragment;

    public PlayerInteraction(PlayerEntity entity) 
    {
        camera = entity.camera;
        interactableMask = entity.p_settings.interactableMask;
        raycastDistance = entity.p_settings.raycastDistance;
        distanceThreshold = entity.p_settings.distanceThreshold;
        this.entity = entity;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
        RayCasting();   
        CheckIfAllowedToInteract();
        // if(CastedObject() != null && interactable is MemoryFragmentTrigger fragment)
        // {
        //     float calcDistance = CalculatedDistance();
        //     if(calcDistance >= distanceThreshold)
        //     {
        //         fragment.OnInteractUI(false);
        //     }
        // }

        // if(interactable is MemoryFragmentTrigger f)
        // {
        //     if(f.isFocused && entity.charController.enabled)
        //         entity.charController.enabled = false;
            
        //     if(!f.isFocused && !entity.charController.enabled)
        //         entity.charController.enabled = true;
        // }
    }

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
                    Debug.Log($"Now looking at new interactable: {hit.collider.gameObject.name}");
                
                interactable = i;
                // CheckIfAllowedToInteract();
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

        float distanceFromFragment = CalculatedDistance();

        if(interactable is MemoryFragmentTrigger fragment)
        {
            if(distanceFromFragment <= distanceThreshold)
            {
                if(!fragment.Interacting) 
                    fragment.OnInteractUI(true);
                
                if(InputManager.InteractPressed)
                    interactable.Interact();
            }
            else
            {
                fragment.IsInteracting(false);
                fragment.OnInteractUI(false);   
            }   
        }
    }
    
    private float CalculatedDistance()
    {
        Vector3 playerPos = entity.p_settings.playerBody.transform.position;
        Vector3 targetPos = hit.collider.transform.position;

        // Horizontal distance (X/Z) only
        float distance = Vector2.Distance(
            new Vector2(playerPos.x, playerPos.z),
            new Vector2(targetPos.x, targetPos.z)
        );

        Debug.Log($"[INTERACTION DEBUG] Looking at {hit.collider.name} | " +
                  $"distance = {distance:F2} | threshold = {distanceThreshold}");

        return distance;
    }

    public GameObject CastedObject()
    {
        return hitInteractable && hit.collider != null ? hit.collider.gameObject : null;
    } 
}