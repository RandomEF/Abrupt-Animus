using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;

public class DialogueDatabaseManager : MonoBehaviour
{
    public IDbConnection OpenDialogueDb()
    {
        return OpenDb("Dialogue", new string[] {"english TEXT"});
    }
    public IDbConnection OpenActorDb(){
        return OpenDb("Actor", new string[] {"actor TEXT"});
    }
    private IDbConnection OpenDb(string tableName, string[] columns){
        string database = "URI=file:" + Path.Combine(Application.streamingAssetsPath, "Dialogue.sqlite");
        IDbConnection connection = new SqliteConnection(database);
        connection.Open();
        IDbCommand createTable = connection.CreateCommand();

        string columnText = "";
        foreach(string column in columns){
            columnText += column + ", ";
        }
        columnText = columnText[..^2];
        createTable.CommandText = $"CREATE TABLE IF NOT EXISTS {tableName} (id INTEGER PRIMARY KEY, {columnText})";
        createTable.ExecuteReader();

        return connection;
    }
}
