using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    // Test: does c# pass by value or pass by reference for a class instance?
    // Test result: pass by value. So I need to pass by reference manually.
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

            // Populate the scene accordingly in PD
        }
    }

    public static void writeFile(ref SaveData saveData, int saveIndex)
    {
        // Serialize the object into JSON and save string.
        string jsonString = JsonUtility.ToJson(saveData);

        // Generate the pathway to store the data
        string saveFile = getSaveFilePath(saveIndex);

        // Write JSON to file.
        File.WriteAllText(saveFile, jsonString);
    }

    // Create a field/pathway for the save file.
    // If saveIndex is 0 then it's autosave; otherwise it's a manual save. 
    private static string getSaveFilePath(int saveIndex)
    {
        return Application.persistentDataPath + "/save" + saveIndex + "data.json";
    }
}