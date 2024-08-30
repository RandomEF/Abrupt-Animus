using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject player;
    public PlayerInputs inputs;
    public SaveData saveData;

    void Awake()
    {
        inputs = new PlayerInputs();
        inputs.Player.Enable(); // Enabling only the Player input map
    }
    void Start(){
        saveData = new SaveData();
        saveData.playerData.player = player;
    }
    void SaveGame(){
        SaveSystem.Save(saveData, "Manual", 1);
    }
}
