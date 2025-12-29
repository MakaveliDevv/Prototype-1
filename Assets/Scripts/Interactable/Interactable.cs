using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact() 
    {
        // If the player pressed the 'interact' key

        // Then follow up with logic for this object
        Debug.Log("From: Interactable -> Press 'E' to interact with the object...");
    }
}
