using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SlotSelection : MonoBehaviour
{
    public GameObject selected;
    public PlayerManager manager;

    public void LoadSelected(){
        if (selected == null){
            return;
        }
        SaveData data = SaveSystem.LoadSave(selected.GetComponent<SelectSlot>().dataPath);
        int slot = Int32.Parse(selected.transform.parent.GetChild(2).gameObject.GetComponent<TMP_Text>().text);
        manager.SetSlotandData(slot, data);
        SceneManager.LoadScene(selected.transform.parent.GetChild(4).gameObject.GetComponent<TMP_Text>().text);
    }
}