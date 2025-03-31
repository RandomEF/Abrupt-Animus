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
    { // Initialise PlayerData with some default values
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
        currentScene = SceneManager.GetActiveScene().name; // Get the scene the player is in
        health = player.GetComponent<PlayerEntity>().Health; // Get the player's health from their entity script
        maxHealth = player.GetComponent<PlayerEntity>().MaxHealth; // Get the player's maximum health from their entity script
        position = player.transform.position; // Get the player's position
        velocity = player.GetComponent<Rigidbody>().linearVelocity; // Get the player's current velocity from the rigidbody attached to it
        bodyRotation = player.transform.rotation; // Get the player's current rotation
        lookRotation = Camera.main.transform.rotation; // Get the camera's rotation
        weaponSlots = new List<WeaponData>(); // Clear the weapon slots
        foreach (GameObject weapon in player.GetComponent<PlayerGunInteraction>().weaponSlots) // Get a list every weapon currently equipped
        {
            Weapon weaponScript = weapon.GetComponent<Weapon>(); // Get the underlying Weapon script of the weapon
            weaponSlots.Add(new WeaponData(weaponScript.WeaponType, weaponScript.TotalAmmo, weaponScript.AmmoInClip, weaponScript.ClipCapacity)); // Make new weapon save data and add it to the list
        }
        activeWeaponSlot = player.GetComponent<PlayerGunInteraction>().activeWeaponSlot; // Save the position of the currently equipped weapon
        merit = PlayerManager.Instance.merit; // Assign merit
        sanity = PlayerManager.Instance.sanity; // Assign sanity
        timeOnSave += Time.realtimeSinceStartup; // Add on the time that the player has had the game open
        totalKills = PlayerManager.Instance.totalKills; // Get the total kills the player has done
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
    { // Copies the important weapon data into a smaller format
        this.name = name;
        this.totalAmmo = totalAmmo;
        this.ammoInClip = ammoInClip;
        this.clipCapacity = clipCapacity;
    }
}