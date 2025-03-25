using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.tag = "Interact";
    }
    abstract public void Interact();
}
