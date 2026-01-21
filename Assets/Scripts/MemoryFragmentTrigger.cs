using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemoryFragmentTrigger : MonoBehaviour, IInteractable
{
    public MemoryFragment memoryFragment;
    private bool isFocused;
    private bool isCollecting;

    [Header("UI References")]
    [SerializeField] private List<UIEntry> uiEntries = new();

    private Dictionary<string, GameObject> uiMap;

    public void Interact()
    {
        // Display interact UI prompt
        OnInteractUI(true);

        // If player presses the interact button, call OnInteract
        if(InputManager.InteractPressed && !memoryFragment.Interacting)
        {
            memoryFragment.IsInteracting(true);
            OnInteract();
        }
    }
    
    public void OnInteract()
    {
        // Focus camera onto the memory fragment and stop player movement
        StartCoroutine(FocusCameraOnInteraction());
        
        if(!isFocused) return;

        if(InputManager.InteractPressed && memoryFragment.Interacting && !isCollecting)
        {
            StartCoroutine(Collect());
        }
    }

    private IEnumerator FocusCameraOnInteraction()
    {
        isFocused = true;
        yield break;
    }

    private IEnumerator UnFocusCameraOnCollection()
    {
        isFocused = false;
        yield break;
    }

    private void Awake()
    {
        uiEntries.Add(new UIEntry
        {
            key = memoryFragment.collectUIPrompt,
            value = memoryFragment.collectUI
        });

        uiMap = new Dictionary<string, GameObject>(uiEntries.Count);

        foreach (var e in uiEntries)
        {
            if (string.IsNullOrWhiteSpace(e.key))
            {
                Debug.LogWarning($"{name}: UI entry has empty key.", this);
                continue;
            }

            if (uiMap.ContainsKey(e.key))
            {
                Debug.LogWarning($"{name}: Duplicate key '{e.key}' found. Keeping first.", this);
                continue;
            }

            uiMap.Add(e.key, e.value);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Interaction
    private void OnInteractUI(bool showPrompt)
    {
        var prompt = GetUI(memoryFragment.interactUIPrompt);
        if (prompt) prompt.SetActive(showPrompt);
    }

    // Collect
    private IEnumerator Collect()
    {
        isCollecting = true;

        OnInteractUI(false); // Disable interact prompt

        GameManager.Instance.CollectFragment(memoryFragment);
        yield return StartCoroutine(OnCollect());

        // Unfocus camera and enable player 
        yield return StartCoroutine(UnFocusCameraOnCollection());

        yield return new WaitForSeconds(3f);

        memoryFragment.IsInteracting(false);
        isCollecting = false;
        Destroy(this);
    }

    private IEnumerator OnCollectUI(bool showPrompt, float targetAlpha)
    {
        var prompt = GetUI(memoryFragment.collectUIPrompt);
        if (prompt)
        {
            if(!prompt.activeInHierarchy)
            {
                prompt.SetActive(true);
                GameManager.Instance.graphicFader.Reset(prompt);
                
                yield return new WaitForSeconds(.25f);
                GameManager.Instance.graphicFader.FadeTo(prompt, targetAlpha);
            }
            else
            {
                GameManager.Instance.graphicFader.FadeTo(prompt, targetAlpha);
                prompt.SetActive(false);
            }

        }

        yield break;
    }

    private void OnInteractAudio()
    {
        
    }

    private void OnCollectAudio()
    {
    
    }

    private IEnumerator OnCollect()
    {
        StartCoroutine(OnCollectUI(true, 1f));
        OnCollectAudio();

        yield return new WaitForSeconds(2f);
        
        StartCoroutine(OnCollectUI(false, 0f));
        yield break;
    }
    
    private GameObject GetUI(string key)
    {
        if (uiMap != null && uiMap.TryGetValue(key, out var go) && go != null)
            return go;

        Debug.LogWarning($"{name}: UI key '{key}' not found or is null.", this);
        return null;
    }
}