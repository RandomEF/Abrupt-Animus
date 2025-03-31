using UnityEngine;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

public class CompareSaves : IComparer<string>
{
    public int Compare(string s1, string s2){
        return int.Parse(Regex.Match(Path.GetFileNameWithoutExtension(s1).ToString(), @"\d+$").ToString()) > int.Parse(Regex.Match(Path.GetFileNameWithoutExtension(s2).ToString(), @"\d+$").ToString()) ? 1 : -1; // if the number on the first string is larger than the second, return 1, else return -1
    }
}
public static class SaveSystem
{
    public static void Save(SaveData data, string type, int saveSlot)
    {
        type = new CultureInfo("en-US", false).TextInfo.ToTitleCase(type); // Make sure the type is following the naming convention
        string path = Path.Combine(Application.persistentDataPath, "Saves", "Slot" + saveSlot.ToString(), type); // Make the path to the folder
        string fileMatch = "Slot" + saveSlot.ToString() + type + "*.dat"; // Get the pattern for the important files in the folder
        string[] matches = Directory.GetFiles(path, fileMatch); // Get all files that match the pattern
        Array.Sort(matches, new CompareSaves()); // Sort the files based on the last number
        int fileNum = 1; // Default value if no files are found
        if (matches.Length > 0)
        {
            string lastFile = matches[^1]; // Get the latest file in the directory
            lastFile = Path.GetFileNameWithoutExtension(lastFile).ToString(); // Get only the name of the file
            fileNum = Int32.Parse(Regex.Match(lastFile, @"\d+$").ToString()) + 1; // Get the number of the file and increment it
        }
        path = Path.Combine(path, "Slot" + saveSlot.ToString() + type + fileNum.ToString() + ".dat"); // Make the name of the new file

        using (StreamWriter stream = new StreamWriter(path)) // Make the file on the system
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented); // Convert the save data into JSON
            stream.Write(json); // Fill with constructed save data
        }
        // Modify Lookup
        Dictionary<string, string> lookup = GetLookup(); // Read the lookup
        lookup[$"Slot{saveSlot}"] = path; // Edit this slot's new path
        List<string> keys = new List<string>(lookup.Keys); // Get all the keys
        List<string> values = new List<string>(lookup.Values); // Get all the values

        using (StreamWriter lookupWriter = new StreamWriter(Path.Combine(Application.persistentDataPath, "SaveLookup.txt")))
        { // Write over SaveLookup.txt
            for (int i = 0; i < keys.Count; i++) // For each key in the dictionary
            {
                lookupWriter.WriteLine(keys[i] + "," + values[i]); // Write each line with a comma as the separator
            }
        }
    }
    public static SaveData LoadSave(string path)
    {
        if (!File.Exists(path)){ // If the file doesn't exist
            Debug.Log($"File at '{path}' does not exist.");
        }
        SaveData data; // Define the save data
        using (StreamReader sr = new StreamReader(path))
        { // Open the file for reading
            data = JsonConvert.DeserializeObject<SaveData>(sr.ReadToEnd()); // Convert the file back into a SaveData class
        }

        return data; // Return the read data
    }
    public static List<SaveSlot> ListAllSlots()
    {
        Dictionary<string, string> lookupPairs = GetLookup(); // Get the slot pairs
        if (lookupPairs == null) // If there are no saves made
        {
            return new List<SaveSlot>(); // Return an empty list
        }
        string path = Path.Combine(Application.persistentDataPath, "Saves"); // Path to the saves folder
        if (!Directory.Exists(path)) // If no saves have been made
        {
            Directory.CreateDirectory(path); // Create the folder
            return new List<SaveSlot>(); // Return an empty list
        }
        string[] directories = Directory.GetDirectories(path); // Get all the folders within the Saves folder
        List<SaveSlot> slotList = new List<SaveSlot>(); // Create a new
        foreach (string directory in directories)
        { // For every save slot
            string folder = directory.Substring(path.Length).TrimStart(Path.DirectorySeparatorChar); // Get the folder name
            string savePath = lookupPairs[folder]; // Get the filepath
            SaveSlot slot = new SaveSlot(savePath); // Create a new container
            slotList.Add(slot); // Add it to the list
        }
        return slotList; // Return the SaveSlot list
    }
    public static Dictionary<string, string> GetLookup()
    {
        if (!File.Exists(Path.Combine(Application.persistentDataPath, "SaveLookup.txt")))
        { // If the file doesn't exist
            File.Create(Path.Combine(Application.persistentDataPath, "SaveLookup.txt")).Close(); // Create it and close it
        }

        string[] pathLookup = File.ReadAllLines(Path.Combine(Application.persistentDataPath, "SaveLookup.txt")); // Read every line and store as an array
        Dictionary<string, string> lookupPairs = new Dictionary<string, string>(); // Make a new dictionary
        foreach (string lookup in pathLookup)
        { // For every line in the file
            string[] pairArray = lookup.Split(','); // Split it into pairs
            lookupPairs.Add(pairArray[0], pairArray[1]); // Add each pair into the dictionary
        }
        return lookupPairs; // Return the dictionary
    }
    public static SaveData NewSave()
    {
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Saves"))) // If Saves doesn't exist
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Saves")); // Create it
        }
        string[] dirs = Directory.GetDirectories(Path.Combine(Application.persistentDataPath, "Saves")); // Get all Save slots
        Array.Sort(dirs, new CompareSaves()); // Sort the slots using the same system
        int dirNum = 1;
        if (dirs.Length > 0)
        {
            string lastDir = dirs[^1]; // Get the last directory
            dirNum = Int32.Parse(Regex.Match(lastDir, @"\d+$").ToString()) + 1; // Get the number of the last slot
        }
        // Create the folders for the save slot
        string folderName = "Slot" + dirNum.ToString(); // Create the folder name
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Saves", folderName))) // If the folder doesn't exist
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Saves", folderName)); // Create the folder
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Saves", folderName, "Manual")); // Create the Manual folder inside
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Saves", folderName, "Auto")); // Create the Auto folder inside
        }
        else
        {
            throw new Exception($"The save folder {dirNum} was unable to be created.");
        }
        // Create the save file
        SaveData data = new SaveData(); // Create new save data
        string path = Path.Combine(Application.persistentDataPath, "Saves", folderName, "Manual", "Slot" + dirNum.ToString() + "Manual1.dat"); // Get the path to the file
        using (StreamWriter saveWriter = new StreamWriter(path)) // Make the file on the system
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented); // Convert the save data into JSON
            saveWriter.Write(json); // Fill with constructed save data
        }
        // Modify the lookup
        if (!File.Exists(Path.Combine(Application.persistentDataPath, "SaveLookup.txt"))) // If the lookup doesn't exist
        {
            File.Create(Path.Combine(Application.persistentDataPath, "SaveLookup.txt")).Close(); // Make it and then close it
        }
        File.AppendAllText(Path.Combine(Application.persistentDataPath, "SaveLookup.txt"), $"{folderName},{path}\n"); // Append the necessary pair
        return data; // Return the save data
    }
}

public class SaveSlot
{
    public string dataPath; // Exists so that SaveData only has 1 instance open at anytime for memory
    public string number;
    public string sceneName;
    public float time;
    public SaveSlot(string dataPath)
    {
        this.dataPath = dataPath; // Stores the path to the save on the disk
        number = Regex.Match(Path.GetFileName(dataPath), @"(?<=Slot)\d+").ToString(); // Gets the ending number from every slot
        SaveData data = SaveSystem.LoadSave(dataPath); // Opens the save file to read values
        sceneName = data.playerData.currentScene; // Gets the scene the save was in
        time = data.playerData.timeOnSave; // Gets the amount of time the player has spent in the save
    }
}
