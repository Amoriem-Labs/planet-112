using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static int numSaveFiles = 4;

    // `ref` keyword to make sure saveData is passed by reference
    public static void readFile(ref SaveData saveData, int saveIndex)
    {
        // Generate the pathway to get the data
        string saveFile = getSaveFilePath(saveIndex);

        // Does the file exist?
        if (File.Exists(saveFile))
        {
            // Read the entire file and save its contents.
            string fileContents = File.ReadAllText(saveFile);

            // Deserialize the JSON data into a pattern matching the SaveData class.
            saveData = JsonUtility.FromJson<SaveData>(fileContents);

            // TODO: Populate the scene accordingly in PD.
        }
    }

    public static void writeFile(ref SaveData saveData, int saveIndex)
    {
        // Serialize the object into JSON and save string.
        string jsonString = JsonUtility.ToJson(saveData);

        // Generate the pathway to store the data
        string saveFile = getSaveFilePath(saveIndex);

        print(saveFile);

        // Write JSON to file.
        File.WriteAllText(saveFile, jsonString);
    }

    // Writes any setting changes the user makes to ALL save files. Purpose of this is so that when player changes the background volume or other settings in one save file, they don't have to re-do their change whenever they load a different save file (which would otherwise be annoying).
    public static void writeSettingsChangeToFile(SettingsData settingsData)
    {
        for (int i = 0; i < numSaveFiles; i++){
            // Generate the pathway to get the data
            string saveFile = getSaveFilePath(i);

            // Does the file exist?
            if (File.Exists(saveFile))
            {
                // Dumps filetext from saveFile into string format
                string fileContents = File.ReadAllText(saveFile);

                // Converts filetext into SaveData format for easier modification
                SaveData tempSaveData = JsonUtility.FromJson<SaveData>(fileContents);

                // Change the settings in the saveFile
                tempSaveData.gameStateData.settingsData = new SettingsData();
                tempSaveData.gameStateData.settingsData.fullScreen = settingsData.fullScreen;
                tempSaveData.gameStateData.settingsData.volumeBGM = settingsData.volumeBGM;
                tempSaveData.gameStateData.settingsData.volumeSFX = settingsData.volumeSFX;
                tempSaveData.gameStateData.settingsData.uiScaleIndex = settingsData.uiScaleIndex;

                // Serialize the temporary object into new JSON and save string.
                string jsonString = JsonUtility.ToJson(tempSaveData);

                // Write JSON to file.
                File.WriteAllText(saveFile, jsonString);
            }
        }
    }

    // Get the filepath for this saveIndex
    private static string getSaveFilePath(int saveIndex)
    {
        // saveIndex 0 is autosave
        return Application.persistentDataPath + "/save" + saveIndex + "data.json";
    }
}