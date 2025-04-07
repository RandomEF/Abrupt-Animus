using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class GUISlots : MonoBehaviour
{
    [SerializeField] GameObject slotPrefab;

    private List<SaveSlot> slots;
    private void Start()
    {
        DisplaySaves(); // Display the saves when the game starts
    }
    public void DisplaySaves()
    {
        for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
        { // Destroy all existing visual slots, going backwards to avoid errors
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
        slots = SaveSystem.ListAllSlots(); // Get all the slots
        foreach (SaveSlot slot in slots)
        { // For every slot
            GameObject slotInList = Instantiate(slotPrefab, gameObject.transform); // Make a new object
            var image = slotInList.transform.GetChild(0); // Get the image
            slotInList.transform.GetChild(1).GetComponent<SelectSlot>().dataPath = slot.dataPath; // Set the datapath
            TMP_Text number = slotInList.transform.GetChild(2).gameObject.GetComponent<TMP_Text>(); // Get the number text element
            TMP_Text time = slotInList.transform.GetChild(3).gameObject.GetComponent<TMP_Text>(); // Get the time text element
            TMP_Text scene = slotInList.transform.GetChild(4).gameObject.GetComponent<TMP_Text>(); // Get the scene text element
            var saves = slotInList.transform.GetChild(5); // Get the saves button
            number.text = slot.number; // Set the slot number
            time.text = TimeSpan.FromSeconds(slot.time).ToString(); // Set the time in save
            scene.text = slot.sceneName; // set the scene name
        }
    }
}
