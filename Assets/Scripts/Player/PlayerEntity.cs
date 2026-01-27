using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEntity : MonoBehaviour
{
    public GameObject rig;
    public new Camera camera;
    public InputActionAsset inputActionAsset;
    public PlayerSettings p_settings;

    [HideInInspector]
    public Movement movement;
    private CameraBehaviour cameraBh;
    private PlayerInteraction playerInter;

    [HideInInspector]
    public CharacterController charController;

    void Awake() 
    {
        charController = rig.GetComponent<CharacterController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputManager.Initialize(inputActionAsset);

        movement = new(this);
        cameraBh = new(this);
        playerInter = new(this);

        // playerInter.Start();
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
