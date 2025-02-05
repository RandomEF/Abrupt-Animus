using System;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public GameObject player;
    public GameObject _camera;
    public PlayerInputs inputs;
    public SaveData saveData;
    public int merit = 0;
    public float sanity = 100f;
    public int totalKills;
    public int currentSlot = 1;


    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _camera = GameObject.Find("Main Camera");
        //DontDestroyOnLoad(player);
        inputs = new PlayerInputs();
        inputs.Player.Enable(); // Enabling only the Player input map
        saveData = new SaveData();
    }
    void Start(){
        saveData.playerData.player = player;
        SceneManager.sceneLoaded += ApplyData;
    }
    void SaveGame(string type){
        saveData.UpdateSaveData();
        SaveSystem.Save(saveData, type, currentSlot);
    }
    void ApplyData(Scene _, LoadSceneMode __){
        LoadGame();
    }
    void ApplyData(){
        LoadGame();
    }
    public void SetSlotandData(int slot, SaveData data){
        currentSlot = slot;
        saveData = data;
    }
    void LoadGame(){
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

    public int GetVarValue(string variableName){
        Type type = typeof(WorldFlags);
        PropertyInfo? property = type.GetProperty(variableName);
        if (property == null){
            throw new Exception($"{variableName} is not a variable of the manager.");
        } else{
            return Convert.ToInt32(property.GetValue(saveData.worldFlags, null));
        }
    }
    public void SetVarValue(string variableName, int value){
        Type type = typeof(WorldFlags);
        PropertyInfo? property = type.GetProperty(variableName);
        if (property == null){
            throw new Exception($"{variableName} is not a variable of the manager.");
        } else{
            property.SetValue(saveData, value);
        }
    }
}
