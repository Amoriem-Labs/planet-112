using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    public PlantScript plantScript;

    [SerializeField] float speed = 5f;

    Controls controls;
    PlayerInput playerInput;
    bool playerInputHasBeenInit = false;
    InputAction actionMovement;
    InputAction actionInteract;

    Rigidbody2D rb;
    Vector2 moveInput;





    private void Awake()
    {
        controls = new Controls();
        playerInput = GetComponent<PlayerInput>();
        actionMovement = controls.Main.Movement;
        actionInteract = controls.Main.Interact;
        rb = GetComponent<Rigidbody2D>();
        if (rb is null)
        {
            Debug.LogError("RigidBody2D is null!");
        }
    }

    private void OnEnable()
    {
        controls.Main.Enable();
    }

    public void InitPlayerInput()
    {

        Debug.Log("playerInput has been init");
        //if (playerInput.isActiveAndEnabled) return;
        //playerInput.actions["Movement"].performed +=
        //playerInput.actions["Interact"].performed += OnInteract;
        //playerInput.actions["Interact"].started += OnInteract;
        //playerInput.actions["Interact"].canceled += OnInteract;

        //actionInteract.performed += OnInteract;
        actionInteract.started += OnInteract;
        //actionInteract.canceled += OnInteract;
        Debug.Log("playerInput has been init");

        playerInputHasBeenInit = true;
    }

    private void OnDisable()
    {
        controls.Main.Disable();
        //playerInput.actions["Interact"].performed -= OnInteract;
        //playerInput.actions["Interact"].started -= OnInteract;
        //playerInput.actions["Interact"].canceled -= OnInteract;

        //actionInteract.performed -= OnInteract;
        actionInteract.started -= OnInteract;
        //actionInteract.canceled -= OnInteract;

        playerInputHasBeenInit = true;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!playerInputHasBeenInit)
        {
            InitPlayerInput();
        }
    }

    private void FixedUpdate()
    {
        // character movement
        moveInput.x = actionMovement.ReadValue<Vector2>().x;
        //moveInput.y = 0f;
        Vector2 vector2 = new Vector2(moveInput.x * speed, rb.velocity.y);
        rb.velocity = vector2;
    }

    private bool IsGrounded()
    {
        var groundCheck = Physics2D.Raycast(transform.position, Vector2.down, 0.7f);
        return groundCheck.collider != null && groundCheck.collider.CompareTag("Ground");
    }

    //private bool IsCloseToPlant()
    //{
        
    //}

    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("tried to interact");
        plantScript.IncrementState();
    }
}
