using UnityEngine;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Text;
using Unity.Collections;

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
        Debug.Log(ConvertLine("This is a _spooky_ line filled with <#FFFF00*great* things> ---_and_ *bad* things."));
    }
    public override void Interact()
    {
        (string, string, string[]) output = RetrieveNextLine(); // Get the next line
        if (output.Item1 == "line")
        { // If it is a line, send it to the dialogue manager
            Debug.Log($"Sending line '{output.Item2 + ": " + output.Item3[0]}'");
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
            Debug.LogError($"{(isDialogue ? "Line" : "Actor")} {lineID} is not in {(isDialogue ? "Dialogue" : "Actor")} table");
            connection.Close();
            return ""; // Return an empty string
        }
        string line = response.GetString(0); // Get the first string
        connection.Close(); // Close the database
        return line; // Return the line
    }
    private string ConvertLine(string line)
    {
        string copy = "";
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
    private int lineNumber = 2; // The line of the file that the interpreter is on
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
            { // If the analyzer has decided another line must be read
                continue; // Read another line
            }

            if (type == "line")
            {
                string actor = returnVals.Item2; // Retrieve the actor
                string line = ConvertLine(returnVals.Item3[0]); // Convert the lone line
                string[] lines = new string[1]; // Make a new array of length 1 to store the converted line
                lines[0] = line; // Store the line in the return output
                return (type, actor, lines); // Return as appropriate
            }
            else if (type == "choice")
            { // If the node is a choice node
                string[] lines = returnVals.Item3; // Fetch all the dialogue options
                string[] convertedLines = new string[lines.Length]; // Make a new array for however many dialogue options
                for (int i = 0; i < lines.Length; i++)
                { // For each option
                    convertedLines[i] = ConvertLine(lines[i]); // Convert it and assign to the return output
                }
                return (type, returnVals.Item2, convertedLines); // returnVals.Item2 will be blank as the actor is assumed to be the player, but this must be passed through to fulfill the return type conditions
            } 
            /*else if (type == "facing")
            {

            } */
            else
            {
                throw new Exception($"Line Retrieval: '{type}' was not a recognised type.");
            }
        }
    }
    private TreeNode Tokenizer()
    {
        TreeNode fileTree = new TreeNode(); // This is here in case a choice exists, in which case multiple lines need to be read at once
        using (StreamReader fileReader = new StreamReader(filepath)){
            for (int i = 0; i < lineNumber - 1; i++)
            {
                fileReader.ReadLine(); // Skip lines until the current one is reached
            }
            int lastLineNumber = lineNumber - 1;
            string line = fileReader.ReadLine(); // Read the current line
            while (!fileReader.EndOfStream && lastLineNumber != lineNumber)
            {
                if (line.Length < 3)
                { // The minimum length of the line cannot go below 3
                    throw new Exception($"Tokenizer: The line '{line}' is missing information.");
                }
                byte[] lineBytes = Encoding.UTF8.GetBytes(line); // Convert into a list of characters
                MemoryStream stream = new MemoryStream(lineBytes); // Make into a stream for StreamReader later
                lastLineNumber = lineNumber; // Set the previous line
                StreamReader lineReader = new StreamReader(stream); // Prepare for reading per character
                TreeNode lineTree = new TreeNode(); // New node for the line
                lineTree.AddType("linetree");
                fileTree.AddChild(lineTree); // Add the node to the file tree
                ReadNext(lineReader, lineTree); // Tokenize the line
                bool shouldStay = false; // Initialise shouldStay
                if (fileTree.GetChildren()[^1].GetChildren()[^1].GetNodeType() == "choice")
                { // If the node is a choice node
                    if (fileTree.GetChildren()[^1].GetChildren()[^1].value["end"] == "false")
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
                    } else {
                        throw new Exception("Expected another line to complete the choice: maybe you have an extra comma?");
                    }
                }
            }
        }
        return fileTree; // Return the read lines
    }
    // Tokenizer
    private void ReadNext(StreamReader line, TreeNode lineTree)
    {
        while (!line.EndOfStream)
        { // While the end of the line hasn't been reached
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
            }
            else
            {
                throw new Exception($"Tokenizer: The starting character '{token}' was not recognised.");
            }
        }
    }
                } /*else if (token == '-'){
                    ReadAnimation(line, lineTree);
                }*/
    private char ReadChar(StreamReader line)
    {
        int character = 32; // ASCII for space
        while (character == 32)
        { // Skips over spaces
            character = line.Read();
        }
        return character == -1 ? throw new Exception("Tokenizer: Nothing left to read.") : (char)character; // If it is not the end of the line, return the char
    }
    private char PeekChar(StreamReader line)
    {
        int character = 32; // ASCII for space
        while (character == 32)
        { // Skips over spaces
            character = line.Peek();
        }
        return character == -1 ? throw new Exception("Tokenizer: Nothing left to peek.") : (char)character; // If it is not the end of the line, return the char
    }
    private void ReadID(StreamReader line, TreeNode lineTree)
    {
        char character = ReadChar(line); // Read the next character
        int id = ReadNumber(line); // Read the number attached to it
        TreeNode idNode = new TreeNode(); // Make a new ID node
        lineTree.AddChild(idNode); // Add it to the line's children

        if (character == 'A')
        { // If the ID is an actor
            idNode.AddType("actor");
        }
        else if (character == 'L')
        { // If the ID is a line
            idNode.AddType("line");
        }
        else
        {
            throw new Exception($"Tokenizer: Unable to decide if this is an actor or line reference: {character}");
        }
        idNode.value.Add("id", id.ToString()); // Add the number to the node's dictionary
    }
    private int ReadNumber(StreamReader line)
    {
        string num = "";
        while (!line.EndOfStream)
        { // While the end of the line hasn't been reached
            char character = PeekChar(line); // Peek a character
            if (int.TryParse(character.ToString(), out _))
            { // If it is a number
                num += ReadChar(line); // Read it and and it to the number string
            }
            else
            {
                break;
            }
        }
        return int.Parse(num); // Return the number
    }
    private void ReadIf(StreamReader line, TreeNode lineTree)
    {
        TreeNode ifNode = new TreeNode(); // Make a new node
        lineTree.AddChild(ifNode); // Add it to lineTree's children
        ifNode.AddType("if"); // Set the type to 'if'
        ReadVariable(line, ifNode); // Read the variable and add it to ifNode's children
        ReadOperator(line, ifNode); // Read the operator and add it to ifNode's children
        char character = PeekChar(line); // Peek at the next character
        if (char.IsNumber(character))
        { // If it is a number
            TreeNode numNode = new TreeNode();
            ifNode.AddChild(numNode); // Make a new child node to store the number
            numNode.AddType("num"); // Set the type to 'num'
            numNode.value.Add("num", ReadNumber(line).ToString()); // Assign the number to numNode
        }
        else if (char.IsLetter(character) || character == '_')
        { // If the next character is a letter or _
            ReadVariable(line, ifNode); // Read a variable
        }
        else
        {
            throw new Exception("Tokenizer: Nothing valid to compare to.");
        }
    }
    private void ReadVariable(StreamReader line, TreeNode lineTree)
    {
        char character = PeekChar(line); // Check what the next character is
        if (char.IsNumber(character))
        { // A variable name can't start with a number
            throw new Exception("Tokenizer: Variable can't start with a number!");
        }
        TreeNode variableNode = new TreeNode(); // Make a new node
        lineTree.AddChild(variableNode); // Add it to lineTree's children
        string variable = ""; // Define variable
        while (!line.EndOfStream)
        { // Until the end of the line
            character = PeekChar(line); // Peek at the next character
            if (char.IsLetterOrDigit(character) || character == '_')
            { // If the character is valid
                variable += ReadChar(line); // Read it and add it to the name
            }
            else
            {
                break;
            }
        }
        variableNode.AddType("var"); // Add the type 'var'
        variableNode.value.Add("var", variable); // Add the name of the variable
    }
    private string ReadOperator(StreamReader line, TreeNode lineTree)
    {
        string op = ""; // Create a variable to hold the operator
        char[] opChars = { '!', '>', '<', '=' }; // List of the acceptable characters
        TreeNode opNode = new TreeNode(); // Make a new node
        lineTree.AddChild(opNode); // Add it to lineTree's children
        while (!line.EndOfStream)
        { // While not the end of the line
            char character = PeekChar(line); // Check the next character
            if (op.Length == 0 && opChars.Contains(character))
            { // If nothing has been read and its a valid character
                op += ReadChar(line); // Add it to the string
            }
            else if (op.Length == 1 && opChars.Contains(character))
            { // If it is an comparison
                op += ReadChar(line); // Add the character to the string
                opNode.AddType("comp"); // Assign the type as comparison
                opNode.value.Add("op", op); // Add the comparison type
                return op;
            }
            else if (op.Length == 1 && op == "=")
            { // If it is only an assignment
                opNode.AddType("assign"); // Set the type as assignmentt
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
        char character = ReadChar(line); // Read the next character
        if (character == '>')
        { // If it is an arror
            string section = ""; // Define a variable for the section name
            while (!line.EndOfStream)
            { // Copy all other characters as the section name
                section += ReadChar(line);
            }
            TreeNode sectionNode = new TreeNode(); // Create a new node
            lineTree.AddChild(sectionNode); // Add it to lineTree's children
            sectionNode.AddType("jump"); // Set the type as jump
            sectionNode.value.Add("section", section); // Set the section name
        }
        else
        {
            throw new Exception("Tokenizer: This is not a jump, what were you trying to signify?");
        }
    }
    private void ReadChoice(StreamReader line, TreeNode lineTree)
    {
        TreeNode choiceNode = new TreeNode(); // Make a new node
        choiceNode.AddType("choice"); // Set the type as choice
        lineTree.AddChild(choiceNode); // Add to lineTree's children
        while (!line.EndOfStream)
        { // If it isn't the end of the line
            char character = ReadChar(line); // Get the next character
            
            if (character == '#')
            { // Read an ID
                ReadID(line, lineTree);
            }
            else if (character == ',')
            { // If there is a comma
                choiceNode.value.Add("end", "false"); // Mark that there will be another line
                return;
            }
            else
            {
                throw new Exception($"Tokenizer: Unrecognised character '{character}' following choice line");
            }
        }
        choiceNode.value.Add("end", "true"); // No comma = last option
    }
    private void ReadSeparator(StreamReader line, TreeNode lineTree)
    { // Required to check for proper syntax
        TreeNode separatorNode = new TreeNode(); // Make a new node
        separatorNode.AddType("sep"); // Set the type as a separator
        lineTree.AddChild(separatorNode); // Add to lineTree's children
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
        string[] lines = new string[fileTree.GetChildren().Count]; // Make a new return array based on the number of lines
        string returnType = "none"; // Default return type
        string[] returnLines = new string[lines.Length]; // Copy of lines
        for (int i = 0; i < fileTree.GetChildren().Count; i++)
        { // For each line
            TreeNode line = fileTree.GetChildren()[i]; // Get the line
            List<TreeNode> elements = line.GetChildren(); // Get the elements of the line
            if (elements[0].GetNodeType() == "actor")
            { // If the line is an actor
                /*
                if (elements[1].GetNodeType() == "anim")
                {
                    returnType = "animation";
                    string returnActor = RequestNext(false, "actor", Int32.Parse(elements[0].value["id"]));
                    string[] animID = { elements[1].value["num"] };
                    return (returnType, returnActor, animID);
                } else if (elements[1].GetNodeType() == "sep" && elements[2].GetNodeType() == "actor"){
                    returnType = "facing";
                    string returnActor1 = RequestNext(false, "actor", Int32.Parse(elements[0].value["id"]));
                    string[] returnActor2 = {RequestNext(false, "actor", Int32.Parse(elements[2].value["id"]))};
                    return (returnType, returnActor1, returnActor2);
                } 
                else */ 
                /* if (elements[1].GetNodeType() == "anim")
                {
                    returnType = "anim";
                    string returnActor = RequestNext(false, "actor", Int32.Parse(elements[0].value["id"]));
                    string[] animID = {elements[1].value["num"]};
                    return (returnType, returnActor, returnLines);
                } else */
                if (elements[1].GetNodeType() == "sep" && elements[2].GetNodeType() == "line")
                { // If a separator and a line node follow the actor
                    returnType = "line"; // Set the return type to line
                    returnLines[i] = LineAnalysis(elements.GetRange(2, elements.Count - 2)); // Analyse all the lines into 1
                    string returnActor = RequestNext(false, "actor", Int32.Parse(elements[0].value["id"])); // Get the actor from the database
                    return (returnType, returnActor, returnLines); // Return the values
                }
                else
                {
                    foreach (TreeNode element in elements){
                        Debug.Log(element.GetNodeType());
                    }
                    throw new Exception("Analysis: Unable to progress after discovering actor in analysis.");
                }
            }
            else if (elements[0].GetNodeType() == "jump")
            {
                returnType = "true"; // Tell RequestNextLine that it needs to jump to another line
                JumpToSection(elements[0]); // Get the line number of the section
                return (returnType, null, null);
            }
            else if (elements[0].GetNodeType() == "if")
            {
                returnType = "false";
                int var1 = AnalyseIf(elements[2]); // Analyse the third element
                if (elements[1].GetNodeType() == "assign")
                { // If the element is getting assigned
                    if (elements[2].value["var"] == "choice")
                    { // If the third element was the variable choice
                        choice = var1; // Set choice to the value of var1
                    }
                    else
                    {
                        gameManager.SetVarValue(elements[2].value["var"], var1); // Tell the game manager to assign the variable in the save data
                    }
                }
                else if (elements[1].GetNodeType() == "comp")
                { // If the variable is being compared
                    int var2 = AnalyseIf(elements[3]); // Analyse the next variable
                    bool passed = EvalComparison(var1, elements[1].value["op"], var2); // Compare the variables
                    if (passed)
                    { // If the comparison was true
                        if (elements[4].GetNodeType() == "jump")
                        { // If the next node is a jump node
                            JumpToSection(elements[4]); // Set the line number to the section mentioned
                            returnType = "true"; // Tell RequestNextLine that it needs to jump to another line
                        }
                        else
                        {
                            throw new Exception("Analysis: Expected a 'jump' node with a section.");
                        }
                    }
                }
                return (returnType, null, null);
            }
            else if (elements[0].GetNodeType() == "choice")
            {
                returnType = "choice";
                TreeNode child = elements[0].GetChildren()[0];
                returnLines[i] = LineAnalysis(elements.GetRange(1, elements.Count - 1));
                // Not fully implemented
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
        { // If the node is variable
            if (element.value["var"] == "choice")
            { // If that variable is choice
                return choice; // Return the value of choice
            }
            // Get value of variable
            return gameManager.GetVarValue(element.value["var"]); // Request the game manager for the value of the variable from the save data
        }
        else if (element.GetNodeType() == "num")
        { // If the element was a number
            return int.Parse(element.value["num"]); // Cast the number
        }
        else
        {
            throw new Exception($"Analysis: 'if' came across node of type {element.GetNodeType()} instead of a var or num");
        }
    }
    private string LineAnalysis(List<TreeNode> lines)
    {
        string returnLine = ""; // Store the output line
        for (int j = 0; j < lines.Count; j++)
        { // For every dialogue line
            returnLine += RequestNext(true, "english", Int32.Parse(lines[j].value["id"])) + " "; // Request it from the database then append it the output
        }
        returnLine = returnLine[..^1]; // Remove the trailing space
        return returnLine;
    }
    private void JumpToSection(TreeNode jumpNode)
    {
        string section = jumpNode.value["section"]; // Get the line number of the section
        lineNumber = dialogueSections[section] + 1; // Set the new line number
    }
    private bool EvalComparison(int variable, string op, int compareTo)
    {
        switch (op)
        { // Compare based on the operator
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
