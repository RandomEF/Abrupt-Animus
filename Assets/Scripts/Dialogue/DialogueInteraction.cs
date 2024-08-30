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
    [SerializeField] private Dictionary<string, int> dialogueSections = new Dictionary<string, int>();
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
                dialogueSections.Add(line[1..^1], lineNumber);
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
    public (string, string, string[]) RetrieveNextLine(){
        while (true){
            TreeNode tokenTree = Tokenizer();
            (string, string, string[]) returnVals = Analyzer(tokenTree);
            string type = returnVals.Item1;
            if (type == "true"){
                continue;
            }

            if (type == "choice"){
                string[] lines = returnVals.Item3;
                string[] convertedLines = new string[lines.Length];
                for(int i = 0; i < lines.Length; i++){
                    convertedLines[i] = ConvertLine(lines[i]);
                }
                return (type, returnVals.Item2, convertedLines); // returnVals.Item2 will be blank as the actor is assumed to be the player, but this must be passed through to fulfill the return type conditions
            } else if (type == "facing"){

            } else if (type == "line"){
                string actor = returnVals.Item2;
                string line = ConvertLine(returnVals.Item3[0]);
            }
        }
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
            } else if (token == ':'){
                token = ReadChar(line);
                if (token ==  ':'){
                    ReadSeparator(line, lineTree);
                } else if (token == '-'){
                    ReadAnimation(line, lineTree);
                }
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
        ReadVariable(line, ifNode);
        ReadOperator(line, ifNode);
        ifNode.AddType("if");
        //ifNode.value.Add("op");
        char character = PeekChar(line);
        if (Char.IsNumber(character)){
            TreeNode numNode = new TreeNode();
            ifNode.AddChild(numNode);
            numNode.AddType("num");
            numNode.value.Add("num", ReadNumber(line).ToString());
        } else if (Char.IsLetter(character) || character == '_'){
            ReadVariable(line, ifNode);
        } else {
            throw new Exception("Nothing valid to compare to.");
        }
    }
    private void ReadVariable(StreamReader line, TreeNode lineTree){
        TreeNode variableNode = new TreeNode();
        lineTree.AddChild(variableNode);
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
        variableNode.AddType("var");
        variableNode.value.Add("var", variable); 
    }
    private string ReadOperator(StreamReader line, TreeNode lineTree){
        string op = "";
        char[] opChars = {'!', '>', '<', '='};
        //string[] operators = {"==", "<=", ">=", "!="};
        TreeNode opNode = new TreeNode();
        lineTree.AddChild(opNode);
        while (!line.EndOfStream){
            char character = PeekChar(line);
            if(op.Length == 0 && opChars.Contains(character)){
                op += ReadChar(line);
            } else if (op.Length == 1 && character == '='){
                op += ReadChar(line);
                opNode.AddType("comp");
                opNode.value.Add("op", op);
                return op;
            } else if (op.Length == 1 && op == "="){
                opNode.AddType("assign");
                return op;
            } else {
                throw new Exception($"Invalid operator '{op}'.");
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
        if (!line.EndOfStream){
            char character = ReadChar(line);
            if (character == ','){
                choiceNode.value.Add("end", "false");
            } else if (character == '#'){
                ReadID(line, lineTree);
            } else {
                throw new Exception($"Unrecognised character '{character}' following choice line");
            }
        } else {
            choiceNode.value.Add("end", "true");
        }
    }
    private void ReadSeparator(StreamReader line, TreeNode lineTree){
        char character = PeekChar(line);
        if (character == ':'){
            ReadChar(line);
            TreeNode separatorNode = new TreeNode();
            separatorNode.AddType("sep");
            lineTree.AddChild(separatorNode);
        }
    }
    private void ReadAnimation(StreamReader line, TreeNode lineTree){
        char character = PeekChar(line);
        if (character == '-'){
            ReadChar(line);
            TreeNode animNode = new TreeNode();
            lineTree.AddChild(animNode);
            animNode.AddType("anim");
            animNode.value.Add("num", ReadNumber(line).ToString());
        }
    }
    // Analysis
    private (string, string, string[]) Analyzer(TreeNode fileTree){
        string[] lines = new string[fileTree.GetChildren().Count];
        string returnType = "none";
        string[] returnLines = new string[lines.Length];
        for (int i = 0; i < fileTree.GetChildren().Count; i++)
        {
            TreeNode line = fileTree.GetChildren()[i];
            List<TreeNode> elements = line.GetChildren();
            if (elements[0].GetNodeType() == "actor"){
                if (elements[1].GetNodeType() == "anim"){
                    returnType = "animation";
                    string returnActor = RequestNext(false, "actor", Int32.Parse(elements[0].value["id"]));
                    string[] animID = {elements[1].value["num"]};
                    return (returnType, returnActor, animID);
                } else if (elements[1].GetNodeType() == "sep" && elements[2].GetNodeType() == "actor"){
                    returnType = "facing";
                    string returnActor1 = RequestNext(false, "actor", Int32.Parse(elements[0].value["id"]));
                    string[] returnActor2 = {RequestNext(false, "actor", Int32.Parse(elements[2].value["id"]))};
                    return (returnType, returnActor1, returnActor2);
                } else if (elements[1].GetNodeType() == "sep" && elements[2].GetNodeType() == "line"){
                    returnType = "line";
                    returnLines[i] = LineAnalysis(elements.GetRange(2, elements.Count - 2));
                    string returnActor = RequestNext(false, "actor", Int32.Parse(elements[0].value["id"]));
                    return (returnType, returnActor, returnLines);
                } else if (elements[1].GetNodeType() == "anim"){
                    returnType = "anim";
                    string returnActor = RequestNext(false, "actor", Int32.Parse(elements[0].value["id"]));
                    string[] animID = {elements[1].value["num"]};
                    return (returnType, returnActor, returnLines);
                } else {
                    throw new Exception("Unable to progress after discovering actor in analysis.");
                }
            } else if (elements[0].GetNodeType() == "choice"){
                returnType = "choice";
                TreeNode child = elements[0].GetChildren()[0];
                returnLines[i] = LineAnalysis(elements.GetRange(1, elements.Count - 1));
            } else if (elements[0].GetNodeType() == "jump"){
                returnType = "true";
                JumpToSection(elements[0]);
                return (returnType, null, null);
            } else if (elements[0].GetNodeType() == "if"){
                int value;
                if (elements[2].GetNodeType() == "var"){
                    // Get value of variable
                    // value == varValue
                } else if (elements[2].GetNodeType() == "num"){
                    value = Int32.Parse(elements[2].value["num"]);
                }

                if (elements[1].GetNodeType() == "assign"){
                    
                } else if (elements[1].GetNodeType() == "comp"){
                    int variableFirst;
                    //bool passed = EvalComparison(variableFirst, elements[1].value["op"], value);
                    // have goto if comp is true
                }
                returnType = "true";
                return (returnType, null, null);
            } else {
                throw new Exception($"Unrecognised node type '{elements[0].GetNodeType()}' found in syntax analysis step.");
            }
        }
        return (returnType, null, returnLines);
    }
    private string LineAnalysis(List<TreeNode> lines){
        string returnLine = "";
        for (int j = 0; j < lines.Count; j++)
        {
            returnLine += RequestNext(true, "english", Int32.Parse(lines[j].value["id"])) + " ";
        }
        returnLine = returnLine[..^1];
        return returnLine;
    }
    private void JumpToSection(TreeNode jumpNode){
        string section = jumpNode.value["section"];
        lineNumber = dialogueSections[section] + 1;
    }
    private bool EvalComparison(int variable, string op, int compareTo){
        switch(op){
            case "==":
                return variable == compareTo;
            case ">=":
                return variable >= compareTo;
            case "<=":
                return variable <= compareTo;
            case "!=":
                return variable != compareTo;
            default:
                throw new Exception($"{op} is not a valid comparison operator");
        }
    }
}
