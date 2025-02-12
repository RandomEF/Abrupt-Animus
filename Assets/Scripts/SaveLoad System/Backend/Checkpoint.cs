using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public PlayerManager manager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manager = GameObject.Find("Game Manager").GetComponent<PlayerManager>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player"){
            manager.SaveGame("Auto");
            Debug.Log("Saved");
        }
    }
}
