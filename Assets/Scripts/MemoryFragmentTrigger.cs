using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryFragmentTrigger : MonoBehaviour, IInteractable
{
    public MemoryFragment memoryFragment;
    public bool isFocused;
    private bool isCollecting;
    public bool Interacting;
    public bool IsInteracting(bool interacting) => Interacting = interacting;


    [Header("UI References")]
    [SerializeField] private List<UIEntry> uiEntries = new();

    private Dictionary<string, GameObject> uiMap;

    public void Interact()
    {
        if (Interacting) return;
        InteractUI(true);
    }
    
    public void OnInteract()
    {
        if (Interacting) return;

        IsInteracting(true);
        InteractUI(false);
    }

    public void Collect()
    {
        if(!Interacting) return;

        // place the fragment in the inventory from the GameManager
        GameManager.Instance.CollectFragment(this);

    }

    public void OnCollect()
    {
        if(!Interacting) return;

        // play some audio and show some message
    }

    public void InteractUI(bool triggerPrompt)
    {
        var prompt = GetUI(memoryFragment.interactUIPrompt);
        if (prompt) prompt.SetActive(triggerPrompt);
    }

    public void OnInteractUI(bool triggerPrompt)
    {
        
    }

    public void CollectUI(bool triggerPrompt)
    {
        var prompt = GetUI(memoryFragment.interactUIPrompt);
        if (prompt) prompt.SetActive(triggerPrompt);
    }

    public void OnCollectUI(bool triggerPrompt)
    {
        var prompt = GetUI(memoryFragment.interactUIPrompt);
        if (prompt) prompt.SetActive(triggerPrompt);
    }

    public void OnInteractAudio(bool triggerPrompt)
    {
        var prompt = GetUI(memoryFragment.interactUIPrompt);
        if (prompt) prompt.SetActive(triggerPrompt);
    }

    public void OnCollectAudio(bool triggerPrompt)
    {
        var prompt = GetUI(memoryFragment.interactUIPrompt);
        if (prompt) prompt.SetActive(triggerPrompt);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (memoryFragment == null)
        {
            Debug.LogError($"{name}: memoryFragment is not assigned.", this);
            enabled = false;
            return;
        }

        if (memoryFragment.interactUI == null)
            Debug.LogError($"{name}: memoryFragment.interactUI is not assigned (TMP UI).", this);

        if (memoryFragment.collectUI == null)
            Debug.LogError($"{name}: memoryFragment.collectUI is not assigned (TMP UI).", this);

        // Only proceed if required refs exist
        if (memoryFragment.interactUI == null || memoryFragment.collectUI == null)
        {
            enabled = false;
            return;
        }

        var canvas = transform.GetChild(0);

        var interactUI = Instantiate(memoryFragment.interactUI, canvas, false);
        var collectUI = Instantiate(memoryFragment.collectUI, canvas, false);

        interactUI.gameObject.SetActive(false);
        collectUI.gameObject.SetActive(false);

        interactUI.gameObject.name = memoryFragment.interactUIPrompt;
        collectUI.gameObject.name = memoryFragment.collectUIPrompt;

        var iRT = interactUI.rectTransform;
        var colRT = collectUI.rectTransform;

        Vector3 worldPosI = transform.position + Vector3.up * 1.5f;
        Vector3 worldPosC = transform.position + Vector3.up * 1.5f;
        
        iRT.transform.position = worldPosI;
        colRT.transform.position = worldPosC;
        
        // interact UI
        uiEntries.Add(new UIEntry
        {
           key = memoryFragment.interactUIPrompt,
           value = interactUI.gameObject 
        });

        // collect UI
        uiEntries.Add(new UIEntry
        {
            key = memoryFragment.collectUIPrompt,
            value = collectUI.gameObject
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

    // Update is called once per frame
    void Update()
    {
        
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
                yield return new WaitForSeconds(1f);
                prompt.SetActive(false);
            }

        }

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