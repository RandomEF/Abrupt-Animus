using UnityEngine;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Text;

public class DialogueInteraction : Interactable
{
    public PlayerManager gameManager;
    private DialogueDatabaseManager ddm;
    public DialogueManager dialogueManager;
    private List<(int, string)> actorsInInteraction = new List<(int, string)>();
    public UnityEngine.Object dialogueFile;
    private string filepath;
    [SerializeField] private Dictionary<string, int> dialogueSections = new Dictionary<string, int>();
    private StreamReader fileReader;
    void Start()
    {
        gameObject.tag = "Interact"; // Set the object that has this to be detected as interactable
        gameManager = PlayerManager.Instance; // Fetch the player manager
        ddm = DialogueDatabaseManager.Instance; // Fetch the dialogue database manager
        dialogueManager = DialogueManager.Instance; // Fetch the dialogue manager
        filepath = Path.Combine(Application.streamingAssetsPath, "Dialogue", dialogueFile.name + ".dlg"); // The path to the dialogue file
        PreprocessSections(); // Fetch the dialogue sections
    }
    public override void Interact()
    {
        (string, string, string[]) output = RetrieveNextLine(); // Get the next line
        if (output.Item1 == "line")
        { // If it is a line, send it to the dialogue manager
            dialogueManager.DisplayLine(output.Item2 + ": " + output.Item3[0]);
        }
    }
    private void PreprocessSections()
    {
        StreamReader reader = new StreamReader(filepath);
        int lineNumber = 1; // Start from the beginning of the file
        while (!reader.EndOfStream)
        { // While the end of the file has not been reached
            string line = reader.ReadLine(); // Read the line into a variable
            if (line.StartsWith("[") && line.EndsWith("]"))
            { // If the line starts and ends with square brackets
                dialogueSections.Add(line[1..^1], lineNumber); // Store the name of the section with the line number
            }
            lineNumber++; // Next line
        }
    }
    private string RequestNext(bool isDialogue, string language, int lineID)
    {
        IDbConnection connection = isDialogue ? ddm.OpenDialogueDb() : ddm.OpenActorDb(); // If the database needed is dialogue, open that table, otherwise open the actor table
        IDbCommand lineRequest = connection.CreateCommand(); // Begin a query
        lineRequest.CommandText = $"SELECT {language} FROM {(isDialogue ? "Dialogue" : "Actor")} WHERE id={lineID}"; // Select the language of ID lineID from the right table
        IDataReader response = lineRequest.ExecuteReader(); // Execute the query
        bool success = response.Read(); // Read a line and store it
        if (!success)
        { // If the query failed
            Debug.LogError($"Line {lineID} is not in {(isDialogue ? "Dialogue" : "Actor")} table");
            connection.Close();
            return ""; // Return an empty string
        }
        string line = response.GetString(0); // Get the first string
        connection.Close(); // Close the database
        return line; // Return the line
    }
    private string ConvertLine(string line)
    {
        string copy = line; // Store a cory of the line
        bool firstItalics = true; // Tracks whether the first underscore is yet to come
        bool firstBold = true; // Tracks whether the first asterisk is yet to come
        for (int i = 0; i < line.Length; i++)
        { // For every character in the line
            if (line[i] == '\\')
            { // Ignore the next character
                i++; // Skip over the next character
            }
            else if (line[i] == '<')
            { // Begin reading a colour
                string hexColour = line.Substring(++i, 7); // Grab a colour code
                copy += "<color=" + hexColour + ">"; // Add the hexcode along with the <color= >
                i += 6; // Skip over the hexcode
            }
            else if (line[i] == '>')
            {
                copy += "</color>"; // Replace with the ending tag
            }
            else if (line[i] == '_')
            {
                if (firstItalics)
                {
                    copy += "<i>"; // Replace with the starting italics tag
                    firstItalics = false;
                }
                else
                {
                    copy += "</i>"; // Replace with the ending italics tag
                    firstItalics = true;
                }
            }
            else if (line[i] == '*')
            {
                if (firstBold)
                {
                    copy += "<b>"; // Replace with the starting bold tag
                    firstBold = false;
                }
                else
                {
                    copy += "</b>"; // Replace with then ending bold tag
                    firstBold = true;
                }
            }
            else
            {
                copy += line[i]; // Just copy the character
            }
        }
        return copy;
    }
    // DLG Interpreter
    private int lineNumber = 1; // The line of the file that the interpreter is on
    /*
    -> is the goto operator
    [] is dialogue section
    #Ax is Actor database
    #Lx is Line database
    :: actor line/actor split
    @ is variable
    = is variable assign
    */
    public (string, string, string[]) RetrieveNextLine()
    {
        while (true)
        {
            TreeNode tokenTree = Tokenizer(); // Tokenize the line
            (string, string, string[]) returnVals = Analyzer(tokenTree); // Analyse the tokens of the lines
            string type = returnVals.Item1;
            if (type == "true")
            {
                continue;
            }

            if (type == "choice")
            {
                string[] lines = returnVals.Item3;
                string[] convertedLines = new string[lines.Length];
                for (int i = 0; i < lines.Length; i++)
                {
                    convertedLines[i] = ConvertLine(lines[i]);
                }
                return (type, returnVals.Item2, convertedLines); // returnVals.Item2 will be blank as the actor is assumed to be the player, but this must be passed through to fulfill the return type conditions
            } /*else if (type == "facing"){

            } */
            else if (type == "line")
            {
                string actor = returnVals.Item2;
                string line = ConvertLine(returnVals.Item3[0]);
                string[] lines = new string[1];
                lines[0] = line;
                return (type, actor, lines);
            }
            else
            {
                throw new Exception($"Line Retrieval: '{type}' was not a recognised type.");
            }
        }
    }
    private TreeNode Tokenizer()
    {
        if (fileReader == null)
        {
            fileReader = new StreamReader(filepath); // Reopen the file
        }
        for (int i = 0; i < lineNumber; i++)
        {
            fileReader.ReadLine(); // Skip lines until the current one is reached
        }
        TreeNode fileTree = new TreeNode(); // This is here in case a choice exists, in which case multiple lines need to be read at once
        int lastLineNumber = lineNumber - 1;
        string line = fileReader.ReadLine(); // Read the current line
        while (!fileReader.EndOfStream && lastLineNumber != lineNumber)
        {
            byte[] lineBytes = Encoding.UTF8.GetBytes(line); // Convert into a list of characters
            MemoryStream stream = new MemoryStream(lineBytes); // Make into a stream for StreamReader later
            lastLineNumber = lineNumber; // Set the previous line
            if (line.Length < 3)
            { // The minimum length of the line cannot go below 3
                throw new System.Exception($"Tokenizer: The line '{line}' is missing information.");
            }
            StreamReader lineReader = new StreamReader(stream); // Prepare for reading per character
            TreeNode lineTree = new TreeNode(); // New node for the line
            fileTree.AddChild(lineTree); // Add the node to the file tree
            ReadNext(lineReader, lineTree); // Tokenize the line
            bool shouldStay = false; // Initialise shouldStay
            if (fileTree.GetChildren()[^1].GetNodeType() == "choice")
            { // If the node is a choice node
                if (fileTree.GetChildren()[^1].value["end"] == "false")
                { // If it is not the last node in the choice - it is necessary to put this on a separate line to avoid an error
                    shouldStay = true; // Require reading the next line
                }
            }

            string nextLine = fileReader.ReadLine(); // Check the next line

            if (nextLine[0] != '[' && nextLine != null)
            { // If the next line isn't a dialogue section header or the end of the file
                lineNumber++; // Now on the next line
                line = nextLine; // Set the current line
                if (!shouldStay)
                { // If staying is not required
                    break; // Exit
                }
            }
        }
        return fileTree; // Return the read lines
    }
    // Tokenizer
    private void ReadNext(StreamReader line, TreeNode lineTree)
    {
        while (!line.EndOfStream)
        {
            char token = ReadChar(line);
            if (token == '#')
            {
                ReadID(line, lineTree);
            }
            else if (token == '@')
            {
                ReadIf(line, lineTree);
            }
            else if (token == '-')
            {
                ReadJump(line, lineTree);
            }
            else if (token == '|')
            {
                ReadChoice(line, lineTree);
            }
            else if (token == ':')
            {
                token = ReadChar(line);
                if (token == ':')
                {
                    ReadSeparator(line, lineTree);
                } /*else if (token == '-'){
                    ReadAnimation(line, lineTree);
                }*/
            }
            else
            {
                throw new System.Exception($"Tokenizer: The starting character '{token}' was not recognised.");
            }
        }
    }
    private char ReadChar(StreamReader line)
    {
        Int32 character = 32;
        while (character == 32)
        {
            character = line.Read();
        }
        return character == -1 ? throw new System.Exception("Tokenizer: Nothing left to read.") : (char)character;
    }
    private char PeekChar(StreamReader line)
    {
        Int32 character = 32;
        while (character == 32)
        {
            character = line.Peek();
        }
        return character == -1 ? throw new System.Exception("Tokenizer: Nothing left to peek.") : (char)character;
    }
    private void ReadID(StreamReader line, TreeNode lineTree)
    {
        char character = ReadChar(line);
        int id = ReadNumber(line);
        TreeNode idNode = new TreeNode();
        lineTree.AddChild(idNode);

        if (character == 'A')
        {
            idNode.AddType("actor");
        }
        else if (character == 'L')
        {
            idNode.AddType("line");
        }
        else
        {
            throw new Exception($"Tokenizer: Unable to decide if this is an actor or line reference: {character}");
        }
        idNode.value.Add("id", id.ToString());
    }
    private int ReadNumber(StreamReader line)
    {
        string num = "";
        while (!line.EndOfStream)
        {
            char character = PeekChar(line);
            if (int.TryParse(character.ToString(), out _))
            {
                num += ReadChar(line);
            }
            else
            {
                break;
            }
        }
        int inted;
        int.TryParse(num, out inted);
        return inted;
    }
    private void ReadIf(StreamReader line, TreeNode lineTree)
    {
        TreeNode ifNode = new TreeNode();
        lineTree.AddChild(ifNode);
        ReadVariable(line, ifNode);
        ReadOperator(line, ifNode);
        ifNode.AddType("if");
        //ifNode.value.Add("op");
        char character = PeekChar(line);
        if (Char.IsNumber(character))
        {
            TreeNode numNode = new TreeNode();
            ifNode.AddChild(numNode);
            numNode.AddType("num");
            numNode.value.Add("num", ReadNumber(line).ToString());
        }
        else if (Char.IsLetter(character) || character == '_')
        {
            ReadVariable(line, ifNode);
        }
        else
        {
            throw new Exception("Tokenizer: Nothing valid to compare to.");
        }
    }
    private void ReadVariable(StreamReader line, TreeNode lineTree)
    {
        TreeNode variableNode = new TreeNode();
        lineTree.AddChild(variableNode);
        string variable = "";
        char character = PeekChar(line);
        if (Char.IsNumber(character))
        {
            throw new Exception("Tokenizer: Variable can't start with a number!");
        }
        while (!line.EndOfStream)
        {
            character = PeekChar(line);
            if (Char.IsLetterOrDigit(character) || character == '_')
            {
                variable += ReadChar(line);
            }
            else
            {
                break;
            }
        }
        variableNode.AddType("var");
        variableNode.value.Add("var", variable);
    }
    private string ReadOperator(StreamReader line, TreeNode lineTree)
    {
        string op = "";
        char[] opChars = { '!', '>', '<', '=' };
        //string[] operators = {"==", "<=", ">=", "!="};
        TreeNode opNode = new TreeNode();
        lineTree.AddChild(opNode);
        while (!line.EndOfStream)
        {
            char character = PeekChar(line);
            if (op.Length == 0 && opChars.Contains(character))
            {
                op += ReadChar(line);
            }
            else if (op.Length == 1 && character == '=')
            {
                op += ReadChar(line);
                opNode.AddType("comp");
                opNode.value.Add("op", op);
                return op;
            }
            else if (op.Length == 1 && op == "=")
            {
                opNode.AddType("assign");
                return op;
            }
            else
            {
                throw new Exception($"Tokenizer: Invalid operator '{op}'.");
            }
        }
        throw new Exception($"Tokenizer: No valid operator found: '{op}'");
    }
    private void ReadJump(StreamReader line, TreeNode lineTree)
    {
        char character = ReadChar(line);
        if (character == '>')
        {
            string section = "";
            while (!line.EndOfStream)
            {
                section += ReadChar(line);
            }
            TreeNode sectionNode = new TreeNode();
            lineTree.AddChild(sectionNode);
            sectionNode.AddType("jump");
            sectionNode.value.Add("section", section);
        }
        else
        {
            throw new Exception("Tokenizer: This is not a jump, what were you trying to signify?");
        }
    }
    private void ReadChoice(StreamReader line, TreeNode lineTree)
    {
        TreeNode choiceNode = new TreeNode();
        choiceNode.AddType("choice");
        lineTree.AddChild(choiceNode);
        if (!line.EndOfStream)
        {
            char character = ReadChar(line);
            if (character == ',')
            {
                choiceNode.value.Add("end", "false");
            }
            else if (character == '#')
            {
                ReadID(line, lineTree);
            }
            else
            {
                throw new Exception($"Tokenizer: Unrecognised character '{character}' following choice line");
            }
        }
        else
        {
            choiceNode.value.Add("end", "true");
        }
    }
    private void ReadSeparator(StreamReader line, TreeNode lineTree)
    {
        char character = PeekChar(line);
        if (character == ':')
        {
            ReadChar(line);
            TreeNode separatorNode = new TreeNode();
            separatorNode.AddType("sep");
            lineTree.AddChild(separatorNode);
        }
    }/*
    private void ReadAnimation(StreamReader line, TreeNode lineTree){
        char character = PeekChar(line);
        if (character == '-'){
            ReadChar(line);
            TreeNode animNode = new TreeNode();
            lineTree.AddChild(animNode);
            animNode.AddType("anim");
            animNode.value.Add("num", ReadNumber(line).ToString());
        }
    }*/
    // Analysis
    private (string, string, string[]) Analyzer(TreeNode fileTree)
    {
        string[] lines = new string[fileTree.GetChildren().Count];
        string returnType = "none";
        string[] returnLines = new string[lines.Length];
        for (int i = 0; i < fileTree.GetChildren().Count; i++)
        {
            TreeNode line = fileTree.GetChildren()[i];
            List<TreeNode> elements = line.GetChildren();
            if (elements[0].GetNodeType() == "actor")
            {
                if (elements[1].GetNodeType() == "anim")
                {
                    returnType = "animation";
                    string returnActor = RequestNext(false, "actor", Int32.Parse(elements[0].value["id"]));
                    string[] animID = { elements[1].value["num"] };
                    return (returnType, returnActor, animID);
                }/* else if (elements[1].GetNodeType() == "sep" && elements[2].GetNodeType() == "actor"){
                    returnType = "facing";
                    string returnActor1 = RequestNext(false, "actor", Int32.Parse(elements[0].value["id"]));
                    string[] returnActor2 = {RequestNext(false, "actor", Int32.Parse(elements[2].value["id"]))};
                    return (returnType, returnActor1, returnActor2);
                } */
                else if (elements[1].GetNodeType() == "sep" && elements[2].GetNodeType() == "line")
                {
                    returnType = "line";
                    returnLines[i] = LineAnalysis(elements.GetRange(2, elements.Count - 2));
                    string returnActor = RequestNext(false, "actor", Int32.Parse(elements[0].value["id"]));
                    return (returnType, returnActor, returnLines);
                }/* else if (elements[1].GetNodeType() == "anim"){
                    returnType = "anim";
                    string returnActor = RequestNext(false, "actor", Int32.Parse(elements[0].value["id"]));
                    string[] animID = {elements[1].value["num"]};
                    return (returnType, returnActor, returnLines);
                } */
                else
                {
                    throw new Exception("Analysis: Unable to progress after discovering actor in analysis.");
                }
            }
            else if (elements[0].GetNodeType() == "choice")
            {
                returnType = "choice";
                TreeNode child = elements[0].GetChildren()[0];
                returnLines[i] = LineAnalysis(elements.GetRange(1, elements.Count - 1));
            }
            else if (elements[0].GetNodeType() == "jump")
            {
                returnType = "true";
                JumpToSection(elements[0]);
                return (returnType, null, null);
            }
            else if (elements[0].GetNodeType() == "if")
            {
                returnType = "false";
                int var1 = AnalyseIf(elements[2]);
                if (elements[1].GetNodeType() == "assign")
                {
                    if (elements[2].value["var"] == "choice")
                    {
                        choice = var1;
                    }
                    else
                    {
                        gameManager.SetVarValue(elements[2].value["var"], var1);
                    }
                }
                else if (elements[1].GetNodeType() == "comp")
                {
                    int var2 = AnalyseIf(elements[3]);
                    bool passed = EvalComparison(var1, elements[1].value["op"], var2);
                    if (passed)
                    {
                        if (elements[4].GetNodeType() == "jump")
                        {
                            lineNumber = dialogueSections[elements[4].value["section"]];
                            returnType = "true";
                        }
                        else
                        {
                            throw new Exception("Analysis: Expected a 'jump' node with a section.");
                        }
                    }
                }
                return (returnType, null, null);
            }
            else
            {
                throw new Exception($"Analysis: Unrecognised node type '{elements[0].GetNodeType()}' found in syntax analysis step.");
            }
        }
        return (returnType, null, returnLines);
    }
    private int choice = 0;
    private int AnalyseIf(TreeNode element)
    {
        if (element.GetNodeType() == "var")
        {
            if (element.value["var"] == "choice")
            {
                return choice;
            }
            // Get value of variable
            return gameManager.GetVarValue(element.value["var"]);
        }
        else if (element.GetNodeType() == "num")
        {
            return Int32.Parse(element.value["num"]);
        }
        else
        {
            throw new Exception($"Analysis: 'if' came across node of type {element.GetNodeType()} instead of a var or num");
        }
    }
    private string LineAnalysis(List<TreeNode> lines)
    {
        string returnLine = "";
        for (int j = 0; j < lines.Count; j++)
        {
            returnLine += RequestNext(true, "english", Int32.Parse(lines[j].value["id"])) + " ";
        }
        returnLine = returnLine[..^1];
        return returnLine;
    }
    private void JumpToSection(TreeNode jumpNode)
    {
        string section = jumpNode.value["section"];
        lineNumber = dialogueSections[section] + 1;
    }
    private bool EvalComparison(int variable, string op, int compareTo)
    {
        switch (op)
        {
            case "==":
                return variable == compareTo;
            case ">=":
                return variable >= compareTo;
            case "<=":
                return variable <= compareTo;
            case "!=":
                return variable != compareTo;
            default:
                throw new Exception($"Analysis: {op} is not a valid comparison operator");
        }
    }
}
