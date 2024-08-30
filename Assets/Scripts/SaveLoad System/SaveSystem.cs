using UnityEngine;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System;
using Newtonsoft.Json;

public static class SaveSystem
{
    public static void Save(SaveData data, string type, int saveSlot){
        type = new CultureInfo("en-US", false).TextInfo.ToTitleCase(type);
        string path = Path.Combine(Application.persistentDataPath, "Slot" + saveSlot.ToString(), type);
        string fileMatch = "Slot" + saveSlot.ToString() + type + "*.dat";
        string[] matches =Directory.GetFiles(path, fileMatch);
        Array.Sort(matches);
        int fileNum = 1;
        if (matches.Length > 0){
            string lastFile = matches[^1];
            lastFile = Path.GetFileNameWithoutExtension(lastFile).ToString();
            fileNum = Int32.Parse(Regex.Match(lastFile, @"\d+$").ToString()) + 1;
        }
        path = Path.Combine(path, "Slot" + saveSlot.ToString() + type + fileNum.ToString() + ".dat");

        using (StreamWriter stream = new StreamWriter(path)){
            string json = JsonConvert.SerializeObject(data);
            stream.Write(json); // Fill with constructed save data
        }
    }
    public static SaveData Load(string path){
        SaveData data;
        using (StreamReader sr = new StreamReader(path)){
            data = (SaveData)JsonConvert.DeserializeObject(sr.ReadToEnd());
        }
        return data;
    }
}
