using UnityEngine;

public class SelectSlot : MonoBehaviour
{
    public string dataPath;
    public void SelectThisSlot()
    {
        SlotSelection button = GameObject.Find("Load").GetComponent<SlotSelection>();
        button.selected = gameObject;
    }
}
