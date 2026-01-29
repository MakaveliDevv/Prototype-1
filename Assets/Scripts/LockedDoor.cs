using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class LockedDoor : MonoBehaviour, IInteractable
{
    public List<string> requiredFragments;
    public bool isOpen;
    public GameObject UIpanel;
    private bool UIpanelVisible; 

    public void Collect()
    {
        throw new System.NotImplementedException();
    }

    public void CollectUI(bool trigger)
    {
        throw new System.NotImplementedException();
    }

    public void Interact()
    {
        
        if(UIpanelVisible)
        {
            // Make the UI of the door able to be visible on the UI panel of the player
        }
    }

    public void InteractUI(bool trigger)
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

    public void OnInteract()
    {
        // if(GameManager.Instance.HasAllFragments(requiredFragments))
        // {
        //     // 
        // }
    }

    public void OnInteractUI(bool trigger)
    {
        throw new System.NotImplementedException();
    }

    public void PlayAudio(FragmentAudioKey key)
    {
        throw new System.NotImplementedException();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
