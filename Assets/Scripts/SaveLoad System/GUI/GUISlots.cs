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
        DisplaySaves();
    }
    public void DisplaySaves()
    {
        for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
        slots = SaveSystem.ListAllSlots();
        foreach (SaveSlot slot in slots)
        {
            GameObject slotInList = Instantiate(slotPrefab, gameObject.transform);
            var image = slotInList.transform.GetChild(0);
            slotInList.transform.GetChild(1).GetComponent<SelectSlot>().dataPath = slot.dataPath;
            TMP_Text number = slotInList.transform.GetChild(2).gameObject.GetComponent<TMP_Text>();
            TMP_Text time = slotInList.transform.GetChild(3).gameObject.GetComponent<TMP_Text>();
            TMP_Text scene = slotInList.transform.GetChild(4).gameObject.GetComponent<TMP_Text>();
            var saves = slotInList.transform.GetChild(5);
            number.text = slot.number;
            time.text = TimeSpan.FromSeconds(slot.time).ToString();
            scene.text = slot.sceneName;
        }
    }
}
