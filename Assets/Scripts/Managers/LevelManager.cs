using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static int currentLevelID = 0; // change when level changes.
    public static string currentBiome = "plains"; // change when biome changes.
    public static int currentOxygenLevel = 0; // change when oxygen level changes.
    public static int currentFirstTargetOxygenLevel = 50; // change when level changes. Each level has its own first target level for amount of oxygen.
    public static int currentSecondTargetOxygenLevel = 70; // change when level changes. Each level has its own second target level for amount of oxygen.
}
