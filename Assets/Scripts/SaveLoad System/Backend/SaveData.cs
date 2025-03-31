using UnityEngine;

[System.Serializable]
public class SaveData
{
    public PlayerData playerData;
    public WorldFlags worldFlags;
    public LevelHighscores levelHighscores;
    public LevelBullets levelBullets;

    public SaveData()
    { // Initialises new save data with the default values
        playerData = new PlayerData();
        worldFlags = new WorldFlags();
        levelHighscores = new LevelHighscores();
        levelBullets = new LevelBullets();
    }
    public void UpdateSaveData(GameObject player)
    { // Update the player's save data
        playerData.UpdateData(player);
    }
}
