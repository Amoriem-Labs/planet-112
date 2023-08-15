using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour
{
    public List<PlantScript> closePlants = new List<PlantScript>();

    public TriggerResponse nearbyPlantDetectionRange;
    public TriggerResponse regularDetectionRange;
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] GameObject plantObject;

    Controls controls;
    PlayerInput playerInput;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    [SerializeField] float groundRay; // serialized to 0.5f
    [SerializeField] float diagonalRay; // serialized to 0.56f

    GameObject background;

    public bool inventoryIsLoaded;
    public GameObject inventoryCanvas;
    public GameObject hotbarPanel;

    public bool settingsAreLoaded;
    public GameObject settingsCanvas;

    public bool canOpenShop;
    public bool shopIsLoaded;
    public GameObject shopCanvas;
    public GameObject shopPopupButton;

    public GameObject playerPopupCanvas;
    private bool canMoveNextLevel;
    private bool canMovePreviousLevel;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        controls = new Controls();
        playerInput = GetComponent<PlayerInput>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inventoryIsLoaded = false;
        settingsAreLoaded = false;
        canOpenShop = false;
        shopIsLoaded = false;
        canMoveNextLevel = false;
        canMovePreviousLevel = false;

        // Quickly loads inventory and settings in and out so it doesn't matter whether they are awake in Scene editor
        //    or not when Game is played.
        inventoryCanvas.SetActive(true);
        inventoryCanvas.SetActive(false);
        settingsCanvas.SetActive(true);
        settingsCanvas.SetActive(false);
        shopCanvas.SetActive(true);
        shopCanvas.SetActive(false);

        // Set the popups to be invisible when initializing scene.
        shopPopupButton.SetActive(false);
        playerPopupCanvas.SetActive(false);

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
        nearbyPlantDetectionRange.onTriggerEnter2D = OnPlantDetectorTriggerEnter2D;
        nearbyPlantDetectionRange.onTriggerExit2D = OnPlantDetectorTriggerExit2D;
        regularDetectionRange.onTriggerEnter2D = OnRegularTriggerEnter2D;
        regularDetectionRange.onTriggerExit2D = OnRegularTriggerExit2D;
    }

    private void OnEnable()
    {
        controls.Main.Enable();
        controls.Main.Interact.started += OnInteract;
        controls.Main.NewPlant.started += GeneratePlant;
        controls.Main.Inventory.started += OnInventory;
        controls.Main.Settings.started += OnSettings;
        controls.Main.Hotbar.started += OnHotbarPress;
        controls.Main.Temp.started += OnResetInventory; // temporary keybinding. Just for debugging purposes.
    }

    private void OnDisable()
    {
        controls.Main.Disable();
        controls.Main.Interact.started -= OnInteract;
        controls.Main.NewPlant.started -= GeneratePlant;
        controls.Main.Inventory.started -= OnInventory;
        controls.Main.Settings.started -= OnSettings;
        controls.Main.Hotbar.started -= OnHotbarPress;
        controls.Main.Temp.started -= OnResetInventory;
    }

    private void FixedUpdate()
    {
        Vector2 velocity = rb.velocity;
        // Get character movement
        // If statement ensures that player cannot move while inventory screen is on
        if (!inventoryIsLoaded && !shopIsLoaded){
            Vector2 moveInput = controls.Main.Movement.ReadValue<Vector2>();

            // Flip sprite according to movement
            if (moveInput.x != 0) { spriteRenderer.flipX = moveInput.x > 0; }
                
            velocity.x = moveInput.x * speed;

            // First condition only triggers jump if player is pressing Up or W on keyboard, second condition and third condition prevents you from double jumping
            if (moveInput.y > 0 && isGrounded() && velocity.y == 0)
            {
                velocity.y = moveInput.y * jumpSpeed;
            } 
        } else {
            velocity.x = 0;
        }
        rb.velocity = velocity; // needed to ensure the changes we make go back to the rb
    }

    private bool isGrounded(){
        LayerMask obstacleLayerMask = LayerMask.GetMask("Obstacle");
        RaycastHit2D obstacleDownCheck = Physics2D.Raycast(transform.position, Vector2.down, groundRay, obstacleLayerMask);
        RaycastHit2D obstacleDiagonalDownLeftCheck = Physics2D.Raycast(transform.position, new Vector2(-1,-1), diagonalRay, obstacleLayerMask);
        RaycastHit2D obstacleDiagonalDownRightCheck = Physics2D.Raycast(transform.position, new Vector2(1,-1), diagonalRay, obstacleLayerMask);
        LayerMask groundLayerMask = LayerMask.GetMask("Ground");
        RaycastHit2D groundCheck = Physics2D.Raycast(transform.position, Vector2.down, groundRay, groundLayerMask);
        //8 is binary -- to look at just layer 3, we need binary 1000 

        return obstacleDownCheck.collider != null || obstacleDiagonalDownLeftCheck.collider != null || obstacleDiagonalDownRightCheck.collider != null || groundCheck.collider != null;
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
            // Below if statement makes sure that when there is both lilypad and 
            //      another plant planted on top of the lilypad, the player ALWAYS picks up the plant on top of the lilypad first
            if (currentPlantDist == closestPlantDist && !plant.plantSO.unlockPlantability){
                closestPlantDist = currentPlantDist;
                closestPlant = plant;
            }
        }
        return closestPlant; // null if empty, or closest plant is outside
    }

    PlantScript plantInHand = null;
    public void OnInteract(InputAction.CallbackContext context) // testing rn: press E to pick up & place plants
    {
        if (!inventoryIsLoaded && !TimeManager.IsGamePaused()){
            // If can move onto next level
            if (canMoveNextLevel){
                canMoveNextLevel = false;
                LevelManager.LoadLevelScene(LevelManager.currentLevelID + 1); // this automatically increments LevelManager's currentLevelID
                transform.position = new Vector2(0.25f, transform.position.y);
                GridScript.ClearGrid();
                GridScript.SpawnGrid(GridConfigs.levelGridDimensions[LevelManager.currentLevelID], 
                    PersistentData.GetLevelData(LevelManager.currentLevelID));
                return;
            }
            if (canMovePreviousLevel){
                canMovePreviousLevel = false;
                if (LevelManager.currentLevelID == 0){
                    Debug.Log("Cannot load previous level since you are on level 1 and there is no previous level!");
                    return;
                }
                LevelManager.LoadLevelScene(LevelManager.currentLevelID - 1); // this automatically decrements LevelManager's currentLevelID
                GridScript.ClearGrid();
                GridScript.SpawnGrid(GridConfigs.levelGridDimensions[LevelManager.currentLevelID], 
                    PersistentData.GetLevelData(LevelManager.currentLevelID));
                return;
            }

            if (!shopIsLoaded){
                if (plantInHand) // has a plant in hand
                {
                    if (plantInHand.PlacePlant(GridScript.CoordinatesToGrid(transform.position, plantInHand.plantSO.offset[plantInHand.plantData.currStageOfLife])))
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

            // If in front of Mav
            if (canOpenShop && !shopIsLoaded){
                shopCanvas.SetActive(true);
                shopCanvas.GetComponent<ShopManager>().OpenShopSelectUI();
                shopIsLoaded = true;
            } else {
                shopCanvas.SetActive(false);
                shopIsLoaded = false;
            }
        }
    }

    public void closeShopUI(){
        shopCanvas.SetActive(false);
        shopIsLoaded = false;
    }

    public void GeneratePlant(InputAction.CallbackContext context)
    {
        if (!(inventoryIsLoaded || TimeManager.IsGamePaused())){
            GameObject plant = GameManager.SpawnPlant(PlantName.Bob, GridScript.CoordinatesToGrid(transform.position));
            //if(plant != null) plant.GetComponent<PlantScript>().RunPlantModules(new List<PlantModuleEnum>() { PlantModuleEnum.Test });
        }
    }

    // Opens/closes inventory when player presses I
    public void OnInventory(InputAction.CallbackContext context)
    {
        if (!TimeManager.IsGamePaused() && !shopIsLoaded){
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
        if (!TimeManager.IsGamePaused() && !shopIsLoaded){
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
        if (!inventoryIsLoaded && !TimeManager.IsGamePaused() && !shopIsLoaded){
            string[] hotbarKeys = new string[]{"1","2","3","4","5","6","7","8","9"};
            string pressedKey = context.control.displayName;
            int index = Array.IndexOf(hotbarKeys, pressedKey);
            if (index != -1){
                HotbarManagerScript hotbarManager = hotbarPanel.GetComponent<HotbarManagerScript>();
                Transform linkedSlotTransform = hotbarManager.linkedSlotTransforms[index];
                if (linkedSlotTransform.childCount > 0){
                    ICollectible linkedItemPrefab = linkedSlotTransform.GetComponentInChildren<InventoryItem>().linkedItemPrefab.GetComponent<ICollectible>();
                    linkedItemPrefab.Use();
                }
            }
        }
    }

    // Resets inventory. Is triggered by pressing R. Temporary keybinding. Will remove this feature in final game.
    public void OnResetInventory(InputAction.CallbackContext context){
        inventoryCanvas.GetComponentInChildren<InventoryManager>().ResetInventory();
    }

    #region Collider methods.
    // The below two methods are for detecting nearby plants and use the PlantDetectionRange child object's BoxCollider2D.
    private void OnPlantDetectorTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "plant")
        {
            closePlants.Add(collision.gameObject.GetComponent<PlantScript>());
        }
    }

    private void OnPlantDetectorTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "plant")
        {
            closePlants.Remove(collision.gameObject.GetComponent<PlantScript>());
        }
    }

    // The below two methods are for regular collisions and use the RegularDetectionRange child object's BoxCollider2D.
    // In a "regular" collision, the player's BoxCollider2D is set to be the same size as the player sprite instead of being larger or smaller than the player sprite.
    private void OnRegularTriggerEnter2D(Collider2D collision){
        // If player collides with collectible, the collect method is called for that collectible.
        if (collision.gameObject.tag == "collectible"){
            ICollectible collectible = collision.GetComponent<ICollectible>();
            if (collectible != null){
                collectible.Collect();
            }
        }
        if (collision.gameObject.tag == "Mav"){
            canOpenShop = true;
            shopPopupButton.SetActive(true);
        }
        if (collision.gameObject.tag == "NearLeftWall" && LevelManager.currentLevelID != 0){
            canMovePreviousLevel = true;
            playerPopupCanvas.SetActive(true);
        }
        if (collision.gameObject.tag == "NearRightWall" && LevelManager.currentOxygenLevel >= LevelManager.currentFirstTargetOxygenLevel){
            canMoveNextLevel = true;
            playerPopupCanvas.SetActive(true);
        }
    }

    private void OnRegularTriggerExit2D(Collider2D collision){
        if (collision.gameObject.tag == "Mav"){
            canOpenShop = false;
            shopPopupButton.SetActive(false);
        }
        if (collision.gameObject.tag == "NearLeftWall"){
            canMovePreviousLevel = false;
            playerPopupCanvas.SetActive(false);
        }
        if (collision.gameObject.tag == "NearRightWall"){
            canMoveNextLevel = false;
            playerPopupCanvas.SetActive(false);
        }
    }
    #endregion
}
