using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public enum UIKey { Interact, OnInteract, Collect, OnCollect, DoorPanel }
    public enum AudioKey { Interact, OnInteract, Collect, OnCollect }
    
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
        public HashSet<MemoryFragmentTrigger> collectedFragments = new();
        public GraphicFader graphicFader;
        public AudioSource audioSource;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            graphicFader = new(this);
        }

        // Update is called once per frame
        void Update()
        {
            
        }
        
        public void CollectFragment(MemoryFragmentTrigger fragment)
        {
            if (!collectedFragments.Contains(fragment))
            {
                collectedFragments.Add(fragment);
                OnMemoryFragmentCollected(fragment.memoryFragment.id);
            }
        }

        public bool HasAllFragments(IReadOnlyCollection<string> requiredTags)
        {
            if (requiredTags == null || requiredTags.Count == 0)
                return false;

            var remaining = new HashSet<string>(requiredTags);

            foreach (var trigger in collectedFragments)
            {
                if (trigger == null) 
                    continue;

                var fragment = trigger.memoryFragment;
                if (fragment == null)
                    continue;

                var tag = fragment.tag; 
                if (string.IsNullOrWhiteSpace(tag))
                    continue;

                remaining.Remove(tag);

                if (remaining.Count == 0)
                    return true;
            }

            return false;
        }


        private void OnMemoryFragmentCollected(int fragmentId) { }

        private bool CanUnlockRooftop() { return false; }

        public float CalculatedDistance(Vector3 a, Vector3 b)
        {
            // Horizontal (XZ) only
            return Vector2.Distance(
                new Vector2(a.x, a.z),
                new Vector2(b.x, b.z)
            );
        }

        public bool RayCastHit(ref RaycastHit rayHit, GameObject go, float rayDistance, LayerMask layer)
        {
            return Physics.Raycast(
                go.transform.position,
                go.transform.forward,
                out rayHit,
                rayDistance,
                layer
            );
        }

        public bool TryGetFromMap<TKey, TValue>
        (
            IReadOnlyDictionary<TKey, TValue> map,
            TKey key,
            out TValue value)
        {
            value = default;

            if (map == null) return false;
            if (!map.TryGetValue(key, out value)) return false;

            // UnityEngine.Object has special null semantics
            if (value is Object unityObj)
                return unityObj != null;

            return value != null;
        }

        public TValue GetFromMapOrDefault<TKey, TValue>(
            IReadOnlyDictionary<TKey, TValue> map,
            TKey key,
            TValue fallback = default,
            Object logContext = null,
            string mapName = null)
        {
            if (TryGetFromMap(map, key, out var value))
                return value;

            if (logContext != null)
                Debug.LogWarning($"Missing key '{key}' in {(string.IsNullOrEmpty(mapName) ? "map" : mapName)}.", logContext);

            return fallback;
        }

        public void UIInitialization(Fragment fragment, Transform transform, ref Dictionary<UIKey, GameObject> map)
        {
            var canvas = transform.GetChild(0);

            var interactUI   = Instantiate(fragment.interactUI,   canvas, false);
            var onInteractUI = Instantiate(fragment.onInteractUI, canvas, false);
            var collectUI    = Instantiate(fragment.collectUI,    canvas, false);
            var onCollectUI  = Instantiate(fragment.onCollectUI,  canvas, false);

            // Disable all initially
            interactUI.gameObject.SetActive(false);
            onInteractUI.gameObject.SetActive(false);
            collectUI.gameObject.SetActive(false);
            onCollectUI.gameObject.SetActive(false);

            // Optional: nice names in hierarchy
            interactUI.gameObject.name   = "UI_Interact";
            onInteractUI.gameObject.name = "UI_OnInteract";
            collectUI.gameObject.name    = "UI_Collect";
            onCollectUI.gameObject.name  = "UI_OnCollect";

            Vector3 worldPos = transform.position + Vector3.up * 1.5f;

            interactUI.rectTransform.position   = worldPos;
            onInteractUI.rectTransform.position = worldPos;
            collectUI.rectTransform.position    = worldPos;
            onCollectUI.rectTransform.position  = worldPos;

            // Build enum dictionary in one shot
            map = new Dictionary<UIKey, GameObject>(4)
            {
                { UIKey.Interact,   interactUI.gameObject },
                { UIKey.OnInteract, onInteractUI.gameObject },
                { UIKey.Collect,    collectUI.gameObject },
                { UIKey.OnCollect,  onCollectUI.gameObject },
            };

            // Optional: warn if any UI refs missing in the ScriptableObject
            WarnIfNullUI(UIKey.Interact,   interactUI);
            WarnIfNullUI(UIKey.OnInteract, onInteractUI);
            WarnIfNullUI(UIKey.Collect,    collectUI);
            WarnIfNullUI(UIKey.OnCollect,  onCollectUI);
        }

        private void WarnIfNullUI(UIKey key, TextMeshProUGUI tmp)
        {
            if (!tmp)
                Debug.LogWarning($"{name}: UI '{key}' is not assigned on MemoryFragment.", this);
        }

        public void AudioInitialization(Fragment fragment, ref Dictionary<AudioKey, AudioClip> map)
        {
            map = new Dictionary<AudioKey, AudioClip>(4)
            {
                { AudioKey.Interact,   fragment.interactAudio },
                { AudioKey.OnInteract, fragment.onInteractAudio },
                { AudioKey.Collect,    fragment.collectAudio },
                { AudioKey.OnCollect,  fragment.onCollectAudio },
            };
        }
    
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
}