using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;

public class DialogueDatabaseManager : MonoBehaviour
{
    public static DialogueDatabaseManager Instance;

    void Awake()
    {
        if (Instance == null)
        { // If the instance has not been assigned
            Instance = this; // Set it as this one
        }
    }
    public IDbConnection OpenDialogueDb()
    { // Open the database with the dialogue table
        return OpenDb("Dialogue", new string[] { "english TEXT" });
    }
    public IDbConnection OpenActorDb()
    { // Open the database with the actor table
        return OpenDb("Actor", new string[] { "actor TEXT" });
    }
    private IDbConnection OpenDb(string tableName, string[] columns)
    {
        string database = "URI=file:" + Path.Combine(Application.streamingAssetsPath, "Dialogue.sqlite"); // Get a path to the database
        IDbConnection connection = new SqliteConnection(database); // Create a connection to it
        connection.Open(); // Open the connection
        IDbCommand createTable = connection.CreateCommand(); // Begin creating a new command

        string columnText = ""; // Store the creation text for the columns
        foreach (string column in columns)
        { // For each column supplied
            columnText += column + ", "; // Add it to columnText
        }
        columnText = columnText[..^2]; // Remove the trailing ', '
        createTable.CommandText = $"CREATE TABLE IF NOT EXISTS {tableName} (id INTEGER PRIMARY KEY, {columnText})"; // Set the SQL command
        createTable.ExecuteReader(); // Execute the query

        return connection;
    }
}
