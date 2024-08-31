using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject player;
    public PlayerInputs inputs;
    public SaveData saveData;
    public int merit = 0;
    public float sanity = 100f;
    public int totalKills;

    void Awake()
    {
        inputs = new PlayerInputs();
        inputs.Player.Enable(); // Enabling only the Player input map
        saveData = new SaveData();
    }
    void Start(){
        saveData.playerData.player = player;
    }
    void SaveGame(){
        saveData.UpdateSaveData();
        SaveSystem.Save(saveData, "Manual", 1);
    }
}
