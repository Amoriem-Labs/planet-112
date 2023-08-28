using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random=UnityEngine.Random;

public class TitleManager : MonoBehaviour
{
    public GameObject settingsCanvas;
    public GameObject playCanvas;
    public PersistentData persistentData;

    void Awake(){
        playCanvas.SetActive(false);
    }

    void Start(){
        string[] soundtrackNames = {
            "plainsSoundtrack",
            "NievenResolveSoundtrack",
            "ReadingSierraJournalSoundtrack",
            "ThePrincessPastSoundtrack",
            "RevelationSoundtrack"
        };
        int soundtrackIndex = Random.Range(0,5);
        AudioManager.GetSoundtrack(soundtrackNames[soundtrackIndex]).Play();
        settingsCanvas.GetComponent<Settings>().LoadSaveFileUIsForTitleScreen(); // This loads in the text UI for the save files in the savePanel of the Settings menu
    }

    public void PlayGame(){
        playCanvas.SetActive(true);
    }

    public void OpenSettings(){
        settingsCanvas.SetActive(true);
    }

    public void QuitGame(){
        Application.Quit();
    }

    public void playAutosave(){
        persistentData.LoadSave(0);
    }

    public void playFile1(){
        persistentData.LoadSave(1);
    }

    public void playFile2(){
        persistentData.LoadSave(2);
    }

    public void playFile3(){
        persistentData.LoadSave(3);
    }

    public void ClosePlayCanvas(){
        playCanvas.SetActive(false);
    }
}
