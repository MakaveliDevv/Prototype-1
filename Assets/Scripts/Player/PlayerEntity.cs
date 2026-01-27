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

    public bool InputLocked { get; private set; }
    public void SetInputLocked(bool locked) => InputLocked = locked;

    // public void SetInputLocked(bool locked)
    // {
    //     InputLocked = locked;

    //     if(locked)
    //     {
    //         Cursor.lockState = CursorLockMode.None;
    //         Cursor.visible = true;  
    //     }
    //     else
    //     {
    //         Cursor.lockState = CursorLockMode.Locked;
    //         Cursor.visible = false;  
    //     }
    // }


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
