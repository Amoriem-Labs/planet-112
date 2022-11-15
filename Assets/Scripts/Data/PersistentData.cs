using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TODO: Make this a static class? MonoBehaviours cannot be static classes
// This is a global, essentially static class accessible by its name
public class PersistentData : MonoBehaviour
{
    // currSaveData and currLevelDatas are private vars accessible through getters
    static SaveData currSaveData;
    // Level id is not necessarily the same as list index in currSaveData.levelDatas, so we store references to each LevelData in a dictionary. If you need level data, get it from currLevelDatas, not from currSaveData.levelDatas - note that any changes to currLevelDatas entries will be reflected in currSaveData because the dictionary contains references, not copies, BUT if you want to add a new LevelData object to the list, you MUST use AddLevelData.
    static Dictionary<int, LevelData> currLevelDatas;

    // Save index 0 is always auto save; Save >= 1 (up to max) is manual save. 
    public static void LoadSave(int saveIndex)
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
        }
        else
        {
            Debug.LogError("ERROR: failed to load save " + saveIndex + ".");
        }
    }

    public static void WriteToSave(int saveIndex)
    {
        DataManager.writeFile(ref currSaveData, saveIndex);
    }

    public static LevelData GetLevelData(int levelID)
    {
        return currLevelDatas[levelID];
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
    public static void CreateNewSave(int saveIndex) // things we ignore are automatically obvious default values.
    {
        SaveData newSave = new SaveData();

        // deal with level datas
        newSave.levelDatas = new List<LevelData>();
        LevelData firstLevel = new LevelData();
        firstLevel.levelID = 0;
        newSave.levelDatas.Add(firstLevel);

        // deal with player data
        PlayerData initPlayerData = new PlayerData();
        newSave.playerData = initPlayerData;

        // deal with gamestate data
        GameStateData initGameStateData = new GameStateData();
        SettingsData initSettingsData = new SettingsData();
        initGameStateData.settingsData = initSettingsData;
        newSave.gameStateData = initGameStateData;

        // deal with event data
        EventsData initEventData = new EventsData();
        newSave.eventsData = initEventData;

        // write the current save data to the saveIndex save
        DataManager.writeFile(ref newSave, saveIndex);
    }
}
