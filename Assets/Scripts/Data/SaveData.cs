using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Contains all the info needed in a save file
[Serializable]
public class SaveData
{
    public List<LevelData> levelDatas; // a list of all the unlocked levels in a run
    public PlayerData playerData; // explained in defintion
    public GameStateData gameStateData; // explained in defintion
    public EventsData eventsData; //explained in definition
}

#region LevelData
// Contains all the info needed in a level
[Serializable]
public class LevelData
{
    public int levelID; // id of the level. Each level (individual scene) has a unique id, labeled on the game map.
    public List<PlantData> plantDatas; // a list of all the existing, planted plants in a level. 
    public List<PestData> pestDatas; // similar. could be null (since using probaility model) or existing (aka saved during battle).
    public int oxygenLevel; // total oxygen level of the level, updated every time a plant spawns / dies.
}

// Contains the dynamic data of a plant object to be stored into json. 
[Serializable]
public class PlantData
{
    public Vector2 location; // x_index, y_index of this plant in the map 2D array
    public int currStageOfLife; // plant's current stage of life
    public int plantName; // the type of the plant, used to get the inherited, specific plant at run time.
    public float stageTimeLeft; // time left before the plant evolves
    public float currentHealth; // remaining health of the plant
    //public float fruitProductionRate; //subject to status-effect, otherwise fixed.
    // Status effects' durations?
}

// Contains the dynamic data of a pest
[Serializable]
public class PestData
{
    public Vector2 location; // x_index, y_index of this pest in the map 2D array
    // Recalculate current target plant
    public int pestType; // the type of the pest, used to get the inherited, specific pest at run time.
    public float currentHealth; // remaining health of the pest
    //public float attackDamage; //subject to status-effect, otherwise fixed.
    //public float attackRange; //subject to status-effect, otherwise fixed.
    // Status effects' durations?
}
#endregion

#region PlayerData
// Contains data relating to the player, ex. num fruits, num & type seeds / tools, inventory stuff, etc.
[Serializable]
public class PlayerData
{
    public int numFruits; // the number of fruits (currency) the player has
    public List<InventoryItemData> inventoryItemDatas; // a list of all the items that the player has in the inventory
}

// Contains the dynamic data of any item that can exist in an inventory. 
[Serializable]
public class InventoryItemData
{
    public int itemType; // the type of the item, used to get the inherited, specific item at run time.
    public int quantityCount; // the amount of this item that the player has
    public int slotIndex; // Which slot in the inventory is the item placed in? If MC: 0-9 hand, 10-MAX backpack, etc.
}
#endregion

#region GameStateData
// Contains data relating to the game state, ex. settings, total time passed, etc.
[Serializable]
public class GameStateData
{
    // these together account for the total time that has passed since the creation of the save, cumulative. 
    public int timePassedSeconds; 
    public int timePassedMinutes;
    public int timePassedHours;
    public int timePassedDays;
    public SettingsData settingsData; // explained in the definition
}

// Contains the settings data that the player configurated
public class SettingsData
{
    public float volumeBGM; // slider volume of the background music
    public bool mutedBGM; // is the background music muted?
}
#endregion

#region EventsData
// Contains the completion status for the plot events, or progress bar towards one (like NPC relationships for ex), etc.
[Serializable]
public class EventsData
{
    public bool tutorialCompleted; // marks whether the player has done the tutorial
}
#endregion
