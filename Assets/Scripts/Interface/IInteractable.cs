namespace Assets.Scripts
{
    public interface IInteractable
    {
        public void Interact();
        public void OnInteract();
        
        public void Collect();
        public void OnCollect();

        public void InteractUI(bool trigger);
        public void OnInteractUI(bool trigger);
        public void CollectUI(bool trigger);
        public void OnCollectUI(bool trigger);
        public void PlayAudio(FragmentAudioKey key);
        // public void UI(FragmentUIKey key);
    }
}