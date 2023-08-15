using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Level")]
// Blueprint for easily designing new levels
public class Level : ScriptableObject
{
    public int levelID;
    public string sceneName;
    public string biome;
    public int oxygenLevel; 
    public int firstTargetOxygenLevel;
    public int secondTargetOxygenLevel;
}
