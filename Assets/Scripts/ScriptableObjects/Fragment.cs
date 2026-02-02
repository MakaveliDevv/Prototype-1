using UnityEngine;
using TMPro;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "Fragment", menuName = "Scriptable Objects/Fragments")]
    public class Fragment : ScriptableObject
    {
        public int id;
        public int floorIndex;
        public string title = "";
        public string descriptionText = "";
        public string tag = "";
        
        public TextMeshProUGUI interactUI;
        public TextMeshProUGUI onInteractUI;

        public TextMeshProUGUI collectUI;
        public TextMeshProUGUI onCollectUI;

        public AudioClip interactAudio;
        public AudioClip onInteractAudio;

        public AudioClip collectAudio;
        public AudioClip onCollectAudio;
    }
}