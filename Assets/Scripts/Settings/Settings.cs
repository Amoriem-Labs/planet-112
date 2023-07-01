using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public bool fullScreen;
    public GameObject cam;
    public GameObject hotbarCanvas;
    public GameObject inventoryCanvas;
    private Vector3 settingsStartingScale;
    private Vector3 hotbarStartingScale;
    private Vector3 inventoryStartingScale;
    public GameObject quitPanel;
    public GameObject savePanel;
    private float[] uiScalings = new float[]{0.8f, 0.9f, 1.0f, 1.1f, 1.2f};
    public PersistentData persistentData;
    public float uiScale;

    void Start(){
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        fullScreen = true;
        settingsStartingScale = transform.localScale;
        hotbarStartingScale = hotbarCanvas.transform.localScale;
        inventoryStartingScale = inventoryCanvas.transform.localScale;
        GetComponentInChildren<Dropdown>().value = 2;
    }

    // This script is to update the position of SettingsCanvas to match camera position when settings are loaded.
    public void setPosition()
    {
        var pos = transform.position;
        pos.x = cam.transform.position.x;
        pos.y = cam.transform.position.y;
        transform.position = pos;
    }

    // TODO: need to test if this works.
    public void setFullScreen(){
        if( !fullScreen ) {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
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
        persistentData.CreateNewSave(1);
        
    }

    public void saveGame_2(){
        exitSaveScreen();
        persistentData.CreateNewSave(2);

    }

    public void saveGame_3(){
        exitSaveScreen();
        persistentData.CreateNewSave(3);

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
}
