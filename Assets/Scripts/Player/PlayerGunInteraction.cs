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
        manager = PlayerManager.Instance; // Get the game manager
        playerInputs = manager.inputs; // Get the player's inputs
        playerHead = Camera.main; // Get the camera
        lookScript = Camera.main.GetComponent<PlayerLook>(); // Get the player's looking script
        notPlayerLayer = ~LayerMask.GetMask("Player"); // Get the layer that the player isn't on
        playerInputs.Player.Fire.performed += Fire; // Run Fire() when the player presses the fire key
        playerInputs.Player.Reload.performed += Reload; // Run Reload() when the player presses the reload key
        playerInputs.Player.SwapWeapon.performed += SwapWeapon; // Run SwapWeapon() when the player wants to swap the weapon
        playerInputs.Player.WeaponSlot1.performed += (i) => SwapSpecificSlot(1); // Swap to slot 1
        playerInputs.Player.WeaponSlot2.performed += (i) => SwapSpecificSlot(2); // Swap to slot 2
        playerInputs.Player.WeaponSlot3.performed += (i) => SwapSpecificSlot(3); // Swap to slot 3
        playerInputs.Player.WeaponSlot4.performed += (i) => SwapSpecificSlot(4); // Swap to slot 3
        playerInputs.Player.Interact.performed += Interact; // Run Interact() when the player presses the interact key

        foreach (GameObject weapon in weaponSlots)
        { // for every equipped weapon
            if (weapon != null)
            { // If it exists
                weapon.SetActive(false); // Disable it
                weapon.GetComponent<Rigidbody>().isKinematic = true; // Disable the physics system for it
                weapon.GetComponent<Rigidbody>().detectCollisions = false; // Disable its ability to detect collision
                weapon.transform.position = weaponStow.position; // Move the weapon to the stowed position
            }
        }
        if (weaponSlots.Count > 0)
        { // If the player has more than 1 weapon
            weaponSlots[0].SetActive(true); // Activate the first one
            weaponSlots[0].transform.position = weaponHold.position; // Place it into the held position
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
    { // Get the currently seleccted weapon
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
            GameObject weapon = SelectFireWeapon(); // Get the current weapon
            weapon.GetComponent<Gun>().Fire(); // Fire it
        }
    }
    private void Reload(InputAction.CallbackContext inputType)
    {
        if (weaponSlots.Count > 0)
        { //  Checks that there is a weapon to reload
            GameObject weapon = SelectFireWeapon(); // Get the current weapon
            weapon.GetComponent<Gun>().Reload(); // Reload it
        }
    }
    private void Zoom()
    {
        if (playerInputs.Player.Zoom.inProgress)
        { // If currently pressing zoom
            Camera.main.fieldOfView = zoomFov; // Set the camera fov to the zoomed in one
            lookScript.sensitivity = lookScript.zoomSensitivity; // Decrease the camera sensitivity
        }
        else
        {
            Camera.main.fieldOfView = fov; // Set the camera fov to the normal one
            lookScript.sensitivity = lookScript.normalSensitivity; // Return the camera sensitivity to normal
        }
    }
    private void SwapWeapon(InputAction.CallbackContext inputType)
    {
        if (weaponSlots.Count > 0)
        { // If the player has weapons
            int direction = (int)inputType.ReadValue<float>(); // Get the swap direction and cast it to an integer
            weaponSlots[activeWeaponSlot].SetActive(false); // Disable the current weapon
            weaponSlots[activeWeaponSlot].transform.position = weaponStow.position; // Move to the stowed position
            activeWeaponSlot = Wrap(activeWeaponSlot + direction, weaponSlots); // Get the new slot
            weaponSlots[activeWeaponSlot].transform.position = weaponHold.position; // Move that weapon to the held position
            weaponSlots[activeWeaponSlot].SetActive(true); // Enable this weapon
        }
    }
    private void SwapSpecificSlot(int slot)
    {
        if (slot > weaponSlots.Count)
        { // Don't swap if there isn't that many weapons to swap to
            return;
        }
        if (weaponSlots[slot] == null)
        { // If that weapon doesn't exist don't swap to it
            return;
        }
        weaponSlots[activeWeaponSlot].SetActive(false); // Disable the current weapon
        weaponSlots[activeWeaponSlot].transform.position = weaponStow.position; // Move to the stowed position
        activeWeaponSlot = slot; // Swap to the specific slot
        weaponSlots[activeWeaponSlot].transform.position = weaponHold.position; // Move that weapon to the held position
        weaponSlots[activeWeaponSlot].SetActive(true); // Enable this weapon
    }
    private int GetSwapSlot(out bool wasUsed)
    {
        if (weaponSlots.Count == 0){ // If there are no weapons
            wasUsed = false;
            return 0;
        }
        if (weaponSlots.Count < maxWeapons)
        { // If there are places left
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
        { // If the raycast collides with something
            Debug.Log("Raycast hit " + hit.collider.gameObject.name);
            if (hit.collider.gameObject.tag == "Interact")
            { // If the hit object is interactable
                InteractHit(hit.collider.gameObject); // Interact with it
            }
            else if (hit.collider.gameObject.tag == "Weapon")
            { // If the hit object is a weapon
                AddWeapon(hit.collider.gameObject); // Add the weapon to the player's slots
            }
        }
    }
    public void AddWeapon(GameObject hit)
    {
        Debug.Log("weapon interaction");
        bool wasUsed;
        int temp = GetSwapSlot(out wasUsed); // Check which slot to swap into
        if (wasUsed)
        { // remove the current weapon
            weaponSlots[activeWeaponSlot].transform.SetParent(null, true); // Free the weapon from the player
            weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().isKinematic = false; // Allow the physics system to push it
            weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().detectCollisions = true; // Allow it to detect collisions
        }
        else if (!weaponSlots.Contains(hit))
        { // add the new weapon
            weaponSlots.Add(hit);
        }
        activeWeaponSlot = temp; // Set the new weapon slot
        // Format the new weapon to the appropriate locations and properties
        weaponSlots[activeWeaponSlot].transform.SetParent(playerHead.transform, true); // Parent the weapon to the camera
        weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().isKinematic = true; // Disable the physics system from interacting with it
        weaponSlots[activeWeaponSlot].GetComponent<Rigidbody>().detectCollisions = false; // Don't allow it to detect collisions
        weaponSlots[activeWeaponSlot].transform.position = weaponHold.position; // Put the weapon in the held position
        weaponSlots[activeWeaponSlot].transform.rotation = weaponHold.rotation; // Rotate the weapon properly
    }
    private void InteractHit(GameObject hit)
    {
        hit.GetComponent<Interactable>().Interact(); // Call Interact() on the interactable
    }
}
