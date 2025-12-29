using UnityEngine;

// [CreateAssetMenu(fileName = "MemoryFragment", menuName = "ScriptableObjects/MemoryFragments", order = 1)]
[CreateAssetMenu(fileName = "MemoryFragment", menuName = "Scriptable Objects/MemoryFragments")]
public class MemoryFragment : ScriptableObject
{
    public int id;
    public int floorIndex;
    public string title = "";
    public string descriptionText = "";
    public string tag = "";
    public AudioClip audioClip;
}
