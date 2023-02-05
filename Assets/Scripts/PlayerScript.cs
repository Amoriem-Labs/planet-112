using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    public List<PlantScript> closePlants = new List<PlantScript>();

    [SerializeField] float speed = 5f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] GameObject plantObject;

    Controls controls;
    PlayerInput playerInput;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    [SerializeField] float groundedRay;

    private void Awake()
    {
        controls = new Controls();
        playerInput = GetComponent<PlayerInput>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();
        if (rb is null)
        {
            Debug.LogError("RigidBody2D is null!");
        }
    }

    private void OnEnable()
    {
        controls.Main.Enable();
        controls.Main.Interact.started += OnInteract;
        controls.Main.NewPlant.started += GeneratePlant;
    }

    private void OnDisable()
    {
        controls.Main.Disable();
        controls.Main.Interact.started -= OnInteract;
        controls.Main.NewPlant.started -= GeneratePlant;
    }

    private void FixedUpdate()
    {
        // Get character movement
        Vector2 moveInput = controls.Main.Movement.ReadValue<Vector2>();
        
        // Flip sprite according to movement
        if (moveInput.x != 0) { spriteRenderer.flipX = moveInput.x > 0; }
        
        Vector2 velocity = rb.velocity;
        
        velocity.x = moveInput.x * speed;
        
        if (moveInput.y > 0 && IsGrounded())
        {
            velocity.y = moveInput.y * jumpSpeed;
        }
        
        rb.velocity = velocity;
    }

    private bool IsGrounded()
    {
        RaycastHit2D groundCheck = Physics2D.Raycast(transform.position, Vector2.down, groundedRay, 8); 
        //8 is binary -- to look at just layer 3, we need binary 1000 

        return groundCheck.collider != null && groundCheck.collider.gameObject.CompareTag("Ground");
    }

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
        return closestPlant; // null if empty, or closest plant is outside
    }

    PlantScript plantInHand = null;
    public void OnInteract(InputAction.CallbackContext context) // testing rn: press E to pick up & place plants
    {
        //closestPlant.TakeDamage(50);
        //Debug.Log("Closest Plant: ow! My current hp is: " + closestPlant.plantData.currentHealth);
        if (plantInHand) // has a plant in hand
        {
            if(plantInHand.PlacePlant(GridScript.CoordinatesToGrid(transform.position)))
            {
                plantInHand = null;
            }
            else
            {
                Debug.Log("Can't place it here; not enough space.");
            }
        }
        else // no plant in hand
        {
            PlantScript closestPlant = findClosestPlant();
            if (closestPlant) // Could be null, gotta check
            {
                closestPlant.LiftPlant(transform);
                plantInHand = closestPlant;
            }
        }
    }

    public void GeneratePlant(InputAction.CallbackContext context)
    {
        GameObject plant = GameManager.SpawnPlant(PlantName.Bob, GridScript.CoordinatesToGrid(transform.position));

        if(plant != null) plant.GetComponent<PlantScript>().RunPlantModules(new List<PlantModuleEnum>() { PlantModuleEnum.Test });

    }
}
