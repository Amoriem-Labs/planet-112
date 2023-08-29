using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random=UnityEngine.Random;

public class TitleManager : MonoBehaviour
{
    public GameObject titleScreenSettingsCanvas;
    public GameObject gameScreenSettingsCanvas;
    public GameObject playCanvas;
    public GameObject askIfWantToDeleteSaveCanvas;
    public PersistentData persistentData;
    public int deleteSaveIndex;

    void Awake(){
        playCanvas.SetActive(false);
        askIfWantToDeleteSaveCanvas.SetActive(false);
        deleteSaveIndex = -1;
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
        titleScreenSettingsCanvas.GetComponent<Settings>().LoadSaveFileUIsForTitleScreen(); // This loads in the text UI for the save files in the savePanel of the Settings menu
    }

    public void PlayGame(){
        playCanvas.SetActive(true);
    }

    public void OpenSettings(){
        titleScreenSettingsCanvas.SetActive(true);
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

    public void deleteAutosave(){
        askIfWantToDeleteSaveCanvas.SetActive(true);
        deleteSaveIndex = 0;
    }

    public void deleteFile1(){
        askIfWantToDeleteSaveCanvas.SetActive(true);
        deleteSaveIndex = 1;
    }

    public void deleteFile2(){
        askIfWantToDeleteSaveCanvas.SetActive(true);
        deleteSaveIndex = 2;
    }

    public void deleteFile3(){
        askIfWantToDeleteSaveCanvas.SetActive(true);
        deleteSaveIndex = 3;
    }

    public void confirmDeleteFile(){
        if (deleteSaveIndex == -1){
            Debug.Log("Did not select a save file to delete.");
        } else {
            DataManager.deleteSaveFile(deleteSaveIndex);
            titleScreenSettingsCanvas.GetComponent<Settings>().LoadSaveFileUIsForTitleScreen();
            gameScreenSettingsCanvas.GetComponent<Settings>().LoadSaveFileUIs();
        }
    }

    public void closeAskIfWantToDeleteSaveCanvas(){
        askIfWantToDeleteSaveCanvas.SetActive(false);
        deleteSaveIndex = -1;
    }
}
