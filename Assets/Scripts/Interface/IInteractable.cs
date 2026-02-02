using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public interface IInteractable
    {
        Dictionary<UIKey, GameObject> UIMap { get; }

        Object LogContext { get; }

        public void Interact();
        public void OnInteract();
        public void Collect();
        public void OnCollect();
        public void PlayAudio(AudioKey key);

        // ---- Default UI behavior ----
        void InteractUI(bool trigger)    => SetUI(UIKey.Interact, trigger, "uiMap");
        void OnInteractUI(bool trigger)  => SetUI(UIKey.OnInteract, trigger, "uiMap");
        void CollectUI(bool trigger)     => SetUI(UIKey.Collect, trigger, "uiMap");
        void OnCollectUI(bool trigger)   => SetUI(UIKey.OnCollect, trigger, "uiMap");

        private void SetUI(UIKey key, bool trigger, string mapName)
        {
            // If your GetFromMapOrDefault is static (recommended):
            var prompt = GameManager.Instance.GetFromMapOrDefault(
                UIMap,
                key,
                fallback: null,
                logContext: LogContext,
                mapName: mapName
            );

            if (prompt) prompt.SetActive(trigger);
        }
    }
}