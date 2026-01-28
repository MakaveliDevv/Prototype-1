using UnityEngine;
using TMPro;

// [CreateAssetMenu(fileName = "MemoryFragment", menuName = "ScriptableObjects/MemoryFragments", order = 1)]
[CreateAssetMenu(fileName = "MemoryFragment", menuName = "Scriptable Objects/MemoryFragments")]
public class MemoryFragment : ScriptableObject
{
    public int id;
    public int floorIndex;
    public string title = "";
    public string descriptionText = "";
    // public string tag = "";
    // public AudioClip audioClip;
    public string interactUIPrompt = "";
    public string collectUIPrompt = "";
    
    public TextMeshProUGUI interactUI;
    public TextMeshProUGUI onInteractUI;

    public TextMeshProUGUI collectUI;
    public TextMeshProUGUI onCollectUI;

    public AudioSource interactAudio;
    public AudioSource onInteractAudio;

    public AudioSource collectAudio;
    public AudioSource onCollectAudio;
}
