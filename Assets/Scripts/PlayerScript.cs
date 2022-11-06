using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    List<PlantScript> plantScripts = new List<PlantScript>();
    public List<PlantScript> closePlants = new List<PlantScript>();


    [SerializeField] float speed = 5f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float interactRange = 50f;
    [SerializeField] GameObject plantObject;

    Controls controls;
    PlayerInput playerInput;
    InputAction actionMovement;
    InputAction actionInteract;
    InputAction actionNewPlant;

    Rigidbody2D rb;
    Vector2 moveInput;
    SpriteRenderer spriteRenderer;
    [SerializeField] float groundedRay;

    private void Awake()
    {
        controls = new Controls();
        playerInput = GetComponent<PlayerInput>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        actionMovement = controls.Main.Movement;
        actionInteract = controls.Main.Interact;
        actionNewPlant = controls.Main.NewPlant;
        rb = GetComponent<Rigidbody2D>();
        if (rb is null)
        {
            Debug.LogError("RigidBody2D is null!");
        }
        foreach (PlantScript plant in GameObject.FindObjectsOfType<PlantScript>())
        {
            plantScripts.Add(plant);
        }
        // add initially close plants to closePlants list


    }

    private void OnEnable()
    {
        controls.Main.Enable();
        actionInteract.started += OnInteract;
        actionNewPlant.started += GeneratePlant;
        Debug.Log("playerInput has been init");
    }

    private void OnDisable()
    {
        controls.Main.Disable();
        actionInteract.started -= OnInteract;
        actionNewPlant.started -= GeneratePlant;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        // character movement
        moveInput.x = actionMovement.ReadValue<Vector2>().x;
        moveInput.y = actionMovement.ReadValue<Vector2>().y;
        // flip sprite according to movement
        if (moveInput.x > 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (moveInput.x < 0 )
        {
            spriteRenderer.flipX = false;
        }
        Debug.DrawRay(transform.position, Vector2.down, Color.green, 1, false);

        Vector2 vector2 = new Vector2(moveInput.x * speed, rb.velocity.y);
        //jump
        if (moveInput.y > 0 && IsGrounded()) 
        {
            vector2 = new Vector2(moveInput.x * speed, moveInput.y * jumpSpeed);
        }
        rb.velocity = vector2;
        
    }

    private bool IsGrounded()
    {
        RaycastHit2D groundCheck = Physics2D.Raycast(transform.position, Vector2.down, groundedRay, 8); 
        //8 is binary -- to look at just layer 3, we need binary 1000 

        return groundCheck.collider != null && groundCheck.collider.gameObject.CompareTag("Ground");
    }

    /*public void AddPlantsToList()
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
    }*/


    public PlantScript findClosestPlant()
    {
        float closestPlantDist = Screen.width;
        PlantScript closestPlant = null;
        foreach (PlantScript plant in closePlants)
        {
            float currentPlantDist = Vector3.Distance(transform.position, plant.transform.position);
            if (currentPlantDist < closestPlantDist)
            {
                closestPlantDist = currentPlantDist;
                closestPlant = plant;
            }
        }
        return closestPlant; //null if empty, or closest plant is outside
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        PlantScript closestPlant = findClosestPlant();
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

    public void GeneratePlant(InputAction.CallbackContext context)
    {
        Vector3 plantLocation = new Vector3(transform.position.x, -2.84f, transform.position.z);
        GameObject newPlant = Instantiate(plantObject, plantLocation, transform.rotation);
        // add the new plant to the overall list:
        plantScripts.Add(newPlant.GetComponent<PlantScript>());
    }
}
