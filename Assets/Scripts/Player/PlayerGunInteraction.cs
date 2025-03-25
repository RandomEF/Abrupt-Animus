using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGunInteraction : MonoBehaviour
{
    Camera playerHead;
    public Transform weaponHold;
    public Transform weaponStow;
    // private Vector3 difference;
    public PlayerManager manager;
    public List<GameObject> weaponSlots = new List<GameObject>(); // Maybe reimplement as a list, so that scrolling works
    public int activeWeaponSlot = 0;
    public int maxWeapons = 4;
    private PlayerInputs playerInputs;
    private PlayerLook lookScript;
    private LayerMask notPlayerLayer;

    [Header("Zoom")]
    public float fov = 60;
    public float zoomFov = 20;

    [SerializeField] private float rotationSpeed = 10f;

    /*
    This script should handle swapping between weapon slots, picking up weapons, firing the weapons
    */
    private void Start()
    {
        manager = PlayerManager.Instance;
        playerInputs = manager.inputs;
        playerHead = Camera.main;
        lookScript = Camera.main.GetComponent<PlayerLook>();
        notPlayerLayer = ~LayerMask.GetMask("Player");
        playerInputs.Player.Fire.performed += Fire;
        playerInputs.Player.Reload.performed += Reload;
        playerInputs.Player.SwapWeapon.performed += SwapWeapon;
        playerInputs.Player.WeaponSlot1.performed += (i) => SwapSpecificSlot(1);
        playerInputs.Player.WeaponSlot2.performed += (i) => SwapSpecificSlot(2);
        playerInputs.Player.WeaponSlot3.performed += (i) => SwapSpecificSlot(3);
        playerInputs.Player.Interact.performed += Interact;

        foreach (GameObject weapon in weaponSlots)
        {
            if (weapon != null)
            {
                weapon.SetActive(false);
                weapon.GetComponent<Rigidbody>().isKinematic = true;
                weapon.GetComponent<Rigidbody>().detectCollisions = false;
                weapon.transform.position = weaponStow.position;
            }
        }
        if (weaponSlots.Count > 0)
        {
            weaponSlots[0].SetActive(true);
            weaponSlots[0].transform.position = weaponHold.position;
            // difference = playerHead.transform.position - weaponHold.position;
        }
    }
    private void Update()
    {
        Zoom(); // Check the fov state
        if (activeWeaponSlot >= maxWeapons || activeWeaponSlot < 0)
        { // if the current weapon slot is outside the maximum weapon range
            activeWeaponSlot = Wrap(activeWeaponSlot, weaponSlots); // Wrap it appropriately
        }
        if (weaponSlots.Count > 0)
        { // If there is a weapon active
            RaycastHit playerHit;
            Quaternion lookRotation = playerHead.transform.rotation; // temporarily store the current facing rotation
            if (Physics.Raycast( // if this raycast hits something
                origin: playerHead.transform.position,
                direction: playerHead.transform.rotation * Vector3.forward,
                hitInfo: out playerHit,
                layerMask: notPlayerLayer,
                maxDistance: Mathf.Infinity
                ))
            {
                Vector3 pointDirection = (playerHit.point - weaponSlots[activeWeaponSlot].transform.position).normalized; // find the direction the weapon needs to point towards
                lookRotation = Quaternion.LookRotation(pointDirection); // Store the required rotation to reach the target
            }
            weaponSlots[activeWeaponSlot].transform.rotation = Quaternion.Slerp(weaponSlots[activeWeaponSlot].transform.rotation, lookRotation, rotationSpeed * Time.deltaTime); // smoothly turn the weapon towards the
        }
    }
    private GameObject SelectFireWeapon()
    {
        return weaponSlots[activeWeaponSlot];
    }
    private int Wrap(int value, List<GameObject> list)
    {
        return ((value % list.Count) + list.Count) % list.Count; // Wrap both around negatively and positively
    }
    private void Fire(InputAction.CallbackContext inputType)
    {
        if (weaponSlots.Count > 0)
        { // Checks that there is a weapon to fire
            GameObject weapon = SelectFireWeapon();
            weapon.GetComponent<Gun>().Fire();
        }
    }
    private void Reload(InputAction.CallbackContext inputType)
    {
        if (weaponSlots.Count > 0)
        { //  Checks that there is a weapon to reload
            GameObject weapon = SelectFireWeapon();
            weapon.GetComponent<Gun>().Reload();
        }
    }
    private void Zoom()
    {
        if (playerInputs.Player.Zoom.inProgress)
        {
            Camera.main.fieldOfView = zoomFov;
            lookScript.sensitivity = lookScript.zoomSensitivity;
        }
        else
        {
            Camera.main.fieldOfView = fov;
            lookScript.sensitivity = lookScript.normalSensitivity;
        }
    }
    private void SwapWeapon(InputAction.CallbackContext inputType)
    {
        if (weaponSlots.Count > 0)
        {
            int direction = (int)inputType.ReadValue<float>();
            weaponSlots[activeWeaponSlot].SetActive(false);
            weaponSlots[activeWeaponSlot].transform.position = weaponStow.position;
            activeWeaponSlot = Wrap(activeWeaponSlot + direction, weaponSlots);
            weaponSlots[activeWeaponSlot].transform.position = weaponHold.position;
            weaponSlots[activeWeaponSlot].SetActive(true);
        }
    }
    private void SwapSpecificSlot(int slot)
    {
        if (slot > weaponSlots.Count)
        {
            return;
        }
        if (weaponSlots[slot] == null)
        {
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
        if (weaponSlots.Count < maxWeapons)
        {
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
        if (Physics.Raycast(origin: playerHead.transform.position, direction: playerHead.transform.rotation * Vector3.forward, hitInfo: out hit))
        {
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
    public void AddWeapon(GameObject hit)
    {
        Debug.Log("weapon interaction");
        bool wasUsed;
        int temp = GetSwapSlot(out wasUsed);
        if (wasUsed)
        { // remove the current weapon
            weaponSlots[activeWeaponSlot].transform.SetParent(null, true);
            weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().isKinematic = false;
            weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().detectCollisions = true;
        }
        else if (!weaponSlots.Contains(hit))
        { // add the new weapon
            weaponSlots.Add(hit);
        }
        activeWeaponSlot = temp;
        // Format the new weapon to the appropriate locations and properties
        weaponSlots[activeWeaponSlot].transform.SetParent(playerHead.transform, true);
        weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().isKinematic = true;
        weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().detectCollisions = false;
        weaponSlots[activeWeaponSlot].transform.position = weaponHold.position;
        weaponSlots[activeWeaponSlot].transform.rotation = weaponHold.rotation;
    }
    private void InteractHit(GameObject hit)
    {
        hit.GetComponent<Interactable>().Interact();
    }
}
