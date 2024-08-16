using UnityEngine;
using System.Data;
using System.Collections.Generic;
using System.IO;

public class DialogueInteraction : MonoBehaviour
{
    public GameObject gameManager;
    private DialogueDatabaseManager ddm;
    public GameObject dialogueManager;
    private IDbConnection dialogueConnection;
    private IDbConnection actorConnection;
    private List<(int, string)> actorsInInteraction = new List<(int, string)>();
    public Object dialogueFile;
    private string filepath;
    [SerializeField] private List<(int, string)> dialogueSections = new List<(int, string)>();
    [SerializeField] private int nextLine = 1;
    void Start()
    {
        ddm = gameManager.GetComponent<DialogueDatabaseManager>();
        filepath = Path.Combine(Application.streamingAssetsPath, "Dialogue", dialogueFile.name + ".dlg");
        PreprocessSections();
        string line = RequestNextLine("english");
        actorConnection = ddm.OpenActorDb();
        actorConnection.Close();
    }
    private void PreprocessSections(){
        StreamReader reader = new StreamReader(filepath);
        int lineNumber = 1;
        while(!reader.EndOfStream){
            string line = reader.ReadLine();
            if(line.StartsWith("[") && line.EndsWith("]")){
                dialogueSections.Add((lineNumber, line[1..^1]));
            }
            lineNumber++;
        }
    }
    private string RequestNextLine(string language){
        dialogueConnection = ddm.OpenDialogueDb();
        IDbCommand lineRequest = dialogueConnection.CreateCommand();
        lineRequest.CommandText = $"SELECT {language} FROM Dialogue WHERE id=" + nextLine;
        IDataReader response = lineRequest.ExecuteReader();
        response.Read();
        string line = response.GetString(0);
        dialogueConnection.Close();
        return line;
    }
    private string ConvertLine(string line){
        string copy = line;
        bool firstItalics = true;
        bool firstBold = true;
        for(int i = 0; i < line.Length; i++){
            if (line[i] == '\\'){
                i++;
            } else if (line[i] == '<') {
                string hexColour = line.Substring(++i, 7);
                copy += "<color=" + hexColour + ">";
                i += 6;
            } else if (line[i] == '>'){
                copy += "</color>";
            } else if (line[i] == '_'){
                if (firstItalics){
                    copy += "<i>";
                    firstItalics = false;
                } else {
                    copy += "</i>";
                    firstItalics = true;
                }
            } else if (line[i] == '*'){
                if (firstBold){
                    copy += "<b>";
                    firstBold = false;
                } else {
                    copy += "</b>";
                    firstBold = true;
                }
            } else {
                copy += line[i];
            }
        }
        return copy;
    }
}
