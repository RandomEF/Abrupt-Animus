using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGunInteraction : MonoBehaviour
{
    public Camera playerHead;
    public Transform weaponHold;
    public Transform weaponStow;
    private Vector3 difference;
    public PlayerManager manager;
    public List<GameObject> weaponSlots = new List<GameObject>(); // Maybe reimplement as a list, so that scrolling works
    public int activeWeaponSlot = 0;
    private PlayerInputs playerInputs;

    [SerializeField] private float rotationSpeed = 10f;

    /*
    This script should handle swapping between weapon slots, picking up weapons, firing the weapons
    */
    private void Start()
    {
        manager = PlayerManager.Instance;
        playerInputs = manager.inputs;
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
        if (weaponSlots.Count > 0){
            weaponSlots[0].SetActive(true);
            weaponSlots[0].transform.position = weaponHold.position;
            difference = playerHead.transform.position - weaponHold.position;
        }
    }
    private void Update() {
        if (weaponSlots.Count > 0){
            RaycastHit playerHit;
            Quaternion lookRotation = playerHead.transform.rotation;
            if (Physics.Raycast(
                origin: playerHead.transform.position,
                direction: playerHead.transform.rotation * Vector3.forward,
                hitInfo: out playerHit
                )){
                Vector3 pointDirection = (playerHit.point - weaponSlots[activeWeaponSlot].transform.position).normalized;
                lookRotation = Quaternion.LookRotation(pointDirection);
            }
            weaponSlots[activeWeaponSlot].transform.rotation = Quaternion.Slerp(weaponSlots[activeWeaponSlot].transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
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
        if (weaponSlots.Count > 0){
            GameObject weapon = SelectFireWeapon();
            weapon.GetComponent<Gun>().Fire();
        }
    }
    private void SwapWeapon(InputAction.CallbackContext inputType)
    {
        if (weaponSlots.Count > 0){
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
                InteractHit(hit.collider.gameObject);
            }
            else if (hit.collider.gameObject.tag == "Weapon")
            {
                AddWeapon(hit.collider.gameObject);
            }
        }
    }
    public void AddWeapon(GameObject hit){
        Debug.Log("weapon interaction");
        bool wasUsed;
        int temp = GetSwapSlot(out wasUsed);
        if (wasUsed){
            weaponSlots[activeWeaponSlot].transform.SetParent(null, true);
            weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().isKinematic = false;
            weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().detectCollisions = true;
        } else if (!weaponSlots.Contains(hit)){
            weaponSlots.Add(hit);
        }
        activeWeaponSlot = temp;
        weaponSlots[activeWeaponSlot].transform.SetParent(playerHead.transform, true);
        weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().isKinematic = true;
        weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().detectCollisions = false;
        weaponSlots[activeWeaponSlot].transform.position = weaponHold.position;
        weaponSlots[activeWeaponSlot].transform.rotation = weaponHold.rotation;
    }
    private void InteractHit(GameObject hit){
        
    }
}
