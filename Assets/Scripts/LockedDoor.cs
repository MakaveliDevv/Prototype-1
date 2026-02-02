using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class LockedDoor : MonoBehaviour, IInteractable
{
    public Fragment doorFragment;

    [Header("Unlock Requirements")]
    public List<string> requiredFragments;

    [Header("State")]
    public bool isOpen;

    [Header("UI")]
    public GameObject reqFragsUIpanel;
    
    public bool Interacting { get; private set; }
    public bool IsInteracting(bool interacting) => Interacting = interacting;
    public bool isFocused;
    public bool canOpen;

    private Dictionary<UIKey, GameObject> uiMap;
    public Dictionary<UIKey, GameObject> UIMap => uiMap;
    private Dictionary<AudioKey, AudioClip> audioMap;

    public Object LogContext => this;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.UIInitialization(doorFragment, transform, ref uiMap);
        GameManager.Instance.AudioInitialization(doorFragment, ref audioMap);

        if(reqFragsUIpanel == null) 
            Debug.Log("UI panel needs to be assigned...!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact()
    {
        InteractUI(true);
    }

    public void OnInteract()
    {
        if (isOpen) return;
        
        IsInteracting(true);
        InteractUI(false);

        canOpen = requiredFragments == null || requiredFragments.Count == 0 
        || GameManager.Instance.HasAllFragments(requiredFragments);

        if (!canOpen) return;

        OpenDoor();
    }

    public void InteractUI(bool trigger)
    {
        
    }
    
    public void OnInteractUI(bool trigger)
    {
        if(!Interacting) return;

        // Display UI panel
        if(!reqFragsUIpanel.activeInHierarchy) reqFragsUIpanel.SetActive(true);

        // Handle here the drag and drop mechanic
        // Drag from the collected fragments into the holder of requiredFragments
    }

    public void Collect()
    {
        throw new System.NotImplementedException();
    }

    public void CollectUI(bool trigger)
    {
        throw new System.NotImplementedException();
    }

    public void OnCollect()
    {
        throw new System.NotImplementedException();
    }

    public void OnCollectUI(bool trigger)
    {
        throw new System.NotImplementedException();
    }
    
    public void PlayAudio(AudioKey key)
    {
        throw new System.NotImplementedException();
    }

    private void OpenDoor()
    {
        isOpen = true;

        // Hide any UI
        InteractUI(false);
        IsInteracting(false);
        OnInteractUI(false);

        // TODO (optional):
        // - play animation (Animator trigger)
        // - disable collider / change scene state
        Debug.Log($"{name}: Door unlocked/opened.");
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!collider.CompareTag("Player"))
            return;

        var player = collider.GetComponentInParent<PlayerEntity>();
        if (player == null)
        {
            Debug.LogError("No 'PlayerEntity' component found on the player collider/parent.");
            return;
        }

        player.Interaction.Door.EnterRange(this);
    }

    private void OnTriggerExit(Collider collider)
    {
        if (!collider.CompareTag("Player"))
            return;

        var player = collider.GetComponentInParent<PlayerEntity>();
        if (player == null)
            return;

        player.Interaction.Door.ExitRange(this);
    }
}
