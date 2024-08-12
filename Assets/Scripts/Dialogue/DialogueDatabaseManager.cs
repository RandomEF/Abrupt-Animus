using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine.UIElements;

public class DialogueDatabaseManager : MonoBehaviour
{
    public IDbConnection OpenDialogueDb()
    {
        return OpenDb("Dialogue", new string[] {"english TEXT"});
    }
    public IDbConnection OpenActorDb(){
        return OpenDb("Actor", new string[] {"actor TEXT"});
    }
    private IDbConnection OpenDb(string databaseName, string[] columns){
        string database = "URI=file:" + Application.streamingAssetsPath + "/" + databaseName + ".sqlite";
        IDbConnection connection = new SqliteConnection(database);
        connection.Open();
        IDbCommand createTable = connection.CreateCommand();
        createTable.CommandText = $"CREATE TABLE IF NOT EXISTS {databaseName} (id INTEGER PRIMARY KEY, {columns})";
        createTable.ExecuteReader();

        return connection;
    }
}
