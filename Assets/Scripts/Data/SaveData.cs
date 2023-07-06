using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Contains all the info needed in a save file
[Serializable]
public class SaveData
{
    public List<LevelData> levelDatas; // a list of all the unlocked levels in a run
    public int currLevelIndex; // index of which level the player last saved 
    public PlayerData playerData; // explained in definition
    public GameStateData gameStateData; // explained in definition
    public EventsData eventsData; //explained in definition
}

#region LevelData
// Contains all the info needed in a level
[Serializable]
public class LevelData
{
    public int levelID; // id of the level. Each level (individual scene) has a unique id, labeled on the game map.
    public string biome; // biome that level is in. 
    public List<PlantData> plantDatas; // a list of all the existing, planted plants in a level. 
    // TODO: should this be an "agentsData" list containing pests, neutrals, NPCs, etc (non-player agents)
    public List<PestData> pestDatas; // similar. could be null (since using probaility model) or existing (aka saved during battle).
    public int oxygenLevel; // total oxygen level of the level, updated every time a plant spawns / dies.
    public int firstTargetOxygenLevel; // first target oxygen level of the level, once player reaches this oxygen level, they can move onto the next level
    public int secondTargetOxygenLevel; // second target oxygen level of the level, once player reaches this oxygen level, they get a 2% attack damage boost
    public int[] mapGrid; // the current distribution of this level's map's grids. Use 1D array math thingy to represent 2D array.
    public PlantData plantInHand; // if any, the plant the player picked up. Can move it to personal data for cross level or make it into a list. 
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
    //public List<int> plantModules; // OLD VERSION modules that this plant currently has
    public Dictionary<PlantModuleEnum, String> plantModuleData;
    // Status effects' durations?
}

// Contains the dynamic data of a pest
[Serializable]
public class PestData
{
    public Vector2 location; // x_index, y_index of this pest in the map 2D array
    // Recalculate current target plant
    public int currStageOfLife; // pest's current stage of life
    public int pestName; // the type of the pest, used to get the inherited, specific pest at run time.
    public float currentHealth; // remaining health of the pest
    public Dictionary<PestModuleEnum, String> pestModuleData;
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
    public Vector2 location; // x_index, y_index of the player in the map 2D array
    public List<InventoryItemData> inventoryItemDatas; // a list of all the items that the player has in the inventory
    public int nSeafoam; // number of seafoam icura player has in inventory
    public int nSunset; // number of sunset icura player has in inventory
    public int nAmethyst; // number of amethyst icura player has in inventory
    public int nCrystalline; // number of crystalline icura player has in inventory
}

// Contains the dynamic data of any item that can exist in an inventory. 
[Serializable]
public class InventoryItemData
{
    public string itemName; // name of the item
    public int count; // the amount of this item that the player has
    // public int inventorySlotIndex; // the location of this item in the inventory, may actually not need this variable since the list already has index in it
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
[Serializable]
public class SettingsData
{
    public bool fullScreen; // bool for whether the game is fullscreen or not
    public float volumeBGM; // slider volume of the background music
    public float volumeSFX; // slider volume of the SFX
    public int uiScaleIndex; // dropdown scale for size of UI
}
#endregion

#region EventsData
// Contains the completion status for the plot events, or progress bar towards one (like NPC relationships for ex), etc.
[Serializable]
public class EventsData
{
  // DEMO: marks whether the player has done the tutorial
  public bool tutorialCompleted;
}
#endregion
