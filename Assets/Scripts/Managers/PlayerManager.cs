using System;
using System.Data;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Mono.Data.Sqlite;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public GameObject player;
    public GameObject _camera;
    public LevelLoading sceneLoader;
    public MenuManager menuManager;

    private IDbConnection db;
    public bool allowedToMove = false;

    public PlayerInputs inputs;
    public SaveData saveData;
    public int merit = 0;
    public float sanity = 100f;
    public int totalKills;
    public int currentSlot = 1;


    void Awake()
    {
        if (Instance == null){
            Instance = this;
        }
        inputs = new PlayerInputs();
        inputs.Menu.Enable(); // Enabling only the Menu input map
        saveData = new SaveData();
        db = OpenDb();
        GetStartPos("TitleMenu");
    }
    void Start(){
        menuManager = MenuManager.Instance;
        DontDestroyOnLoad(gameObject); // Since the game will start in the title screen, it makes sure that the player, manager, and all things attached are loaded and maintained across all scene loads
        saveData.playerData.player = player;
        //SceneManager.sceneLoaded += ApplyData; 
        SceneManager.sceneLoaded += SceneLoad;
        //SceneManager.sceneLoaded += FetchPlayer;
    }
    public void SaveGame(string type){
        saveData.UpdateSaveData();
        SaveSystem.Save(saveData, type, currentSlot);
    }
    void ApplyData(Scene _, LoadSceneMode __){
        LoadGame();
    }
    void ApplyData(){ // When a scene loads, it reloads the save data
        LoadGame();
    }
    void FetchReferences(){ // Depreciated function to find player, camera, and loading screen after a scene load.
        player = GameObject.FindWithTag("Player");
        _camera = GameObject.Find("Main Camera");
        sceneLoader = GetComponentInChildren<LevelLoading>();
    }
    public void SetSlotandData(int slot, SaveData data){
        currentSlot = slot;
        saveData = data;
    }
    public void LoadSlot(string sceneName){
        sceneLoader.gameObject.SetActive(true); // Enables the load screen
        sceneLoader.Load(sceneName, gameObject); // Once the new scene has loaded, execution will return here
        //FetchReferences();
        sceneLoader.gameObject.SetActive(false); // Disables the load screen
    }
    public void LoadGame(){ // Applies save data onto the objects in the world
        player.GetComponent<PlayerEntity>().Health = saveData.playerData.health;
        player.GetComponent<PlayerEntity>().MaxHealth = saveData.playerData.maxHealth;
        player.transform.position = saveData.playerData.position;
        player.GetComponent<Rigidbody>().linearVelocity = saveData.playerData.velocity;
        player.transform.rotation = saveData.playerData.bodyRotation;
        _camera.transform.rotation = saveData.playerData.lookRotation;
        // Grab the prefab in the right order for the weapons
        foreach(WeaponData data in saveData.playerData.weaponSlots){
            Addressables.LoadAssetAsync<GameObject>(data.name).Completed += (asyncOp) => {
                if (asyncOp.Status == AsyncOperationStatus.Succeeded) {
                    GameObject weapon = Instantiate(asyncOp.Result);
                    player.GetComponent<PlayerGunInteraction>().AddWeapon(weapon);
                    weapon.GetComponent<Weapon>().TotalAmmo = data.totalAmmo;
                    weapon.GetComponent<Weapon>().AmmoInClip = data.ammoInClip;
                } else {
                    Debug.LogError("Failed to load weapon");
                }
            };
        }
        player.GetComponent<PlayerGunInteraction>().activeWeaponSlot = saveData.playerData.activeWeaponSlot;
        merit = saveData.playerData.merit;
        sanity = saveData.playerData.sanity;
        totalKills = saveData.playerData.totalKills;
    }

    public int GetVarValue(string variableName){ // Gets the value of the variable named within itself
        Type type = typeof(WorldFlags);
        PropertyInfo? property = type.GetProperty(variableName); // Potentially null output
        if (property == null){
            throw new Exception($"{variableName} is not a variable of the manager.");
        } else{
            return Convert.ToInt32(property.GetValue(saveData.worldFlags, null));
        }
    }
    public void SetVarValue(string variableName, int value){ // Sets the value of the variable named within itself
        Type type = typeof(WorldFlags);
        PropertyInfo? property = type.GetProperty(variableName);
        if (property == null){
            throw new Exception($"{variableName} is not a variable of the manager.");
        } else{
            property.SetValue(saveData, value);
        }
    }

    public void AddMerit(int amount) => merit += amount;

    private IDbConnection OpenDb(){
        string database = "URI=file:" + Path.Combine(Application.streamingAssetsPath, "SceneInfo.sqlite");
        IDbConnection connection = new SqliteConnection(database);
        connection.Open();
        IDbCommand createTable = connection.CreateCommand();
        createTable.CommandText = $"CREATE TABLE IF NOT EXISTS SceneInfo (name TEXT PRIMARY KEY, moveability INTEGER, startX REAL, startY REAL, startZ REAL)";
        return connection;
    }
    public bool GetMoveability(string sceneName){
        IDbCommand request = db.CreateCommand();
        request.CommandText = $"SELECT moveability FROM SceneInfo WHERE name={sceneName}";
        IDataReader response = request.ExecuteReader();
        bool success = response.Read();
        if (!success){
            Debug.LogWarning($"Scene '{sceneName}' does not have an entry in the info database.");
            return true; // Most of the time the scene will be a level, so it is safer to put true.
        }
        return response.GetString(0) == "1";
    }
    private Vector3 GetStartPos(string sceneName){
        IDbCommand request = db.CreateCommand();
        request.CommandText = $"SELECT startX, startY, startZ FROM SceneInfo WHERE name=\"{sceneName}\"";
        IDataReader response = request.ExecuteReader();
        bool success = response.Read();
        if (!success){
            Debug.LogWarning($"Scene '{sceneName}' does not have an entry in the info database.");
            return Vector3.zero; // Default backup as most levels should be centered near 0,0,0
        }
        return new Vector3(response.GetFloat(0), response.GetFloat(1), response.GetFloat(2));
    }
    void SceneLoad(Scene scene, LoadSceneMode __){
        if (GetMoveability(scene.name)){
            inputs.Player.Enable();
            inputs.Menu.Disable();
            menuManager.ChangeMenu("HUD");
        } else {
            inputs.Player.Disable();
            inputs.Menu.Enable();
            menuManager.ChangeMenu("Main");
        }
        player.transform.position = GetStartPos(scene.name);
        ApplyData();
    }
    public void Quit(){
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    /*
    public void ToggleHUD(){
        if (HUD.isActiveAndEnabled){
            HUD.gameObject.SetActive(false);
        } else {
            HUD.gameObject.SetActive(true);
        }
    }*/
}
