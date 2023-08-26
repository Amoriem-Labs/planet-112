using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static int currentLevelID = 0; // change when level changes.
    public static string currentBiome = "plains";
    public static int currentOxygenLevel = 0; // change when oxygen level changes.
    //public static int currentFirstTargetOxygenLevel = 50; // change when level changes. Each level has its own first target level for amount of oxygen.
    //public static int currentSecondTargetOxygenLevel = 70; // change when level changes. Each level has its own second target level for amount of oxygen.
    public static Dictionary<int, int> plantOxygenContributions = new Dictionary<int, int>();
    public Level[] levelSOs; // list of level scriptable objects
    public static Level[] levelSOsStatic; // list of level scriptable objects
    public GameObject oxygenLevelCanvas;
    public Slider oxygenLevelSlider;
    public static Slider oxygenLevelSliderStatic;
    public GameObject firstOxygenLevelMark;
    public static GameObject firstOxygenLevelMarkStatic;
    public TextMeshProUGUI firstOxygenLevelText;
    public static TextMeshProUGUI firstOxygenLevelTextStatic;
    public TextMeshProUGUI secondOxygenLevelText;
    public static TextMeshProUGUI secondOxygenLevelTextStatic;

    void Awake(){
        DontDestroyOnLoad(oxygenLevelCanvas);
        oxygenLevelSliderStatic = oxygenLevelSlider;
        levelSOsStatic = levelSOs;
        firstOxygenLevelTextStatic = firstOxygenLevelText;
        secondOxygenLevelTextStatic = secondOxygenLevelText;
        firstOxygenLevelMarkStatic = firstOxygenLevelMark;
        foreach (Level levelSO in levelSOsStatic){
            levelSO.oxygenLevel = 0;
        }
    }

    public static int UpdateOxygenLevel(int plantID, int oxygenLevel){
        int totalOxygenLevel = 0;
        plantOxygenContributions[plantID] = oxygenLevel;
        foreach (KeyValuePair<int, int> entry in plantOxygenContributions){
            totalOxygenLevel += entry.Value;
        }
        levelSOsStatic[currentLevelID].oxygenLevel = totalOxygenLevel;
        oxygenLevelSliderStatic.value = levelSOsStatic[currentLevelID].oxygenLevel;
        firstOxygenLevelTextStatic.text = $"{levelSOsStatic[currentLevelID].oxygenLevel}/{levelSOsStatic[currentLevelID].firstTargetOxygenLevel}";
        secondOxygenLevelTextStatic.text = $"{levelSOsStatic[currentLevelID].oxygenLevel}/{levelSOsStatic[currentLevelID].secondTargetOxygenLevel}";
        return levelSOsStatic[currentLevelID].oxygenLevel;
    }

    public static void LoadLevel(int levelID){
        oxygenLevelSliderStatic.value = levelSOsStatic[levelID].oxygenLevel;
        oxygenLevelSliderStatic.maxValue = levelSOsStatic[levelID].secondTargetOxygenLevel;
        float sliderWidth = oxygenLevelSliderStatic.GetComponent<RectTransform>().rect.width; // is based off the width from RectTransform component of oxygenLevelSlider. Since oxygenLevelSlider is anchored in the middle of a Canvas object whose Camera is set to Screen Space - Camera Overlay, the position of the RectTransform is 0 and the actual width is twice what is displayed in RectTransform.
        float xPos = (levelSOsStatic[levelID].firstTargetOxygenLevel - (levelSOsStatic[levelID].secondTargetOxygenLevel / 2f)) * 0.01f * sliderWidth; // places firstLevelOxygenMark in proper position relative to oxygenLevelSlider
        firstOxygenLevelMarkStatic.transform.localPosition = new Vector3(xPos, 455, 0);
        firstOxygenLevelTextStatic.text = $"{levelSOsStatic[levelID].oxygenLevel}/{levelSOsStatic[levelID].firstTargetOxygenLevel}";
        secondOxygenLevelTextStatic.text = $"{levelSOsStatic[levelID].oxygenLevel}/{levelSOsStatic[levelID].secondTargetOxygenLevel}";
    }

    public static void LoadLevelScene(int levelID){
        SceneManager.LoadScene(levelSOsStatic[levelID].sceneName);
        LoadLevel(levelID);
        currentLevelID = levelID;
    }
}
