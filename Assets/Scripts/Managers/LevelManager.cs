using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static int currentLevelID = 0; // change when level changes.
    public static string currentBiome = "plains"; // change when biome changes.
    public static int currentOxygenLevel = 0; // change when oxygen level changes.
    public static int currentFirstTargetOxygenLevel = 50; // change when level changes. Each level has its own first target level for amount of oxygen.
    public static int currentSecondTargetOxygenLevel = 70; // change when level changes. Each level has its own second target level for amount of oxygen.
    public static Dictionary<int, int> plantOxygenContributions = new Dictionary<int, int>();
    public static Dictionary<int, string> levelScenes = new Dictionary<int, string>(){
        {0, "TestScene"},
        {1, "Level2Scene"},
    };
    public Slider oxygenLevelSlider;
    public static Slider oxygenLevelSliderStatic; // to be used in static methods
    //public GameObject firstOxygenLevelMark; // UI element that indicates where the first oxygen level mark is

    void Awake(){
        LoadLevelScene(currentLevelID);
        oxygenLevelSliderStatic = oxygenLevelSlider;
        oxygenLevelSliderStatic.value = 0;
        oxygenLevelSliderStatic.maxValue = currentSecondTargetOxygenLevel;
        float sliderWidth = oxygenLevelSlider.GetComponent<RectTransform>().rect.width; // is based off the width from RectTransform component of oxygenLevelSlider. Since oxygenLevelSlider is anchored in the middle of a Canvas object whose Camera is set to Screen Space - Camera Overlay, the position of the RectTransform is 0 and the actual width is twice what is displayed in RectTransform.
        float xPos = currentFirstTargetOxygenLevel - (currentSecondTargetOxygenLevel / 2f) * 0.01f * sliderWidth; // places firstLevelOxygenMark in proper position relative to oxygenLevelSlider
        firstOxygenLevelMark.transform.localPosition = new Vector3(xPos, 455, 0);
    }

    public static int UpdateOxygenLevel(int plantID, int oxygenLevel){
        currentOxygenLevel = 0;
        plantOxygenContributions[plantID] = oxygenLevel;
        foreach (KeyValuePair<int, int> entry in plantOxygenContributions){
            currentOxygenLevel += entry.Value;
        }
        if (currentOxygenLevel >= currentSecondTargetOxygenLevel){
            Debug.Log("Reached second target oxygen level for Level " + currentLevelID);
        } else if (currentOxygenLevel >= currentFirstTargetOxygenLevel){
            Debug.Log("Reached first target oxygen level for Level " + currentLevelID);
        }
        Debug.Log("Current oxygen level: " + currentOxygenLevel);
        oxygenLevelSliderStatic.value = currentOxygenLevel;
        return currentOxygenLevel;
    }

    public static void LoadLevelScene(int sceneID){
        currentLevelID = sceneID;
        SceneManager.LoadScene(levelScenes[sceneID]);
        currentOxygenLevel = PersistentData.GetLevelData(currentLevelID).oxygenLevel;
        currentFirstTargetOxygenLevel = PersistentData.GetLevelData(currentLevelID).firstTargetOxygenLevel;
        currentSecondTargetOxygenLevel = PersistentData.GetLevelData(currentLevelID).secondTargetOxygenLevel;
    }
}
