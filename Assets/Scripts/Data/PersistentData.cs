using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PersistentData : MonoBehaviour
{
    public GameObject player;
    public GameObject Mav;
    public GameObject hotbarCanvas;
    public GameObject inventoryCanvas;
    public InventoryManager inventory;
    public AudioManager audioManager;
    public Settings settings;
    public FruitManager fruitManager;

    // currSaveData and currLevelDatas are private vars accessible through getters
    static SaveData currSaveData;
    // Level id is not necessarily the same as list index in currSaveData.levelDatas, so we store references to each LevelData in a dictionary. If you need level data, get it from currLevelDatas, not from currSaveData.levelDatas - note that any changes to currLevelDatas entries will be reflected in currSaveData because the dictionary contains references, not copies, BUT if you want to add a new LevelData object to the list, you MUST use AddLevelData.
    static Dictionary<int, LevelData> currLevelDatas;
    public GameObject eventSystem;
    public GameObject cameraObj;
    public GameObject cinemaMachineCamera;
    public GameObject cinemaMachineConfineBounds;
    public TimeManager timeManager;

    void Awake(){
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(eventSystem);
        DontDestroyOnLoad(hotbarCanvas);
        DontDestroyOnLoad(inventoryCanvas);
        DontDestroyOnLoad(cameraObj);
        DontDestroyOnLoad(cinemaMachineCamera);
        DontDestroyOnLoad(cinemaMachineConfineBounds);
        DontDestroyOnLoad(Mav);
        inventoryCanvas.SetActive(false);
        player.SetActive(false);
        Mav.SetActive(false);
    }

    // Save index 0 is always auto save; Save >= 1 (up to max) is manual save. 
    public void LoadSave(int saveIndex)
    {
        // Delete current in-game dynamic data and replace it with save file data
        currSaveData = null;
        if(currLevelDatas != null) currLevelDatas.Clear();

        DataManager.readFile(ref currSaveData, saveIndex);

        // Load in level datas for easier reference, since level id != elem index.
        if(currSaveData != null)
        {
            // Store references to each LevelData in a dictionary
            currLevelDatas = currSaveData.levelDatas.ToDictionary(keySelector: ld => ld.levelID, elementSelector: ld => ld);
            
            // Load in the scene
            LevelManager.LoadLevelScene(currSaveData.currLevelIndex);

            //Here is where I insert my code to link backend to frontend.
            // need to implement levelData, and eventData to frontend.

            // Load in current level
            LevelData currLevel = currLevelDatas[currSaveData.currLevelIndex];
            LevelManager.currentLevelID = currLevel.levelID;
            LevelManager.currentBiome = currLevel.biome;
            if (currSaveData.currLevelIndex == 0){
                LevelManager.currentOxygenLevel = 0;
            } else {
                LevelManager.currentOxygenLevel = currLevelDatas[currSaveData.currLevelIndex - 1].firstTargetOxygenLevel;
            }
            // TODO: load in plant in hand
            ////player.GetComponent<PlayerScript>().plantInHand = currLevel.plantInHand;
            // TODO: load in plant and pest datas
            //LevelManager.currentOxygenLevel = currLevel.oxygenLevel;

            // Initialize player position from player position in currSaveData
            var pos = player.transform.position;
            pos.x = currSaveData.playerData.location.x;
            pos.y = currSaveData.playerData.location.y;
            player.transform.position = pos;

            // Load in inventory from inventory in currSaveData
            inventory.LoadInventory(currSaveData.playerData);

            // Initialize settings from settings in currSaveData
            settings.fullScreen = currSaveData.gameStateData.settingsData.fullScreen;
            settings.loadScreen(settings.fullScreen);
            audioManager.volumeBGM = currSaveData.gameStateData.settingsData.volumeBGM;
            audioManager.OnMusicVolumeChanged(audioManager.volumeBGM);
            audioManager.volumeSFX = currSaveData.gameStateData.settingsData.volumeSFX;
            audioManager.OnSFXVolumeChanged(audioManager.volumeSFX);
            settings.loadVolumeSliders(audioManager.volumeBGM, audioManager.volumeSFX);
            settings.uiScaleIndex = currSaveData.gameStateData.settingsData.uiScaleIndex;
            settings.scaleUI(settings.uiScaleIndex);
        }
        else
        {
            // Load in the scene and create new save data
            LevelManager.LoadLevelScene(0);
            currSaveData = new SaveData();
            currSaveData.gameStateData = new GameStateData();
            currSaveData.gameStateData.timePassedSeconds = 0;
            currSaveData.gameStateData.timePassedMinutes = 0;
            currSaveData.gameStateData.timePassedHours = 0;
            currSaveData.gameStateData.timePassedDays = 0;
            currSaveData.gameStateData.settingsData = new SettingsData();
            LevelData firstLevelData = new LevelData();
            firstLevelData.levelID = 0;
            firstLevelData.biome = "plains"; // biome that level is in. 
            firstLevelData.plantDatas = new List<PlantData>(); // a list of all the existing, planted plants in a level. 
            // TODO: should this be an "agentsData" list containing pests, neutrals, NPCs, etc (non-player agents)
            firstLevelData.pestDatas = new List<PestData>();
            currLevelDatas = new Dictionary<int, LevelData>(){
                {0, firstLevelData},
            };
            inventory.LoadNewInventory();
        }

        player.SetActive(true);
        Mav.SetActive(true);
        hotbarCanvas.SetActive(true);
        cameraObj.SetActive(true);
        cinemaMachineCamera.SetActive(true);
        timeManager.StartGameTimer();

        // Play audio specific to whichever biome was loaded in
            if (LevelManager.currentBiome.Equals("plains")) AudioManager.GetSoundtrack("plainsSoundtrack").Play();
    }

    public static void WriteToSave(int saveIndex)
    {
        DataManager.writeFile(ref currSaveData, saveIndex);
    }

    public static LevelData GetLevelData(int levelID)
    {
        if (currLevelDatas.ContainsKey(levelID)){
            return currLevelDatas[levelID];
        } else {
            return new LevelData();
        }
        
    }

    // Add a new LevelData to both currSaveData and currLevelDatas. This method is only called when a new level is unlocked. Once a level is unlocked, it stays unlocked. 
    public static void AddLevelData(LevelData newLevelData) // Are they connected through same pass by value object? Test result: yes.
    {
        currSaveData.levelDatas.Add(newLevelData);
        currLevelDatas.Add(newLevelData.levelID, newLevelData);
    }

    // Thus this is why every data is categorized into a class; passed by reference: more intuitive and directly-reflected modification.
    public static PlayerData GetPlayerData()
    {
        return currSaveData.playerData;
    }

    public static GameStateData GetGameStateData()
    {
        return currSaveData.gameStateData;
    }
    
    public static EventsData GetEventsData()
    {
        return currSaveData.eventsData;
    }

    // This function is subject to change and finalization as dynamic variables increase!
    public void CreateNewSave(int saveIndex) // things we ignore are automatically obvious default values.
    {
        SaveData newSave = new SaveData();

        // deal with level datas
        newSave.levelDatas = new List<LevelData>();
        foreach (Level levelSO in LevelManager.levelSOsStatic){
            LevelData levelData = new LevelData();
            levelData.levelID = levelSO.levelID;
            levelData.biome = levelSO.biome;
            levelData.plantDatas = new List<PlantData>();
            GameObject[] plants = GameObject.FindGameObjectsWithTag("plant");
            if (plants.Length != 0){
                foreach (GameObject plant in plants){
                    levelData.plantDatas.Add(plant.GetComponent<PlantScript>().plantData);
                }
            }
            levelData.pestDatas = new List<PestData>();
            GameObject[] pests = GameObject.FindGameObjectsWithTag("pest");
            if (pests.Length != 0){
                foreach (GameObject pest in pests){
                    levelData.pestDatas.Add(pest.GetComponent<PestScript>().pestData);
                }
            }
            levelData.oxygenLevel = 0; //levelSO.oxygenLevel;
            levelData.firstTargetOxygenLevel = levelSO.firstTargetOxygenLevel;
            levelData.secondTargetOxygenLevel = levelSO.secondTargetOxygenLevel;
            PlantScript plantInHand = player.GetComponent<PlayerScript>().plantInHand;
            if (plantInHand != null) levelData.plantInHand = plantInHand.plantData;
            newSave.levelDatas.Add(levelData);
        }

        // deal with current level index
        newSave.currLevelIndex = LevelManager.currentLevelID; 

        // deal with player data
        PlayerData initPlayerData = new PlayerData();

        initPlayerData.location.x = player.transform.position.x;
        initPlayerData.location.y = player.transform.position.y;
        initPlayerData.inventoryItemDatas = new List<InventoryItemData>(inventory.numInventorySlots);
        for (int i = 0; i < inventory.numInventorySlots; i++){
            InventoryItemData inventoryItemData = new InventoryItemData();
            Transform slotTransform = inventory.inventorySlots[i].slotTransform;
            if (slotTransform.childCount == 0){
                inventoryItemData.itemName = "empty";
                inventoryItemData.count = 0;
            } else {
                InventoryItem inventoryItem = slotTransform.GetComponentInChildren<InventoryItem>();
                inventoryItemData.itemName = inventoryItem.displayName;
                inventoryItemData.count = inventoryItem.stackSize;
            }
            initPlayerData.inventoryItemDatas.Add(inventoryItemData);
        }
        initPlayerData.nSeafoam = fruitManager.nSeafoam;
        initPlayerData.nSunset = fruitManager.nSunset;
        initPlayerData.nAmethyst = fruitManager.nAmethyst;
        initPlayerData.nCrystalline = fruitManager.nCrystalline;

        newSave.playerData = initPlayerData;

        // deal with gamestate data
        GameStateData initGameStateData = new GameStateData();
        SettingsData initSettingsData = new SettingsData();

        initGameStateData.timePassedSeconds = currSaveData.gameStateData.timePassedSeconds;
        initGameStateData.timePassedMinutes = currSaveData.gameStateData.timePassedMinutes;
        initGameStateData.timePassedHours = currSaveData.gameStateData.timePassedHours;
        initGameStateData.timePassedDays = currSaveData.gameStateData.timePassedDays;

        initSettingsData.fullScreen = settings.fullScreen;
        initSettingsData.volumeBGM = audioManager.volumeBGM;
        initSettingsData.volumeSFX = audioManager.volumeSFX;
        initSettingsData.uiScaleIndex = settings.uiScaleIndex;
        
        initGameStateData.settingsData = initSettingsData;
        newSave.gameStateData = initGameStateData;

        // deal with event data
        EventsData initEventData = new EventsData();
        newSave.eventsData = initEventData;

        // write the current save data to the saveIndex save
        DataManager.writeFile(ref newSave, saveIndex);
        DataManager.writeSettingsChangeToFile(initSettingsData);
    }
}
