using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.UIElements.Experimental;

public class PlayerGunInteraction : MonoBehaviour
{
    public Camera playerHead;
    public Transform weaponHold;
    public Transform weaponStow;
    public GameObject manager;
    public List<GameObject> weaponSlots = new List<GameObject>(); // Maybe reimplement as a list, so that scrolling works
    public int activeWeaponSlot = 0;
    private PlayerInputs playerInputs;
    /*
    This script should handle swapping between weapon slots, picking up weapons, firing the weapons
    */
    private void Start()
    {
        playerInputs = manager.GetComponent<PlayerManager>().inputs;
        playerInputs.Player.Fire.performed += Fire;
        playerInputs.Player.SwapWeapon.performed += SwapWeapon;
        playerInputs.Player.WeaponSlot1.performed += (i) => SwapSpecificSlot(1);
        playerInputs.Player.WeaponSlot2.performed += (i) => SwapSpecificSlot(2);
        playerInputs.Player.WeaponSlot3.performed += (i) => SwapSpecificSlot(3);
        playerInputs.Player.Interact.performed += Interact;
        foreach(GameObject weapon in weaponSlots){
            if (weapon != null){
                weapon.SetActive(false);
                weapon.GetComponent<Rigidbody>().isKinematic = true;
                weapon.GetComponent<Rigidbody>().detectCollisions = false;
                weapon.transform.position = weaponStow.position; 
            }
        }
        weaponSlots[0].SetActive(true);
        weaponSlots[0].transform.position = weaponHold.position;
    }
    private GameObject SelectFireWeapon()
    {
        return weaponSlots[activeWeaponSlot];
    }
    private int Wrap(int value, List<GameObject> list)
    {
        return ((value % list.Count) + list.Count) % list.Count;
    }

    private void Fire(InputAction.CallbackContext inputType)
    {
        GameObject weapon = SelectFireWeapon();
        weapon.GetComponent<Gun>().Fire();
    }

    private void SwapWeapon(InputAction.CallbackContext inputType)
    {
        int direction = (int)inputType.ReadValue<float>();
        weaponSlots[activeWeaponSlot].SetActive(false);
        weaponSlots[activeWeaponSlot].transform.position = weaponStow.position;
        activeWeaponSlot = Wrap(activeWeaponSlot + direction, weaponSlots);
        if (SelectFireWeapon() == null)
        {
            activeWeaponSlot = Wrap(activeWeaponSlot - direction, weaponSlots);
        }
        weaponSlots[activeWeaponSlot].transform.position = weaponHold.position;
        weaponSlots[activeWeaponSlot].SetActive(true);
    }
    private void SwapSpecificSlot(int slot){
        if (slot > weaponSlots.Count){
            return;
        }
        if (weaponSlots[slot] == null){
            return;
        }
        weaponSlots[activeWeaponSlot].SetActive(false);
        weaponSlots[activeWeaponSlot].transform.position = weaponStow.position;
        activeWeaponSlot = slot;
        weaponSlots[activeWeaponSlot].transform.position = weaponHold.position;
        weaponSlots[activeWeaponSlot].SetActive(true);
    }
    private int GetSwapSlot(out bool wasUsed)
    {
        if (weaponSlots.Count < 4){
            wasUsed = false;
            return activeWeaponSlot + 1;
        }
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i] == null)
            {
                wasUsed = false;
                return i; // Find the empty slot and return it
            }
        }
        wasUsed = true;
        return activeWeaponSlot; // Return the current slot if there isn't an empty one
    }
    private void Interact(InputAction.CallbackContext inputType)
    {
        RaycastHit hit;
        Debug.DrawLine(playerHead.transform.position, playerHead.transform.position + playerHead.transform.rotation * Vector3.forward, Color.red, 1f);
        if (Physics.Raycast(origin: playerHead.transform.position, direction: playerHead.transform.rotation * Vector3.forward, hitInfo: out hit)){
            Debug.Log("Raycast hit " + hit.collider.gameObject.name);
            if (hit.collider.gameObject.tag == "Interact")
            {
                // Do interact code
            }
            else if (hit.collider.gameObject.tag == "Weapon")
            {
                Debug.Log("weapon interaction");
                bool wasUsed;
                activeWeaponSlot = GetSwapSlot(out wasUsed);
                if (wasUsed){
                    weaponSlots[activeWeaponSlot].transform.SetParent(null, true);
                    weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().isKinematic = false;
                    weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().detectCollisions = true;
                }
                hit.transform.SetParent(playerHead.transform, true);
                weaponSlots[activeWeaponSlot] = hit.collider.gameObject;
                weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().isKinematic = true;
                weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().detectCollisions = false;
                weaponSlots[activeWeaponSlot].transform.position = weaponHold.position;
                weaponSlots[activeWeaponSlot].transform.rotation = weaponHold.rotation;
                //TODO set the rotation as well
            }
        }
    }
}
