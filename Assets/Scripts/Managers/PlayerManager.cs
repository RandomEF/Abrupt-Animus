using Mono.Data.Sqlite;
using System;
using System.Data;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public GameObject player;
    public LevelLoading sceneLoader;
    public MenuManager menuManager;

    private IDbConnection db;

    public PlayerInputs inputs;
    public SaveData saveData;
    public int merit = 0;
    public float sanity = 100f;
    public int totalKills;
    public int currentSlot = 1;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // Sets the player manager to this object
        }
        inputs = new PlayerInputs(); // make a new set of player inputs
        inputs.Menu.Enable(); // Enabling only the Menu input map
        saveData = new SaveData(); // make a new empty saveData
        //db = OpenDb(); // Open the scene information database
    }

    void Start()
    {
        menuManager = MenuManager.Instance; // Get the menu manager
        DontDestroyOnLoad(gameObject); // Since the game will start in the title screen, it makes sure that the player, manager, and all things attached are loaded and maintained across all scene loads
        //saveData.playerData.player = player;
        //SceneManager.sceneLoaded += ApplyData; 
        SceneManager.sceneLoaded += SceneLoad; // ATtach the relevant functions to the new scene load
        //FetchReferences();
        //SceneManager.sceneLoaded += FetchPlayer;
    }

    public void SaveGame(string type)
    {
        saveData.UpdateSaveData(player); // Update the save data
        SaveSystem.Save(saveData, type, currentSlot); // Write a save file with the current save data
    }

    void ApplyData()
    { // When a scene loads, it reloads the save data
        player.GetComponent<PlayerEntity>().Health = saveData.playerData.health; // Apply the player's health
        player.GetComponent<PlayerEntity>().MaxHealth = saveData.playerData.maxHealth; // Apply the player's maximum health
        player.transform.position = saveData.playerData.position; // Apply the player's position
        player.GetComponent<Rigidbody>().linearVelocity = saveData.playerData.velocity; // Apply the player's velocity
        player.transform.rotation = saveData.playerData.bodyRotation; // Apply the player's body's rotation
        Camera.main.transform.rotation = saveData.playerData.lookRotation; // Apply the player's vertical looking direction
        // Grab the prefab in the right order for the weapons
        foreach (WeaponData data in saveData.playerData.weaponSlots)
        {
            Addressables.LoadAssetAsync<GameObject>(data.name).Completed += (asyncOp) =>
            { // Load the weapon based on the ID
                if (asyncOp.Status == AsyncOperationStatus.Succeeded)
                { // If the load succeded
                    GameObject weapon = Instantiate(asyncOp.Result); // Create the weapon
                    player.GetComponent<PlayerGunInteraction>().AddWeapon(weapon); // Add the weapon to the player
                    weapon.GetComponent<Weapon>().TotalAmmo = data.totalAmmo; // Set the weapon's total ammo
                    weapon.GetComponent<Weapon>().AmmoInClip = data.ammoInClip; // Set the remaining ammo in the weapon's clip
                    weapon.GetComponent<Weapon>().ClipCapacity = data.clipCapacity; // Set the maximum capacity of the clip in the weapon
                }
                else
                {
                    Debug.LogError("Failed to load weapon");
                }
            };
        }
        player.GetComponent<PlayerGunInteraction>().activeWeaponSlot = saveData.playerData.activeWeaponSlot; // Set the active weapon
        merit = saveData.playerData.merit; // Set the merit the player has
        sanity = saveData.playerData.sanity; // Set the player's sanity level
        totalKills = saveData.playerData.totalKills; // Set the player's total kills
    }

    public void SetSlotandData(int slot, SaveData data)
    {
        currentSlot = slot; // Assign the slot to load
        saveData = data; // Assign the save data
    }

    public void LoadSlot(string sceneName)
    {
        if(Application.CanStreamedLevelBeLoaded(sceneName)){ // Check if the level exists
            menuManager.DisableAll();
            sceneLoader.gameObject.SetActive(true); // Enables the load screen
            sceneLoader.Load(sceneName); // Once the new scene has loaded, execution will return here
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' does not exist.");
        }
    }

    public void LoadGame()
    { // Used after a death to call SceneLoad
        SceneLoad(SceneManager.GetActiveScene(), new LoadSceneMode()); // Wrapper
    }

    public void LoadNextLevel(string sceneName)
    {
        SaveGame("Auto"); // Make an automatic save in case the level crashes the game
        LoadSlot(sceneName); // Load the next scene
    }

#nullable enable
    public int GetVarValue(string variableName)
    { // Gets the value of the variable named within itself from the world flags
        Type type = typeof(WorldFlags); // Gets the id of the type of WorldFlags
        PropertyInfo? property = type.GetProperty(variableName); // Potentially null output from trying to find the variable
        if (property == null)
        {
            Debug.LogError($"{variableName} is not a variable of the manager.");
            return -1; // Error code
        }
        else
        {
            return Convert.ToInt32(property.GetValue(saveData.worldFlags, null)); // Cast to an integer
        }
    }

    public void SetVarValue(string variableName, int value)
    { // Sets the value of the variable named within itself
        Type type = typeof(WorldFlags); // Gets the id of the type of WorldFlags
        PropertyInfo? property = type.GetProperty(variableName); // Potentially null output from trying to find the variable
        if (property == null)
        {
            Debug.LogError($"{variableName} is not a variable of the manager.");
            // Do nothing to not cause havoc
        }
        else
        {
            property.SetValue(saveData, value); // save the changed value
        }
    }
