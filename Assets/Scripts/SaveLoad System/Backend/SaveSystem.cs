using UnityEngine;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

public static class SaveSystem
{
    public static void Save(SaveData data, string type, int saveSlot){
        type = new CultureInfo("en-US", false).TextInfo.ToTitleCase(type);
        string path = Path.Combine(Application.persistentDataPath, "Saves", "Slot" + saveSlot.ToString(), type);
        string fileMatch = "Slot" + saveSlot.ToString() + type + "*.dat";
        string[] matches = Directory.GetFiles(path, fileMatch);
        Array.Sort(matches);
        int fileNum = 1;
        if (matches.Length > 0){
            string lastFile = matches[^1];
            lastFile = Path.GetFileNameWithoutExtension(lastFile).ToString();
            fileNum = Int32.Parse(Regex.Match(lastFile, @"\d+$").ToString()) + 1;
        }
        path = Path.Combine(path, "Slot" + saveSlot.ToString() + type + fileNum.ToString() + ".dat");

        using (StreamWriter stream = new StreamWriter(path)){
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            stream.Write(json); // Fill with constructed save data
        }
    }
    public static SaveData LoadSave(string path){
        SaveData data;
        using (StreamReader sr = new StreamReader(path)){
            data = (SaveData)JsonConvert.DeserializeObject(sr.ReadToEnd());
        }
        return data;
    }
    public static List<SaveSlot> ListAllSlots(){
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "SaveLookup.txt"))){
            File.Create(Path.Combine(Application.persistentDataPath, "SaveLookup.txt")).Close();
            return new List<SaveSlot>();
        }

        string[] pathLookup = File.ReadAllLines(Path.Combine(Application.persistentDataPath, "SaveLookup.txt"));
        Dictionary<string, string> lookupPairs = new Dictionary<string, string>();
        foreach (string lookup in pathLookup){
            string[] pairArray = lookup.Split(',');
            lookupPairs.Add(pairArray[0], pairArray[1]);
        } 
        string path = Path.Combine(Application.persistentDataPath, "Saves");
        string[] directories = Directory.GetDirectories(path);
        List<SaveSlot> slotList = new List<SaveSlot>();
        foreach (string directory in directories){
            string folder = directory.Substring(path.Length).TrimStart(Path.DirectorySeparatorChar);
            string savePath = lookupPairs[folder];
            SaveSlot slot = new SaveSlot(savePath);
            slotList.Add(slot);
        }
        return slotList;
    }
    public static SaveData NewSave(){
        
        SaveData data = new SaveData();
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Saves"))){
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Saves"));
        }
        string[] dirs = Directory.GetDirectories(Path.Combine(Application.persistentDataPath, "Saves"));
        Array.Sort(dirs);
        int dirNum = 1;
        if (dirs.Length > 0){
            string lastDir = dirs[^1];
            dirNum = Int32.Parse(Regex.Match(lastDir, @"\d+$").ToString()) + 1;
        }
        // Create the folders for the save slot
        string folderName = "Slot" + dirNum.ToString();
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Saves", folderName))){
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Saves", folderName));
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Saves", folderName, "Manual"));
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Saves", folderName, "Auto"));
        } else {
            throw new Exception($"The save folder {dirNum} was unable to be created.");
        }
        // Create the save file
        string path = Path.Combine(Application.persistentDataPath, "Saves", folderName, "Manual", "Slot" + dirNum.ToString() + "Manual1.dat");
        using (StreamWriter saveWriter = new StreamWriter(path)){
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            saveWriter.Write(json);
        }
        // Modify the lookup
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "SaveLookup.txt"))){
            File.Create(Path.Combine(Application.persistentDataPath, "SaveLookup.txt")).Close();
        }
        File.AppendAllText(Path.Combine(Application.persistentDataPath, "SaveLookup.txt"), $"{folderName},{path}\n");
        // using (StreamWriter saveLookup = File.AppendAllText(Path.Combine(Application.persistentDataPath, "SaveLookup.txt"))){
        //     string pair = folderName + "," + path;
        //     saveLookup.WriteLine(pair);
        // }
        return data;
    }
}

public class SaveSlot {
    public string dataPath;
    public string number;
    public string sceneName = "Empty";
    public float time = 0;
    public SaveSlot(string dataPath){
        this.dataPath = dataPath;
        number = Regex.Match(Path.GetFileName(dataPath), @"(?<=Slot)\d+").ToString();
        SaveData data = SaveSystem.LoadSave(dataPath);
        sceneName = data.playerData.currentScene;
        time = data.playerData.timeOnSave;
    }
}
