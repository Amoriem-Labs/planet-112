using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    public List<PlantScript> closePlants = new List<PlantScript>();

    public TriggerResponse detectionRange;
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float sideRay; //set to 0.4f
    [SerializeField] float diagonalRay; //set to 0.435f
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

        // Debug.Log(null || null);

        // Pre-existing or prefab instantiation is faster than run time script generation
        /*GameObject childObject = new GameObject();
        childObject.transform.SetParent(gameObject.transform);
        childObject.transform.localPosition = Vector2.zero;
        childObject.layer = LayerMask.NameToLayer("Detectors");
        DynamicColliderScript colliderScript = childObject.AddComponent<DynamicColliderScript>();
        colliderScript.gameObject.name = "DetectionRange";
        colliderScript.SetCollider(typeof(BoxCollider2D), new Vector2(0, 0), new Vector2(2, 0.9f), 0,
            OnDetectorTriggerEnter2D, OnDetectorTriggerExit2D);*/
        detectionRange.onTriggerEnter2D = OnDetectorTriggerEnter2D;
        detectionRange.onTriggerExit2D = OnDetectorTriggerExit2D;
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

        if (moveInput.y > 0 && velocity.y == 0) // prevents you from double jumping
        {
            velocity.y = moveInput.y * jumpSpeed;
        } 
        rb.velocity = velocity; // needed to ensure the changes we make go back to the rb
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
            if (plantInHand.PlacePlant(GridScript.CoordinatesToGrid(transform.position)))
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

        //if(plant != null) plant.GetComponent<PlantScript>().RunPlantModules(new List<PlantModuleEnum>() { PlantModuleEnum.Test });

    }

    // Player interaction.
    private void OnDetectorTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "plant")
        {
            closePlants.Add(collision.gameObject.GetComponent<PlantScript>());
        }
    }
    private void OnDetectorTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "plant")
        {
            closePlants.Remove(collision.gameObject.GetComponent<PlantScript>());
        }
    }
}
