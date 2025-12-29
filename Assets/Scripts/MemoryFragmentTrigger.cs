using UnityEngine;

public class MemoryFragmentTrigger : MonoBehaviour, IInteractable
{
    public MemoryFragment memoryFragment;

    public void Interact()
    {
        GameManager.Instance.CollectFragment(memoryFragment);
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
