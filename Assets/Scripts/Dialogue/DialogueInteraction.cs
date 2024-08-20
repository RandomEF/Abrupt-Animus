using UnityEngine;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using Unity.VisualScripting;

public class DialogueInteraction : MonoBehaviour
{
    public GameObject gameManager;
    private DialogueDatabaseManager ddm;
    public GameObject dialogueManager;
    private List<(int, string)> actorsInInteraction = new List<(int, string)>();
    public UnityEngine.Object dialogueFile;
    private string filepath;
    [SerializeField] private List<(int, string)> dialogueSections = new List<(int, string)>();
    private StreamReader fileReader;
    void Start()
    {
        ddm = gameManager.GetComponent<DialogueDatabaseManager>();
        filepath = Path.Combine(Application.streamingAssetsPath, "Dialogue", dialogueFile.name + ".dlg");
        PreprocessSections();
        string line = RequestNext(true, "english", 1);
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
    private string RequestNext(bool isDialogue, string language, int lineID){
        IDbConnection connection = isDialogue ? ddm.OpenDialogueDb() : ddm.OpenActorDb();
        IDbCommand lineRequest = connection.CreateCommand();
        lineRequest.CommandText = $"SELECT {language} FROM {(isDialogue ? "Dialogue" : "Actor")} WHERE id={lineID}";
        IDataReader response = lineRequest.ExecuteReader();
        response.Read();
        string line = response.GetString(0);
        connection.Close();
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
    // DLG Interpreter
    private string[] operators = {"==", "<=", ">=", "!="};
    private int choice = 0;
    private int lineNumber = 0;
    /*
    -> is the goto operator
    [] is dialogue section
    #Ax is Actor database
    #Lx is Line database
    :: actor line/actor split
    @ is variable
    = is variable assign
    */
    public string[] RetrieveNextLine(){
        TreeNode tokenTree = Tokenizer();
        string[] lines = Analyzer(tokenTree);
        string[] convertedLines = new string[lines.Length];
        for(int i = 0; i < lines.Length; i++){
            convertedLines[i] = ConvertLine(lines[i]);
        }
        return convertedLines;
    }
    private TreeNode Tokenizer(){
        if (fileReader == null){
            fileReader = new StreamReader(filepath);
        }
        for (int i = 0; i < lineNumber; i++)
        {
            fileReader.ReadLine();
        }
        TreeNode fileTree = new TreeNode(); // This is here in case a choice exists, in which case multiple lines need to be read at once
        int lastLineNumber = lineNumber - 1;
        while (!fileReader.EndOfStream && lastLineNumber != lineNumber){
            string line = fileReader.ReadLine();
            lastLineNumber = lineNumber;
            if (line.Length < 3){
                throw new System.Exception($"The line '{line}' is missing information.");
            }
            StreamReader lineReader = new StreamReader(line);
            TreeNode lineTree = new TreeNode();
            fileTree.AddChild(lineTree);
            ReadNext(lineReader, lineTree);
            bool shouldStay = false;
            if (fileTree.GetChildren()[^1].GetNodeType() == "choice" && fileTree.GetChildren()[^1].value["end"] == "false"){
                shouldStay = true;
            }

            string nextLine = fileReader.ReadLine();

            if (nextLine[0] != '[' && nextLine != null){
                lineNumber++;
                if (!shouldStay){
                    break;
                }
            }
        }
        return fileTree;
    }
    // Tokenizer
    private void ReadNext(StreamReader line, TreeNode lineTree){
        while(!line.EndOfStream){    
            char token = ReadChar(line);
            if (token == '#'){
                ReadID(line, lineTree);
            } else if (token == '@'){
                ReadIf(line, lineTree);
            } else if (token == '-'){
                ReadJump(line, lineTree);
            } else if (token == '|'){
                ReadChoice(line, lineTree);
            } else {
                throw new System.Exception($"The starting character '{token}' was not recognised.");
            }
        }
    }
    private char ReadChar(StreamReader line){
        Int32 character = 32;
        while (character == 32){
            character = line.Read();
        }
        return character == -1 ? throw new System.Exception("Nothing left to read.") : (char)character;
    }
    private char PeekChar(StreamReader line){
        Int32 character = 32;
        while (character == 32){
            character = line.Peek();
        }
        return character == -1 ? throw new System.Exception("Nothing left to peek.") : (char)character;
    }
    private void ReadID(StreamReader line, TreeNode lineTree){
        char character = ReadChar(line);
        int id = ReadNumber(line);
        TreeNode idNode = new TreeNode();
        lineTree.AddChild(idNode);

        if (character == 'A'){
            idNode.AddType("actor");
        } else if (character == 'L'){
            idNode.AddType("line");
        } else{
            throw new Exception($"Unable to decide if this is an actor or line reference: {character}");
        }
        idNode.value.Add("id", id.ToString());
    }
    private int ReadNumber(StreamReader line){
        string num = "";
        while (!line.EndOfStream){
            char character = PeekChar(line);
            if (int.TryParse(character.ToString(), out _)){
                num += ReadChar(line);
            } else {
                break;
            }
        }
        int inted;
        int.TryParse(num, out inted);
        return inted;
    }
    private void ReadIf(StreamReader line, TreeNode lineTree){
        TreeNode ifNode = new TreeNode();
        lineTree.AddChild(ifNode);
        ifNode.AddType("if");
        ifNode.value.Add("var", ReadVariable(line));
        ifNode.value.Add("op", ReadOperator(line));
        char character = PeekChar(line);
        if (Char.IsNumber(character)){
            ifNode.value.Add("num", ReadNumber(line).ToString());
        } else if (Char.IsLetter(character) || character == '_'){
            ifNode.value.Add("var", ReadVariable(line));
        } else {
            throw new Exception("Nothing valid to compare to.");
        }
    }
    private string ReadVariable(StreamReader line){
        string variable = "";
        char character = PeekChar(line);
        if(Char.IsNumber(character)){
            throw new Exception("Variable can't start with a number!");
        }
        while (!line.EndOfStream){
            character = PeekChar(line);
            if (Char.IsLetterOrDigit(character) || character == '_'){
                variable += ReadChar(line);
            } else {
                break;
            }
        }
        return variable;
    }
    private string ReadOperator(StreamReader line){
        string op = "";
        char[] opChars = {'!', '>', '<', '='};
        while (!line.EndOfStream){
            char character = PeekChar(line);
            if(opChars.Contains(character)){
                op += character;
            }
            if (operators.Contains(op)){
                return op;
            }
        }
        throw new Exception($"No valid operator found: '{op}'");
    }
    private void ReadJump(StreamReader line, TreeNode lineTree){
        char character = ReadChar(line);
        if (character == '>'){
            string section = "";
            while (!line.EndOfStream){
                section += ReadChar(line);
            }
            TreeNode sectionNode = new TreeNode();
            lineTree.AddChild(sectionNode);
            sectionNode.AddType("jump");
            sectionNode.value.Add("section", section);
        } else {
            throw new Exception("This is not a jump, what were you trying to signify?");
        }
    }
    private void ReadChoice(StreamReader line, TreeNode lineTree){
        TreeNode choiceNode = new TreeNode();
        choiceNode.AddType("choice");
        lineTree.AddChild(choiceNode);
        ReadID(line, lineTree);
        if (!line.EndOfStream){
            char character = ReadChar(line);
            if (character == ','){
                choiceNode.value.Add("end", "false");
            } else {
                throw new Exception($"Unrecognised character '{character}' following choice line");
            }
        } else {
            choiceNode.value.Add("end", "true");
        }
    }
    // Analysis
    private string[] Analyzer(TreeNode fileTree){
        string[] lines = new string[fileTree.GetChildren().Count];
        for (int i = 0; i < fileTree.GetChildren().Count; i++)
        {
            TreeNode line = fileTree.GetChildren()[i];
            
        }
    }
}
