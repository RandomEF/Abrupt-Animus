using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PlayerData
{
    [JsonIgnore]
    public GameObject player;
    [JsonIgnore]
    public GameObject camera;
    [JsonIgnore]
    public PlayerManager playerManager;

    public string currentScene;
    public float health;
    public float maxHealth;
    public SerVector3 position;
    public SerVector3 velocity;
    public SerQuaternion bodyRotation;
    public SerQuaternion lookRotation;
    public List<WeaponData> weaponSlots;
    public int activeWeaponSlot;
    public int merit;
    public float sanity;
    public float timeOnSave;
    public int totalKills;

    public PlayerData(){
        currentScene = "Tutorial";
        health = 100f;
        position = new SerVector3(0,0,0);
        velocity = new SerVector3(0,0,0);
        bodyRotation = new SerQuaternion(0,0,0,0);
        lookRotation = new SerQuaternion(0,0,0,0);
        weaponSlots = new List<WeaponData>();
        activeWeaponSlot = 0;
        merit = 0;
        sanity = 100f;
        timeOnSave = 0;
        totalKills = 0;
    }
    private void Start() {
        playerManager = GameObject.Find("Game Manager").GetComponent<PlayerManager>();
    }
    public void UpdateData(){
        currentScene = SceneManager.GetActiveScene().name;
        health = player.GetComponent<PlayerEntity>().Health;
        maxHealth = player.GetComponent<PlayerEntity>().MaxHealth;
        position = player.transform.position;
        velocity = player.GetComponent<Rigidbody>().linearVelocity;
        bodyRotation = player.transform.rotation;
        lookRotation = camera.transform.rotation;
        foreach (GameObject weapon in player.GetComponent<PlayerGunInteraction>().weaponSlots){
            Weapon weaponScript = weapon.GetComponent<Weapon>();
            weaponSlots.Add(new WeaponData(weaponScript.WeaponType, weaponScript.TotalAmmo, weaponScript.AmmoInClip));
        }
        activeWeaponSlot = player.GetComponent<PlayerGunInteraction>().activeWeaponSlot;
        merit = playerManager.merit;
        sanity = playerManager.sanity;
        timeOnSave += Time.realtimeSinceStartup;
        totalKills = playerManager.totalKills;
    }
}

[System.Serializable]
public class WeaponData
{
    public string name;
    public int totalAmmo;
    public int ammoInClip;
    public WeaponData(string name, int totalAmmo, int ammoInClip){
        this.name = name;
        this.totalAmmo = totalAmmo;
        this.ammoInClip = ammoInClip;
    }
}