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
        Debug.Log("playerInput has been init");
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

    public void OnInteract(InputAction.CallbackContext context)
    {
        PlantScript closestPlant = findClosestPlant();
        if (closestPlant) // Could be null, gotta check
        {
            //closestPlant.IncrementState();
        }
    }

    public void GeneratePlant(InputAction.CallbackContext context)
    {
        // TODO: Figure out the y-coordinate computationally instead of hard-coding it
        //Vector3 plantLocation = new Vector3(transform.position.x, -2.84f, transform.position.z);
        //Instantiate(plantObject, plantLocation, transform.rotation);

        GameObject tst = Instantiate(FindObjectOfType<GameManager>().bob.gameObject);
        tst.GetComponent<ProductivePlant>().SpawnNewPlant((int)transform.position.x, -3);

        tst.GetComponent<ProductivePlant>().TryProduce();
    }
}
