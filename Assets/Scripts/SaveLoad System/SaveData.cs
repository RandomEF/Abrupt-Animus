[System.Serializable]
public class SaveData
{
    public PlayerData playerData;
    public WorldFlags worldFlags;
    public LevelBullets levelBullets;
    public LevelHighscores levelHighscores;

    public SaveData(){
        playerData = new PlayerData();
        worldFlags = new WorldFlags();
        levelBullets = new LevelBullets();
        levelHighscores = new LevelHighscores();
    }
    public void UpdateSaveData() {
        playerData.UpdateData();
        worldFlags.UpdateData();
        levelBullets.UpdateData();
        levelHighscores.UpdateData();
    }
}
