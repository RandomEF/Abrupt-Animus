using System;
using UnityEngine;
using TMPro;

public class SlotSelection : MonoBehaviour
{
    public GameObject selected;

    public void LoadSelected()
    {
        if (selected == null)
        { // If nothing has been selected, don't continue
            return;
        }
        SaveData data = SaveSystem.LoadSave(selected.GetComponent<SelectSlot>().dataPath); // Get the save data
        int slot = int.Parse(selected.transform.parent.GetChild(2).gameObject.GetComponent<TMP_Text>().text); // Get the slot number
        PlayerManager.Instance.SetSlotandData(slot, data); // Send the slot number and data to the game manager
        PlayerManager.Instance.LoadSlot(selected.transform.parent.GetChild(4).gameObject.GetComponent<TMP_Text>().text); // Load the game slot
    }
}
