using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts
{
    public class PlayerEntity : MonoBehaviour
    {
        public GameObject rig;
        public new Camera camera;
        public GameObject canvas;
        
        public InputActionAsset inputActionAsset;
        public PlayerSettings p_settings;

        [HideInInspector]
        public Movement movement;
        private CameraBehaviour cameraBh;
        public PlayerInteraction playerInter;
        public PlayerInteraction Interaction => playerInter; 

        [HideInInspector]
        public CharacterController charController;

        public bool InputLocked { get; private set; }
        public void SetInputLocked(bool locked) => InputLocked = locked;

        void Awake() 
        {
            charController = rig.GetComponent<CharacterController>();
            movement = new(this);
            cameraBh = new(this);
            playerInter = new(this);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            InputManager.Initialize(inputActionAsset);
        }

        // Update is called once per frame
        void Update()
        {
            InputManager.UpdateStickAndMoueInput();
            movement.Update();
            cameraBh.Update();
            playerInter.Update();
        }
    }
}

