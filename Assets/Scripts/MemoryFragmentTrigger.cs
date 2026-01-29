using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class MemoryFragmentTrigger : MonoBehaviour, IInteractable
    {
        public MemoryFragment memoryFragment;
        public bool isFocused;
        public bool Interacting;
        public bool IsInteracting(bool interacting) => Interacting = interacting;


        private Dictionary<FragmentUIKey, GameObject> uiMap;
        private Dictionary<FragmentAudioKey, AudioClip> audioMap;

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

        public void InteractUI(bool trigger)
        {
            var prompt = GetUI(FragmentUIKey.Interact);
            if (prompt) prompt.SetActive(trigger);
        }

        public void OnInteractUI(bool trigger)
        {
            var prompt = GetUI(FragmentUIKey.OnInteract);
            if (prompt) prompt.SetActive(trigger);
        }

        public void CollectUI(bool trigger)
        {
            var prompt = GetUI(FragmentUIKey.Collect);
            if (prompt) prompt.SetActive(trigger);
        }

        public void OnCollectUI(bool trigger)
        {
            var prompt = GetUI(FragmentUIKey.OnCollect);
            if (prompt) prompt.SetActive(trigger);
        }

        public void PlayAudio(FragmentAudioKey key)
        {
            if (audioMap == null)
            {
                Debug.LogWarning($"{name}: audioMap not initialized yet.", this);
                return;
            }

            if (!audioMap.TryGetValue(key, out var clip) || clip == null)
                return;

            GameManager.Instance.audioSource.PlayOneShot(clip);
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
            
            UIInitialization();
            AudioInitialization();
        }

        private void AudioInitialization()
        {
            audioMap = new Dictionary<FragmentAudioKey, AudioClip>(4)
            {
                { FragmentAudioKey.Interact,   memoryFragment.interactAudio },
                { FragmentAudioKey.OnInteract, memoryFragment.onInteractAudio },
                { FragmentAudioKey.Collect,    memoryFragment.collectAudio },
                { FragmentAudioKey.OnCollect,  memoryFragment.onCollectAudio },
            };
        }

        private void UIInitialization()
        {
            var canvas = transform.GetChild(0);

            var interactUI   = Instantiate(memoryFragment.interactUI,   canvas, false);
            var onInteractUI = Instantiate(memoryFragment.onInteractUI, canvas, false);
            var collectUI    = Instantiate(memoryFragment.collectUI,    canvas, false);
            var onCollectUI  = Instantiate(memoryFragment.onCollectUI,  canvas, false);

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

            // Position (your current logic sets both to same height anyway)
            Vector3 worldPos = transform.position + Vector3.up * 1.5f;

            interactUI.rectTransform.position   = worldPos;
            onInteractUI.rectTransform.position = worldPos;
            collectUI.rectTransform.position    = worldPos;
            onCollectUI.rectTransform.position  = worldPos;

            // Build enum dictionary in one shot
            uiMap = new Dictionary<FragmentUIKey, GameObject>(4)
            {
                { FragmentUIKey.Interact,   interactUI.gameObject },
                { FragmentUIKey.OnInteract, onInteractUI.gameObject },
                { FragmentUIKey.Collect,    collectUI.gameObject },
                { FragmentUIKey.OnCollect,  onCollectUI.gameObject },
            };

            // Optional: warn if any UI refs missing in the ScriptableObject
            WarnIfNullUI(FragmentUIKey.Interact,   interactUI);
            WarnIfNullUI(FragmentUIKey.OnInteract, onInteractUI);
            WarnIfNullUI(FragmentUIKey.Collect,    collectUI);
            WarnIfNullUI(FragmentUIKey.OnCollect,  onCollectUI);
        }

        private void WarnIfNullUI(FragmentUIKey key, TextMeshProUGUI tmp)
        {
            if (!tmp)
                Debug.LogWarning($"{name}: UI '{key}' is not assigned on MemoryFragment.", this);
        }

        // Update is called once per frame
        void Update()
        {
            
        }
        
        private GameObject GetUI(FragmentUIKey key)
        {
            if (uiMap != null && uiMap.TryGetValue(key, out var go) && go)
                return go;

            Debug.LogWarning($"{name}: UI key '{key}' not found or is null.", this);
            return null;
        }
    }
}