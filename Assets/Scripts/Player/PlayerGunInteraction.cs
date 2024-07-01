using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGunInteraction : MonoBehaviour
{
    public GameObject manager;
    public GameObject weaponSlot0 = null; // Mandatory melee slot
    public GameObject weaponSlot1 = null;
    public GameObject weaponSlot2 = null;
    public GameObject weaponSlot3 = null;
    public int activeWeaponSlot = 0;
    private PlayerInputs playerInputs;
    /*
    This script should handle swapping between weapon slots, picking up weapons, firing the weapons
    */
    private void Start() {
        playerInputs = manager.GetComponent<PlayerManager>().inputs;
        playerInputs.Player.Fire.performed += Fire;
    }
    private GameObject SelectFireWeapon(){
        switch(activeWeaponSlot){
            case 0:
                return weaponSlot0;
            case 1:
                return weaponSlot1;
            case 2:
                return weaponSlot2;
            case 3:
                return weaponSlot3;
            default:
                return weaponSlot0;
        }
    }

    private void Fire(InputAction.CallbackContext inputType){
        GameObject weapon = SelectFireWeapon();
        weapon.GetComponent<Gun>().Fire();
    }
}
