using UnityEngine;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

public static class SaveSystem
{
    public static void Save(SaveData data, string type, int saveSlot)
    {
        type = new CultureInfo("en-US", false).TextInfo.ToTitleCase(type);
        string path = Path.Combine(Application.persistentDataPath, "Saves", "Slot" + saveSlot.ToString(), type);
        string fileMatch = "Slot" + saveSlot.ToString() + type + "*.dat";
        string[] matches = Directory.GetFiles(path, fileMatch);
        Array.Sort(matches);
        int fileNum = 1;
        if (matches.Length > 0)
        {
            string lastFile = matches[^1];
            lastFile = Path.GetFileNameWithoutExtension(lastFile).ToString();
            fileNum = Int32.Parse(Regex.Match(lastFile, @"\d+$").ToString()) + 1;
        }
        path = Path.Combine(path, "Slot" + saveSlot.ToString() + type + fileNum.ToString() + ".dat");

        using (StreamWriter stream = new StreamWriter(path))
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            stream.Write(json); // Fill with constructed save data
        }
        // Modify Lookup
        Dictionary<string, string> lookup = GetLookup();
        lookup[$"Slot{saveSlot}"] = path;
        List<string> keys = new List<string>(lookup.Keys);
        List<string> values = new List<string>(lookup.Values);

        using (StreamWriter lookupWriter = new StreamWriter(Path.Combine(Application.persistentDataPath, "SaveLookup.txt")))
        {
            for (int i = 0; i < keys.Count; i++)
            {
                lookupWriter.WriteLine(keys[i] + "," + values[i]);
            }
        }
    }
    public static SaveData LoadSave(string path)
    {
        SaveData data;
        using (StreamReader sr = new StreamReader(path))
        {
            data = JsonConvert.DeserializeObject<SaveData>(sr.ReadToEnd());
        }

        return data;
    }
    public static List<SaveSlot> ListAllSlots()
    {
        Dictionary<string, string> lookupPairs = GetLookup();
        if (lookupPairs == null)
        {
            return new List<SaveSlot>();
        }
        string path = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return new List<SaveSlot>();
        }
        string[] directories = Directory.GetDirectories(path);
        List<SaveSlot> slotList = new List<SaveSlot>();
        foreach (string directory in directories)
        {
            string folder = directory.Substring(path.Length).TrimStart(Path.DirectorySeparatorChar);
            string savePath = lookupPairs[folder];
            SaveSlot slot = new SaveSlot(savePath);
            slotList.Add(slot);
        }
        return slotList;
    }
    public static Dictionary<string, string> GetLookup()
    {
        if (!File.Exists(Path.Combine(Application.persistentDataPath, "SaveLookup.txt")))
        {
            File.Create(Path.Combine(Application.persistentDataPath, "SaveLookup.txt")).Close();
        }

        string[] pathLookup = File.ReadAllLines(Path.Combine(Application.persistentDataPath, "SaveLookup.txt"));
        Dictionary<string, string> lookupPairs = new Dictionary<string, string>();
        foreach (string lookup in pathLookup)
        {
            string[] pairArray = lookup.Split(',');
            lookupPairs.Add(pairArray[0], pairArray[1]);
        }
        return lookupPairs;
    }
    public static SaveData NewSave()
    {

        SaveData data = new SaveData();
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Saves")))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Saves"));
        }
        string[] dirs = Directory.GetDirectories(Path.Combine(Application.persistentDataPath, "Saves"));
        Array.Sort(dirs, (s1, s2) =>
            Int32.Parse(Regex.Match(s1, @"\d+$").ToString()).CompareTo(Int32.Parse(Regex.Match(s2, @"\d+$").ToString()))
        );
        int dirNum = 1;
        if (dirs.Length > 0)
        {
            string lastDir = dirs[^1];
            dirNum = Int32.Parse(Regex.Match(lastDir, @"\d+$").ToString()) + 1;
        }
        // Create the folders for the save slot
        string folderName = "Slot" + dirNum.ToString();
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Saves", folderName)))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Saves", folderName));
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Saves", folderName, "Manual"));
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Saves", folderName, "Auto"));
        }
        else
        {
            throw new Exception($"The save folder {dirNum} was unable to be created.");
        }
        // Create the save file
        string path = Path.Combine(Application.persistentDataPath, "Saves", folderName, "Manual", "Slot" + dirNum.ToString() + "Manual1.dat");
        using (StreamWriter saveWriter = new StreamWriter(path))
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            saveWriter.Write(json);
        }
        // Modify the lookup
        if (!File.Exists(Path.Combine(Application.persistentDataPath, "SaveLookup.txt")))
        {
            File.Create(Path.Combine(Application.persistentDataPath, "SaveLookup.txt")).Close();
        }
        File.AppendAllText(Path.Combine(Application.persistentDataPath, "SaveLookup.txt"), $"{folderName},{path}\n");
        return data;
    }
}

public class SaveSlot
{
    public string dataPath; // Exists so that SaveData only has 1 instance open at anytime for memory
    public string number;
    public string sceneName = "Empty";
    public float time = 0;
    public SaveSlot(string dataPath)
    {
        this.dataPath = dataPath;
        number = Regex.Match(Path.GetFileName(dataPath), @"(?<=Slot)\d+").ToString(); // Gets the ending number from every slot
        SaveData data = SaveSystem.LoadSave(dataPath);
        sceneName = data.playerData.currentScene;
        time = data.playerData.timeOnSave;
    }
}
