using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [JsonIgnore]
    public GameObject player;
    [JsonIgnore]
    public GameObject camera;

    public float health;
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
    public void UpdateData(){
        health = player.GetComponent<PlayerEntity>().Health;
        position = player.transform.position;
        velocity = player.GetComponent<Rigidbody>().linearVelocity;
        bodyRotation = player.transform.rotation;
        lookRotation = camera.transform.rotation;
        foreach (GameObject weapon in player.GetComponent<PlayerGunInteraction>().weaponSlots){
            //Weapon weaponScript = weapon.GetComponent<Weapon>();
            //weaponSlots.Add(new WeaponData(weapon.GetComponent<Weapon>.weaponName,))
        }
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