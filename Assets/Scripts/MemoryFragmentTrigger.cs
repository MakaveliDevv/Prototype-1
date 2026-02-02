using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class MemoryFragmentTrigger : MonoBehaviour, IInteractable
    {
        public Fragment memoryFragment;
        public bool isFocused;
        public bool Interacting { get; private set; }
        public bool IsInteracting(bool interacting) => Interacting = interacting;


        private Dictionary<UIKey, GameObject> uiMap;
        private Dictionary<AudioKey, AudioClip> audioMap;


        public Dictionary<UIKey, GameObject> UIMap => uiMap;
        public Object LogContext => this;

        public void Interact()
        {
            // if (Interacting) return;
            InteractUI(true);
        }
        
        public void OnInteract()
        {
            // if (Interacting) return;

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
        }

        public void InteractUI(bool trigger)
        {
    
        }

        public void OnInteractUI(bool trigger)
        {
      
        }

        public void CollectUI(bool trigger)
        {
     
        }

        public void OnCollectUI(bool trigger)
        {
   
        }

        public void PlayAudio(AudioKey key)
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
            
            GameManager.Instance.UIInitialization(memoryFragment, transform, ref uiMap);
            GameManager.Instance.AudioInitialization(memoryFragment, ref audioMap);
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}