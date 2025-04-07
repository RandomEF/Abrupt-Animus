using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    void Start()
    {
        gameObject.tag = "Interact"; // Mark the current object as an interactable
    }
    abstract public void Interact(); // Define the required Interact method
}
