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
    [SerializeField] float slidingVelocityMultiplier = 0.5f;
    [SerializeField] float sideRay; //set to 0.52f
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

        if (moveInput.y > 0 && IsGrounded()) // prevents you from double jumping
        {
            velocity.y = moveInput.y * jumpSpeed;
            //rb.AddForce(new Vector2(0f, jumpSpeed), ForceMode2D.Impulse);
        } 
        // else if(!IsGrounded())
        // {
        //     velocity.y -= 0.2f;
        // }

        // if (!IsGrounded()) // Apply gravity only if the player is not grounded
        // {
        //     velocity.y += Physics2D.gravity.y * Time.fixedDeltaTime;
        // }
        Debug.Log("IsGrounded: " + IsGrounded());
        if (!IsGrounded() && IsTouchingWall()) // Apply sliding velocity if the player is not grounded and touching a wall
        {
            rb.velocity = new Vector2(rb.velocity.x * slidingVelocityMultiplier, rb.velocity.y);
            // velocity.y -= Physics2D.gravity * Time.deltaTime;
        }

        rb.velocity = velocity; // needed to ensure the changes we make go back to the rb
        // Debug.Log("Velocity: " + rb.velocity);
        // Debug.Log("IsGrounded: " + IsGrounded());
        // Debug.Log("IsTouchingWallenfvkjelkekljlkejblkefjlk");
        // Debug.Log("IsTouchingWall: " + IsTouchingWall());
    }

    private bool IsGrounded()
    {
        RaycastHit2D groundCheck = Physics2D.Raycast(transform.position, Vector2.down, groundedRay, 8);
        //8 is binary -- to look at just layer 3, we need binary 1000 

        return groundCheck.collider != null && groundCheck.collider.gameObject.CompareTag("Ground");
    }

    private bool IsTouchingWall()
    {
        RaycastHit2D wallRightCheck = Physics2D.Raycast(transform.position, Vector2.right, sideRay, 8);
        RaycastHit2D wallLeftCheck = Physics2D.Raycast(transform.position, Vector2.left, sideRay, 8);

        Debug.Log("wallRightCheck: " + wallRightCheck.collider);
        Debug.Log("wallLeftCheck: " + wallLeftCheck.collider);
        Debug.Log("IsGrounded: " + IsGrounded());
        Debug.Log("Does wallright exist: " + (wallRightCheck.collider != null));
        Debug.Log("Does wallleft exist: " + (wallLeftCheck.collider != null));
        Debug.Log("wallRightCheck.collider.gameObject.CompareTag(\"Obstacle\"): " + wallRightCheck.collider.gameObject.CompareTag("Obstacle")); // without this line and the one below it the player still gets stuck...
        Debug.Log("wallLeftCheck.collider.gameObject.CompareTag(\"Obstacle\"): " + wallLeftCheck.collider.gameObject.CompareTag("Obstacle"));

        return (wallRightCheck.collider != null || wallLeftCheck.collider != null) && (wallRightCheck.collider.gameObject.CompareTag("Obstacle") || wallLeftCheck.collider.gameObject.CompareTag("Obstacle"));
    }

    // private bool IsTouchingWall()
    // {
    //     RaycastHit2D wallRightCheck = Physics2D.Raycast(transform.position, Vector2.right, sideRay, 8);
    //     RaycastHit2D wallLeftCheck = Physics2D.Raycast(transform.position, Vector2.left, sideRay, 8);

    //     Debug.Log("wallRightCheck: " + wallRightCheck.collider);
    //     Debug.Log("wallLeftCheck: " + wallLeftCheck.collider);
    //     Debug.Log("wallRightCheck.collider?.gameObject.CompareTag(\"Obstacle\"): " + (wallRightCheck.collider?.gameObject.CompareTag("Obstacle") ?? false));
    //     Debug.Log("wallLeftCheck.collider?.gameObject.CompareTag(\"Obstacle\"): " + (wallLeftCheck.collider?.gameObject.CompareTag("Obstacle") ?? false));

    //     return (wallRightCheck.collider?.gameObject.CompareTag("Obstacle") ?? false) || (wallLeftCheck.collider?.gameObject.CompareTag("Obstacle") ?? false);
    // }


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
