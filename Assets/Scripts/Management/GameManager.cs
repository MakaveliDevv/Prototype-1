using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton

    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameManager>();
                if (instance == null)
                {
                    GameObject singleton = new ("GameManager");
                    instance = singleton.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }

    #endregion
    public int currentFloor;
    private List<MemoryFragment> collectedFragments = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void CollectFragment(MemoryFragment fragment)
    {
        if (!collectedFragments.Contains(fragment))
        {
            collectedFragments.Add(fragment);
            OnMemoryFragmentCollected(fragment.id);
        }
    }

    private void OnMemoryFragmentCollected(int fragmentId) { }

    private bool CanUnlockRooftop() { return false; }
}