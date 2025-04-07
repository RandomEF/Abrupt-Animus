using UnityEngine;

public class SelectSlot : MonoBehaviour
{
    public string dataPath;
    public void SelectThisSlot()
    {
        GameObject obj = GameObject.Find("Load"); // Find the load button
        SlotSelection button = obj.GetComponent<SlotSelection>(); // Get the selection script on it
        button.selected = gameObject; // Set this object as the most recently selected
    }
}
