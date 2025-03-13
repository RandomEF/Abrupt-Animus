using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public PlayerManager manager;

    void Start()
    {
        manager = PlayerManager.Instance;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player"){
            manager.SaveGame("Auto");
            Debug.Log("Saved");
        }
    }
}
