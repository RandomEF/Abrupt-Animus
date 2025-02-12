using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public GameObject player;
    public GameObject _camera;
    public LevelLoading sceneLoader;

    public PlayerInputs inputs;
    public SaveData saveData;
    public int merit = 0;
    public float sanity = 100f;
    public int totalKills;
    public int currentSlot = 1;


    void Awake()
    {
        inputs = new PlayerInputs();
        inputs.Player.Enable(); // Enabling only the Player input map
        saveData = new SaveData();
    }
    void Start(){
        DontDestroyOnLoad(gameObject); // Since the game will start in the title screen, it makes sure that the player, manager, and all things attached are loaded and maintained across all scene loads
        saveData.playerData.player = player;
        SceneManager.sceneLoaded += ApplyData; // When a scene loads, it reloads the save data
        //SceneManager.sceneLoaded += FetchPlayer;
    }
    public void SaveGame(string type){
        saveData.UpdateSaveData();
        SaveSystem.Save(saveData, type, currentSlot);
    }
    void ApplyData(Scene _, LoadSceneMode __){
        LoadGame();
    }
    void ApplyData(){
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
}
