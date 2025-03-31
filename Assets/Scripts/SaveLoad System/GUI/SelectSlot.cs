using UnityEngine;

public class SelectSlot : MonoBehaviour
{
    public string dataPath;
    public void SelectThisSlot()
    {
        GameObject obj = GameObject.Find("Load");
        Debug.Log(obj);
        Debug.Log(obj.GetInstanceID());
        SlotSelection button = obj.GetComponent<SlotSelection>();
        Debug.Log(button);
        button.selected = gameObject;
    }
}
