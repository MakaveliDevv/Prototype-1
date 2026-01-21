using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public GraphicFader graphicFader;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        graphicFader = new(this);
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

            // Set interating with fragment to false
            fragment.IsInteracting(false);
        }
    }

    private void OnMemoryFragmentCollected(int fragmentId) { }

    private bool CanUnlockRooftop() { return false; }
}

[System.Serializable]
public class UIEntry
{
    public string key;
    public GameObject value;
}

[System.Serializable]
public class GraphicFader
{
    private Graphic graphic;
    [SerializeField] private float duration = 0.3f;

    private Coroutine routine;
    private readonly MonoBehaviour mono;

    public GraphicFader(MonoBehaviour mono)
    {
        this.mono = mono;
    }

    public void Reset(GameObject go)
    {
        graphic = go.GetComponent<Graphic>();
        Color c = graphic.color;
        c.a = 0f;
    }

    public void FadeTo(GameObject go, float targetAlpha)
    {
        if (graphic == null) graphic = go.GetComponent<Graphic>();
        if (graphic == null)
        {
            Debug.LogError("GraphicFader requires a UI Graphic (Image/Text/etc).");
            return;
        }

        if (routine != null) mono.StopCoroutine(routine);
        routine = mono.StartCoroutine(Fade(targetAlpha));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        Color c = graphic.color;
        float startAlpha = c.a;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            c.a = Mathf.Lerp(startAlpha, targetAlpha, p);
            graphic.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        graphic.color = c;

        routine = null;
    }
}