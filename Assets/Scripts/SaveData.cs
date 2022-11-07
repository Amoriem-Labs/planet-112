using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public List<LevelData> levelDatas; 

    //Some other player UI or personal data
}

[Serializable]
public class LevelData
{
    public int levelID; 

    public List<PlantData> plantDatas;
    //Other lists of data regarding to this specific level
}

//This contains the dynamic data of a plant object to be stored into json. 
[Serializable]
public class PlantData
{
    public Vector2 location; //int x_index, y_index of this plant in the map 2D array
    public int currStageOfLife;
    public int plantType;
    public float stageTimeLeft;
}

//Other classes for other types of data
