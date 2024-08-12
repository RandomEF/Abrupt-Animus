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
    public TextAsset dialogueFile;
    [SerializeField] private List<(int, string)> dialogueSections = new List<(int, string)>();
    [SerializeField] private int nextLine = 1;
    void Start()
    {
        ddm = gameManager.GetComponent<DialogueDatabaseManager>();
        //PreprocessSections();

        dialogueConnection = ddm.OpenDialogueDb();
        actorConnection = ddm.OpenActorDb();
        IDbCommand lineRequest = dialogueConnection.CreateCommand();
        lineRequest.CommandText = "SELECT text FROM Dialogue WHERE id=" + nextLine;
        IDataReader response = lineRequest.ExecuteReader();
        response.Read();
        Debug.Log(response.GetString(0));
        
        dialogueConnection.Close();
        actorConnection.Close();
    }/*
    private void PreprocessSections(){
        string[] fileLines = dialogueFile.text.Split('\n');
        for (int i = 0; i < fileLines.Length; i++){
            string line = fileLines[i];
            if (line.StartsWith("[") && line.EndsWith("]")){
                dialogueSections.Add((i, line.Substring(1, line.Length - 2)));
            }
        }
    }*/
}
