using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    public bool fullScreen;
    public Toggle fullScreenToggleCheckbox;
    public GameObject cam;
    public GameObject hotbarCanvas;
    public GameObject inventoryCanvas;
    public GameObject shopCanvas;
    private Vector3 settingsStartingScale;
    private Vector3 hotbarStartingScale;
    private Vector3 inventoryStartingScale;
    private Vector3 shopStartingScale;
    public GameObject quitPanel;
    public GameObject savePanel;
    public Slider musicSlider;
    public Slider sfxSlider;
    public float[] uiScalings = new float[]{0.8f, 0.9f, 1.0f, 1.1f, 1.2f};
    public int uiScaleIndex;
    public PersistentData persistentData;
    public List<Text> saveFileBiomeTexts;
    public List<Text> saveFileOxygenLevelTexts;
    public List<Image> saveFileYraSprites;
    public Sprite currentYraSprite; // If we are continuing to develop Planet 112 and making clothing sets feature a reality, will need to find some way to make this referencing to a sprite dynamic. Currently, it is only set to the yra-idle sprite.

    void Awake(){
        DontDestroyOnLoad(gameObject);
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        fullScreen = true;
        settingsStartingScale = transform.localScale;
        hotbarStartingScale = hotbarCanvas.transform.localScale;
        inventoryStartingScale = inventoryCanvas.transform.localScale;
        shopStartingScale = shopCanvas.transform.localScale;
        uiScaleIndex = 2; // initial UI scale index
        GetComponentInChildren<Dropdown>().value = uiScaleIndex;
        gameObject.SetActive(false);
    }

    // This script is to update the position of SettingsCanvas to match camera position when settings are loaded.
    public void setPosition()
    {
        var pos = transform.position;
        pos.x = cam.transform.position.x;
        pos.y = cam.transform.position.y;
        transform.position = pos;
    }

    public void loadScreen(bool settingsFullScreen){
        fullScreenToggleCheckbox.isOn = settingsFullScreen;
        if (settingsFullScreen){
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            fullScreen = true;
        } else {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            fullScreen = false;
        }
    }

    public void loadVolumeSliders(float volumeBGM, float volumeSFX){
        musicSlider.value = volumeBGM;
        sfxSlider.value = volumeSFX;
    }

    // TODO: need to test if this works.
    public void setFullScreen(){
        if( !fullScreen ) {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            fullScreen = true;
        }
        else {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            fullScreen = false;
        }
    }

    public void scaleUI(int val){
        transform.localScale = Vector3.Scale(settingsStartingScale, new Vector3(uiScalings[val], uiScalings[val], 1.0f));
        hotbarCanvas.transform.localScale = Vector3.Scale(hotbarStartingScale, new Vector3(uiScalings[val], uiScalings[val], 1.0f));
        inventoryCanvas.transform.localScale = Vector3.Scale(inventoryStartingScale, new Vector3(uiScalings[val], uiScalings[val], 1.0f));
        shopCanvas.transform.localScale = Vector3.Scale(shopStartingScale, new Vector3(uiScalings[val], uiScalings[val], 1.0f));
        uiScaleIndex = val;
        GetComponentInChildren<Dropdown>().value = val;
    }

    public void goToSaveScreen(){
        exitQuitScreen();
        savePanel.SetActive(true);
    }

    public void exitSaveScreen(){
        savePanel.SetActive(false);
    }

    // TODO: very hard code-y, fix later
    public void saveGame_1(){
        exitSaveScreen();
        updateSaveFileUI(1);
        persistentData.CreateNewSave(1);
        Application.Quit();
    }

    public void saveGame_2(){
        exitSaveScreen();
        updateSaveFileUI(2);
        persistentData.CreateNewSave(2);
        Application.Quit();
    }

    public void saveGame_3(){
        exitSaveScreen();
        updateSaveFileUI(3);
        persistentData.CreateNewSave(3);
        Application.Quit();
    }

    public void goToQuitScreen(){
        // prompt user to save or not before quitting
        exitSaveScreen();
        quitPanel.SetActive(true);
    }

    public void exitQuitScreen(){
        quitPanel.SetActive(false);
    }

    public void exitSettings(){
        gameObject.SetActive(false);
        TimeManager.ResumeGame();
    }

    // Updates the save file UI for a specific saveIndex in the savePanel of the Settings menu
    public void updateSaveFileUI(int saveIndex){
        // Initializing colors with alpha = 1, and alpha = 0, respectively.
        Color full = saveFileYraSprites[saveIndex - 1].color;
        full.a = 1;

        // Initializing colors for the biome texts.
        Color plainsColor = new Color(0.4470588f, 0.7176471f, 0.2862745f, 1.0f); //hexadecimal is #72B749, green
        Color cityColor = new Color(0.7176471f, 0.2862745f, 0.3254902f, 1.0f); //hexadecimal is #B74953, copper-redish
        Color caveColor = new Color(0.3686275f, 0.1254902f, 0.5568628f, 1.0f); //hexadecimal is #5E208E, dark purple

        // Initializing floats that dictate the starting R and B values in the RGB format for the oxygen level texts.
        float startingOxygenTextColor = 0.6320754f;
        float endingOxygenTextColor = 0.0f;

        // Store references to each LevelData in a dictionary
        saveFileYraSprites[saveIndex - 1].sprite = currentYraSprite;
        saveFileYraSprites[saveIndex - 1].color = full;

        saveFileBiomeTexts[saveIndex - 1].text = String.Format("{0} Level {1}", LevelManager.currentBiome, LevelManager.currentLevelID);
        if (LevelManager.currentBiome.Equals("plains")){
            saveFileBiomeTexts[saveIndex - 1].color = plainsColor;
        } else if (LevelManager.currentBiome.Equals("city")){
            saveFileBiomeTexts[saveIndex - 1].color = cityColor;
        } else if (LevelManager.currentBiome.Equals("cave")){
            saveFileBiomeTexts[saveIndex - 1].color = caveColor;
        }

        float oxygenTextColor = startingOxygenTextColor - ((float)LevelManager.levelSOsStatic[LevelManager.currentLevelID].oxygenLevel / LevelManager.levelSOsStatic[LevelManager.currentLevelID].secondTargetOxygenLevel) * (startingOxygenTextColor - endingOxygenTextColor);
        saveFileOxygenLevelTexts[saveIndex - 1].text = String.Format($"Oxygen: {LevelManager.levelSOsStatic[LevelManager.currentLevelID].oxygenLevel}/{LevelManager.levelSOsStatic[LevelManager.currentLevelID].firstTargetOxygenLevel}");
        saveFileOxygenLevelTexts[saveIndex - 1].color = new Color(oxygenTextColor, 0.6320754f, oxygenTextColor, 1.0f); // color changes continuously from gray to green depending on how much oxygen you have
    }

    public void deleteSaveFileUI(int saveIndex){
        Color empty = saveFileYraSprites[saveIndex - 1].color;
        empty.a = 0;
        saveFileYraSprites[saveIndex - 1].sprite = null;
        saveFileYraSprites[saveIndex - 1].color = empty;
        saveFileBiomeTexts[saveIndex - 1].text = "";
        saveFileOxygenLevelTexts[saveIndex - 1].text = "";
    }

    // Loads in all save file UIs in the savePanel of the Settings menu
    public void LoadSaveFileUIs(){
        SaveData currSaveData;

        // Initializing colors with alpha = 1, and alpha = 0, respectively.
        Color full = saveFileYraSprites[0].color;
        full.a = 1;
        Color empty = saveFileYraSprites[0].color;
        empty.a = 0;

        // Initializing colors for the biome texts.
        Color plainsColor = new Color(0.4470588f, 0.7176471f, 0.2862745f, 1.0f); //hexadecimal is #72B749, green
        Color cityColor = new Color(0.7176471f, 0.2862745f, 0.3254902f, 1.0f); //hexadecimal is #B74953, copper-redish
        Color caveColor = new Color(0.3686275f, 0.1254902f, 0.5568628f, 1.0f); //hexadecimal is #5E208E, dark purple

        // Initializing floats that dictate the starting R and B values in the RGB format for the oxygen level texts.
        float startingOxygenTextColor = 0.6320754f;
        float endingOxygenTextColor = 0.0f;

        for (int i = 1; i < 4; i++){
            currSaveData = null;

            DataManager.readFile(ref currSaveData, i);
            // Load in level datas for easier reference, since level id != elem index.
            if(currSaveData != null) // Triggers if save file exists for that save index.
            {
                // Store references to each LevelData in a dictionary. Level id is not necessarily the same as list index in currSaveData.levelDatas, so we store references to each LevelData in a dictionary. If you need level data, get it from currLevelDatas, not from currSaveData.levelDatas - note that any changes to currLevelDatas entries will be reflected in currSaveData because the dictionary contains references, not copies, BUT if you want to add a new LevelData object to the list, you MUST use AddLevelData.
                Dictionary<int, LevelData> currLevelDatas = currSaveData.levelDatas.ToDictionary(keySelector: ld => ld.levelID, elementSelector: ld => ld);
                LevelData currLevel = currLevelDatas[currSaveData.currLevelIndex];
                saveFileYraSprites[i - 1].sprite = currentYraSprite;
                saveFileYraSprites[i - 1].color = full;

                saveFileBiomeTexts[i - 1].text = String.Format("{0} Level {1}", currLevel.biome, currLevel.levelID);
                if (currLevel.biome.Equals("plains")){
                    saveFileBiomeTexts[i - 1].color = plainsColor;
                } else if (currLevel.biome.Equals("city")){
                    saveFileBiomeTexts[i - 1].color = cityColor;
                } else if (currLevel.biome.Equals("cave")){
                    saveFileBiomeTexts[i - 1].color = caveColor;
                }

                float oxygenTextColor = startingOxygenTextColor - ((float)currLevel.oxygenLevel / currLevel.secondTargetOxygenLevel) * (startingOxygenTextColor - endingOxygenTextColor);
                saveFileOxygenLevelTexts[i - 1].text = String.Format($"Oxygen: {LevelManager.levelSOsStatic[LevelManager.currentLevelID].oxygenLevel}/{LevelManager.levelSOsStatic[LevelManager.currentLevelID].firstTargetOxygenLevel}");
                saveFileOxygenLevelTexts[i - 1].color = new Color(oxygenTextColor, 0.6320754f, oxygenTextColor, 1.0f); // color changes continuously from gray to green depending on how much oxygen you have
            }
            else // Triggers if save file does not yet exist for that save index (i.e. the player hasn't written a save file to that index yet)
            {
                saveFileYraSprites[i - 1].sprite = null;
                saveFileYraSprites[i - 1].color = empty;
                saveFileBiomeTexts[i - 1].text = "";
                saveFileOxygenLevelTexts[i - 1].text = "";
            }
        }
    }

    public void LoadSaveFileUIsForTitleScreen(){
        SaveData currSaveData;

        // Initializing colors with alpha = 1, and alpha = 0, respectively.
        Color full = saveFileYraSprites[0].color;
        full.a = 1;
        Color empty = saveFileYraSprites[0].color;
        empty.a = 0;

        // Initializing colors for the biome texts.
        Color plainsColor = new Color(0.4470588f, 0.7176471f, 0.2862745f, 1.0f); //hexadecimal is #72B749, green
        Color cityColor = new Color(0.7176471f, 0.2862745f, 0.3254902f, 1.0f); //hexadecimal is #B74953, copper-redish
        Color caveColor = new Color(0.3686275f, 0.1254902f, 0.5568628f, 1.0f); //hexadecimal is #5E208E, dark purple

        // Initializing floats that dictate the starting R and B values in the RGB format for the oxygen level texts.
        float startingOxygenTextColor = 0.6320754f;
        float endingOxygenTextColor = 0.0f;

        for (int i = 0; i < 4; i++){
            currSaveData = null;

            DataManager.readFile(ref currSaveData, i);
            // Load in level datas for easier reference, since level id != elem index.
            if(currSaveData != null) // Triggers if save file exists for that save index.
            {
                // Store references to each LevelData in a dictionary. Level id is not necessarily the same as list index in currSaveData.levelDatas, so we store references to each LevelData in a dictionary. If you need level data, get it from currLevelDatas, not from currSaveData.levelDatas - note that any changes to currLevelDatas entries will be reflected in currSaveData because the dictionary contains references, not copies, BUT if you want to add a new LevelData object to the list, you MUST use AddLevelData.
                Dictionary<int, LevelData> currLevelDatas = currSaveData.levelDatas.ToDictionary(keySelector: ld => ld.levelID, elementSelector: ld => ld);
                LevelData currLevel = currLevelDatas[currSaveData.currLevelIndex];
                saveFileYraSprites[i].sprite = currentYraSprite;
                saveFileYraSprites[i].color = full;

                saveFileBiomeTexts[i].text = String.Format("{0} Level {1}", currLevel.biome, currLevel.levelID);
                if (currLevel.biome.Equals("plains")){
                    saveFileBiomeTexts[i].color = plainsColor;
                } else if (currLevel.biome.Equals("city")){
                    saveFileBiomeTexts[i].color = cityColor;
                } else if (currLevel.biome.Equals("cave")){
                    saveFileBiomeTexts[i].color = caveColor;
                }

                float oxygenTextColor = startingOxygenTextColor - ((float)currLevel.oxygenLevel / currLevel.secondTargetOxygenLevel) * (startingOxygenTextColor - endingOxygenTextColor);
                saveFileOxygenLevelTexts[i].text = String.Format($"Oxygen: {LevelManager.levelSOsStatic[LevelManager.currentLevelID].oxygenLevel}/{LevelManager.levelSOsStatic[LevelManager.currentLevelID].firstTargetOxygenLevel}");
                saveFileOxygenLevelTexts[i].color = new Color(oxygenTextColor, 0.6320754f, oxygenTextColor, 1.0f); // color changes continuously from gray to green depending on how much oxygen you have
            }
            else // Triggers if save file does not yet exist for that save index (i.e. the player hasn't written a save file to that index yet)
            {
                saveFileYraSprites[i].sprite = null;
                saveFileYraSprites[i].color = empty;
                saveFileBiomeTexts[i].text = "";
                saveFileOxygenLevelTexts[i].text = "";
            }
        }
    }
}
