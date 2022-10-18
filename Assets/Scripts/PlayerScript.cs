using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    PlantScript[] plantScripts;
    List<PlantScript> closePlants = new List<PlantScript>();
    PlantScript closestPlant;


    [SerializeField] float speed = 5f;
    [SerializeField] float interactRange = 50f;

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
        plantScripts = GameObject.FindObjectsOfType<PlantScript>();
        // add initially close plants to closePlants list


    }

    private void OnEnable()
    {
        controls.Main.Enable();
    }

    public void InitPlayerInput()
    {
        actionInteract.started += OnInteract;
        playerInputHasBeenInit = true;
        Debug.Log("playerInput has been init");

    }

    private void OnDisable()
    {
        controls.Main.Disable();
        actionInteract.started -= OnInteract;

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

        AddPlantsToList();
        findClosestPlant();
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

    public void AddPlantsToList()
    {
        foreach (PlantScript plant in plantScripts)
        {
            float currentPlantDist = Mathf.Abs(Vector3.Distance(plant.transform.position, transform.position));
            if (currentPlantDist < interactRange)
            {
                closePlants.Add(plant);
            }
            else if (closePlants.Contains(plant))
            {
                closePlants.Remove(plant);
            }
        }
    }


    private void findClosestPlant()
    {
        float closestPlantDist = Screen.width;
        closestPlant = null;
        foreach (PlantScript plant in closePlants)
        {
            float currentPlantDist = Vector3.Distance(transform.position, plant.transform.position);
            if (currentPlantDist < closestPlantDist)
            {
                closestPlantDist = currentPlantDist;
                closestPlant = plant;
            }
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        
        if (closestPlant)
        {
            closestPlant.IncrementState();
            //Debug.Log("tried to interact");
            //Debug.Log("closestPlant: " + closestPlant.name);
            //Debug.Log("plantScripts[0]: " + plantScripts[0].name);
            //Debug.Log("closePlants[0]: " + closePlants[0].name);
        }
        //else
        //{
        //    Debug.Log("no closestPlant");
        //}
    }
}
