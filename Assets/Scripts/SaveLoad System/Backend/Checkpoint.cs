using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public PlayerManager manager;

    void Start()
    {
        manager = PlayerManager.Instance; // Get the game manager
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        { // If the player has touched the checkpoint
            manager.SaveGame("Auto"); // Create an automatic save
            Debug.Log("Saved");
        }
    }
}
