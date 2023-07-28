using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    GameObject background;

    public bool inventoryIsLoaded;
    public GameObject inventoryCanvas;
    public GameObject hotbarPanel;

    public bool settingsAreLoaded;
    public GameObject settingsCanvas;

    public AudioManager audioManager;

    private void Awake()
    {
        controls = new Controls();
        playerInput = GetComponent<PlayerInput>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inventoryIsLoaded = false;
        settingsAreLoaded = false;

        // Quickly loads inventory and settings in and out so it doesn't matter whether they are awake in Scene editor
        //    or not when Game is played.
        inventoryCanvas.SetActive(true);
        inventoryCanvas.SetActive(false);
        settingsCanvas.SetActive(true);
        settingsCanvas.SetActive(false);

        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

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
        controls.Main.Inventory.started += OnInventory;
        controls.Main.Settings.started += OnSettings;
        controls.Main.Hotbar.started += OnHotbarPress;
    }

    private void OnDisable()
    {
        controls.Main.Disable();
        controls.Main.Interact.started -= OnInteract;
        controls.Main.NewPlant.started -= GeneratePlant;
        controls.Main.Inventory.started -= OnInventory;
        controls.Main.Settings.started -= OnSettings;
        controls.Main.Hotbar.started -= OnHotbarPress;
    }

    private void FixedUpdate()
    {
        // Get character movement
        // If statement ensures that player cannot move while inventory screen is on
        if (!inventoryIsLoaded){
            Vector2 moveInput = controls.Main.Movement.ReadValue<Vector2>();
            Vector2 velocity = rb.velocity;

            // Flip sprite according to movement
            if (moveInput.x != 0) { spriteRenderer.flipX = moveInput.x > 0; }
                
            velocity.x = moveInput.x * speed;

            if (moveInput.y > 0 && velocity.y == 0) // prevents you from double jumping
            {
                velocity.y = moveInput.y * jumpSpeed;
            } 
            rb.velocity = velocity; // needed to ensure the changes we make go back to the rb
        }
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
        if (!(inventoryIsLoaded || TimeManager.IsGamePaused())){
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
    }

    public void GeneratePlant(InputAction.CallbackContext context)
    {
        if (!(inventoryIsLoaded || TimeManager.IsGamePaused())){
            //GameObject plant = GameManager.SpawnPlant(PlantName.Bob, GridScript.CoordinatesToGrid(transform.position));
            GameObject plant = GameManager.SpawnPlant(PlantName.PeachTree, GridScript.CoordinatesToGrid(transform.position));
            audioManager.plantSFX.Play();
            //if(plant != null) plant.GetComponent<PlantScript>().RunPlantModules(new List<PlantModuleEnum>() { PlantModuleEnum.Test });
        }
    }

    // Opens/closes inventory when player presses I
    public void OnInventory(InputAction.CallbackContext context)
    {
        if (!TimeManager.IsGamePaused()){
            if (!inventoryIsLoaded){
                inventoryCanvas.SetActive(true);
                inventoryIsLoaded = true;
            }
            else {
                inventoryCanvas.SetActive(false);
                inventoryIsLoaded = false;
            }
        }
    }

    // This functions exists for the exit button in inventory UI
    public void CloseInventory(){
        if (!TimeManager.IsGamePaused()){
            inventoryCanvas.SetActive(false);
            inventoryIsLoaded = false;
        }
    }

    // Opens/closes settings when player presses Escape
    public void OnSettings(InputAction.CallbackContext context){
        if (!TimeManager.IsGamePaused()){
                settingsCanvas.SetActive(true);
                settingsCanvas.GetComponent<Settings>().setPosition();
                TimeManager.PauseGame();
            }
        else {
            settingsCanvas.SetActive(false);
            TimeManager.ResumeGame();
        }
    }

    // Calls the Use() method for the item in hotbar slot whose key was pressed.
    public void OnHotbarPress(InputAction.CallbackContext context){
        if (!inventoryIsLoaded || !TimeManager.IsGamePaused()){
            string[] hotbarKeys = new string[]{"1","2","3","4","5","6","7","8","9"};
            string pressedKey = context.control.displayName;
            int index = Array.IndexOf(hotbarKeys, pressedKey);
            if (index != -1){
                HotbarManagerScript hotbarManager = hotbarPanel.GetComponent<HotbarManagerScript>();
                Transform linkedSlotTransform = hotbarManager.linkedSlotTransforms[index]; // need index-1 for zero-based indexing
                if (linkedSlotTransform.childCount > 0){
                    InventoryItem linkedInventoryItem = linkedSlotTransform.GetComponentInChildren<InventoryItem>();
                    linkedInventoryItem.Use();
                }
            }
        }
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

    private void OnTriggerEnter2D(Collider2D collision){
        // If player collides with collectible, the collect method is called for that collectible.
        if (collision.gameObject.tag == "collectible"){
            ICollectible collectible = collision.GetComponent<ICollectible>();
            if (collectible != null){
                collectible.Collect();
            }
        }
    }
}
