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
    public static GameObject oxygenLevelCanvasStatic;
    public Slider oxygenLevelSlider;
    public static Slider oxygenLevelSliderStatic;
    public GameObject firstOxygenLevelMark;
    public static GameObject firstOxygenLevelMarkStatic;
    public TextMeshProUGUI firstOxygenLevelText;
    public static TextMeshProUGUI firstOxygenLevelTextStatic;
    public TextMeshProUGUI secondOxygenLevelText;
    public static TextMeshProUGUI secondOxygenLevelTextStatic;
    public GameObject cameraObj;
    public static GameObject cameraObjStatic;
    public GameObject cinemaMachineCamera; 
    public static GameObject cinemaMachineCameraStatic;

    void Awake(){
        DontDestroyOnLoad(oxygenLevelCanvas);
        oxygenLevelCanvas.SetActive(false);
        oxygenLevelCanvasStatic = oxygenLevelCanvas;
        oxygenLevelSliderStatic = oxygenLevelSlider;
        levelSOsStatic = levelSOs;
        firstOxygenLevelTextStatic = firstOxygenLevelText;
        secondOxygenLevelTextStatic = secondOxygenLevelText;
        firstOxygenLevelMarkStatic = firstOxygenLevelMark;
        cameraObjStatic = cameraObj;
        cinemaMachineCameraStatic = cinemaMachineCamera;
    }

    void Update(){
        if (currentOxygenLevel >= levelSOsStatic[currentLevelID].firstTargetOxygenLevel && !levelSOs[currentLevelID].completed){
            AudioManager.GetSFX("levelSFX").Play();
            AudioManager.GetSFX("swooshSFX").Play();
            levelSOs[currentLevelID].completed = true;
        }
    }

    public static int UpdateOxygenLevel(int plantID, int oxygenLevel){
        int totalOxygenLevel = 0;
        plantOxygenContributions[plantID] = oxygenLevel;
        foreach (KeyValuePair<int, int> entry in plantOxygenContributions){
            totalOxygenLevel += entry.Value;
        }
        currentOxygenLevel = totalOxygenLevel;
        oxygenLevelSliderStatic.value = currentOxygenLevel;
        firstOxygenLevelTextStatic.text = $"{currentOxygenLevel}/{levelSOsStatic[currentLevelID].firstTargetOxygenLevel}";
        secondOxygenLevelTextStatic.text = $"{currentOxygenLevel}/{levelSOsStatic[currentLevelID].secondTargetOxygenLevel}";
        return currentOxygenLevel;
    }

    public static void LoadLevel(int levelID){
        Debug.Log($"Loading in level {levelID+1}");
        oxygenLevelCanvasStatic.SetActive(true);
        oxygenLevelSliderStatic.value = currentOxygenLevel;
        oxygenLevelSliderStatic.maxValue = levelSOsStatic[levelID].secondTargetOxygenLevel;
        float sliderWidth = oxygenLevelSliderStatic.GetComponent<RectTransform>().rect.width; // is based off the width from RectTransform component of oxygenLevelSlider. Since oxygenLevelSlider is anchored in the middle of a Canvas object whose Camera is set to Screen Space - Camera Overlay, the position of the RectTransform is 0 and the actual width is twice what is displayed in RectTransform.
        float xPos = (levelSOsStatic[levelID].firstTargetOxygenLevel - (levelSOsStatic[levelID].secondTargetOxygenLevel / 2f)) * 0.01f * sliderWidth; // places firstLevelOxygenMark in proper position relative to oxygenLevelSlider
        firstOxygenLevelMarkStatic.transform.localPosition = new Vector3(xPos, 455, 0);
        firstOxygenLevelTextStatic.text = $"{currentOxygenLevel}/{levelSOsStatic[levelID].firstTargetOxygenLevel}";
        secondOxygenLevelTextStatic.text = $"{currentOxygenLevel}/{levelSOsStatic[levelID].secondTargetOxygenLevel}";
    }

    public static void LoadLevelScene(int levelID){
        if (currentLevelID == levelSOsStatic.Length - 1){
            SceneManager.LoadScene("GameCompletedScene");
            MonoBehaviour instance = GameObject.FindObjectOfType<LevelManager>();
            instance.StopAllCoroutines();
            cameraObjStatic.SetActive(false);
            cinemaMachineCameraStatic.SetActive(false);
        } else {
            SceneManager.LoadScene(levelSOsStatic[levelID].sceneName);
            LoadLevel(levelID);
            currentLevelID = levelID;
            //StopCoroutine(GameManager.StartPestWaves);
            MonoBehaviour instance = GameObject.FindObjectOfType<LevelManager>();
            instance.StopAllCoroutines(); // TODO: make a better system for stopping previous level's start pest waves coroutine.
            Coroutine pestWaves = instance.StartCoroutine(GameManager.StartPestWaves(levelSOsStatic[levelID]));
            // If second target oxygen level is reached in the level, then stick damage will be boosted in the future levels.
            if (currentOxygenLevel >= levelSOsStatic[currentLevelID].secondTargetOxygenLevel){
                Combat.attackDamage *= 1.5f;
            }
        }
    }
}
