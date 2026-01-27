public interface IInteractable
{
    public void Interact();
    public void OnInteract();
    
    public void Collect();
    public void OnCollect();

    public void InteractUI(bool triggerPrompt);
    public void OnInteractUI(bool triggerPrompt);
    public void CollectUI(bool triggerPrompt);
    public void OnCollectUI(bool triggerPrompt);
    public void OnInteractAudio(bool triggerPrompt);
    public void OnCollectAudio(bool triggerPrompt);
}

