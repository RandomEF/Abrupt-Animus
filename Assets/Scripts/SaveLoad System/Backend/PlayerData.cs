using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PlayerData
{
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

    public PlayerData()
    {
        currentScene = "Tutorial";
        health = 100f;
        maxHealth = health;
        position = new SerVector3(0, 0.85f, 0);
        velocity = new SerVector3(0, 0, 0);
        bodyRotation = new SerQuaternion(0, 0, 0, 0);
        lookRotation = new SerQuaternion(0, 0, 0, 0);
        weaponSlots = new List<WeaponData>();
        activeWeaponSlot = 0;
        merit = 0;
        sanity = 100f;
        timeOnSave = 0;
        totalKills = 0;
    }

    public void UpdateData(GameObject player)
    {
        currentScene = SceneManager.GetActiveScene().name;
        health = player.GetComponent<PlayerEntity>().Health;
        maxHealth = player.GetComponent<PlayerEntity>().MaxHealth;
        position = player.transform.position;
        velocity = player.GetComponent<Rigidbody>().linearVelocity;
        bodyRotation = player.transform.rotation;
        lookRotation = Camera.main.transform.rotation;
        weaponSlots = new List<WeaponData>();
        foreach (GameObject weapon in player.GetComponent<PlayerGunInteraction>().weaponSlots)
        {
            Weapon weaponScript = weapon.GetComponent<Weapon>();
            weaponSlots.Add(new WeaponData(weaponScript.WeaponType, weaponScript.TotalAmmo, weaponScript.AmmoInClip, weaponScript.ClipCapacity));
        }
        activeWeaponSlot = player.GetComponent<PlayerGunInteraction>().activeWeaponSlot;
        merit = PlayerManager.Instance.merit;
        sanity = PlayerManager.Instance.sanity;
        timeOnSave += Time.realtimeSinceStartup;
        totalKills = PlayerManager.Instance.totalKills;
    }
}

[System.Serializable]
public class WeaponData
{
    public string name;
    public int totalAmmo;
    public int ammoInClip;
    public int clipCapacity;
    public WeaponData(string name, int totalAmmo, int ammoInClip, int clipCapacity)
    {
        this.name = name;
        this.totalAmmo = totalAmmo;
        this.ammoInClip = ammoInClip;
        this.clipCapacity = clipCapacity;
    }
}