#nullable disable

    public void AddMerit(int amount) => merit += amount; // Increment the merit by amount. If amount is negative, allows subtracting merit as well

    private IDbConnection OpenDb()
    { // Opens the database and returns a connection to it
        string database = "URI=file:" + Path.Combine(Application.streamingAssetsPath, "SceneInfo.sqlite"); // The exact file path to where it is stored
        IDbConnection connection = new SqliteConnection(database); // Make a new connection with it
        connection.Open(); // Open the connection
        IDbCommand createTable = connection.CreateCommand(); // Begins a query
        createTable.CommandText = $"CREATE TABLE IF NOT EXISTS SceneInfo (name TEXT PRIMARY KEY NOT NULL, moveability INTEGER DEFAULT 1, startX REAL DEFAULT 0, startY REAL DEFAULT 0.85001, startZ REAL DEFAULT 0, maxEnemyCount INTEGER DEFAULT 0, menuOpen TEXT DEFAULT 'HUD')"; // Verifies that the required table exists, and creates it if not
        createTable.ExecuteReader(); // Execute the query
        return connection;
    }

    private (IDataReader, bool) QueryDatabase(string sceneName, string field)
    {
        db = OpenDb(); // Open and store a reference to the database
        IDbCommand request = db.CreateCommand(); // Begins a query
        request.CommandText = $"SELECT {field} FROM SceneInfo WHERE name='{sceneName}'"; // Get whether the player's movement script should be enabled or not
        IDataReader response = request.ExecuteReader(); // Execute the query
        bool success = response.Read(); // Read if the query succeded
        if (!success)
        {
            Debug.LogWarning($"Scene '{sceneName}' does not have an entry in the info database for the player's {field}.");
        }
        return (response, success); // Most of the time the scene will be a level, so it is safer to put true.
    }

    public bool GetMoveability(string sceneName)
    {
        (IDataReader response, bool success) = QueryDatabase(sceneName, "moveability"); // Query for the moveability
        bool output = success ? response.GetInt32(0) != 0 : true; // return whether the first integer result is equal to 0 if succeded, otherwise return true since it will usually be a level
        db.Close(); // Close the database
        return output;
    }

    public string GetCanvasOpen(string sceneName)
    {
        (IDataReader response, bool success) = QueryDatabase(sceneName, "menuOpen"); // Query for the open canvas
        string output = success ? response.GetString(0) : "HUD";  // Return the first string result if succeded, otherwise return "HUD" since it will usually be a level
        db.Close(); // Close the database
        return output;
    }

    private Vector3 GetStartPos(string sceneName)
    {
        (IDataReader response, bool success) = QueryDatabase(sceneName, "startX, startY, startZ"); // Query for the start position
        Vector3 output = success ? new Vector3(response.GetFloat(0), response.GetFloat(1), response.GetFloat(2)) : new Vector3(0, 0.85f, 0); // Return the combination of the 3 axis if succeded or the database default otherwise
        db.Close(); // Close the database
        return output;
    }

    void SceneLoad(Scene scene, LoadSceneMode __)
    {
        sceneLoader.gameObject.SetActive(false);
        if (GetMoveability(scene.name))
        { // If the player should move
            inputs.Player.Enable(); // Enable movement
            inputs.Menu.Disable(); // Disable menu inputs
        }
        else
        {
            inputs.Player.Disable(); // Disable movement
            inputs.Menu.Enable(); // Enable menu inputs
        }
        player.transform.position = GetStartPos(scene.name); // Grab the player's start position

        Debug.Log("Requested canvas on scene load");
        menuManager.ChangeMenu(GetCanvasOpen(scene.name)); // Change the menu to the one that should be open
        //Debug.Log(GetStartPos(scene.name));
        player.GetComponent<PlayerMovement>().movementState = PlayerMovement.PlayerMovementState.idle; // Give the player the ability to move
        ApplyData(); // Reload the save data
    }

    public void PlayerDeath()
    {
        player.GetComponent<PlayerMovement>().movementState = PlayerMovement.PlayerMovementState.dead; // Stop the player's movement
    }

    public void SetCursorMode()
    {
        Camera.main.GetComponent<PlayerLook>().SetCursorLock(SceneManager.GetActiveScene(), new LoadSceneMode()); // Set whether the cursor should be locked or not
    }

    public void Quit()
    {
#if UNITY_EDITOR // If this function is being run inside the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false; // Set the current playing state to false
#else
        Application.Quit(); // Call the system-level quit on the game
#endif
    }
    /*
    public void ToggleHUD(){
        if (HUD.isActiveAndEnabled){
            HUD.gameObject.SetActive(false);
        } else {
            HUD.gameObject.SetActive(true);
        }
    }
    void ApplyData(Scene _, LoadSceneMode __){
        LoadGame();
    }
    void FetchReferences()
    { // Depreciated function to find player, camera, and loading screen after a scene load.
        player = GameObject.FindWithTag("Player"); // Find the player
        //_camera = GameObject.Find("Main Camera"); // Find the camera
        sceneLoader = GetComponentInChildren<LevelLoading>(); // Get the loading screen script
    }
    */
}
