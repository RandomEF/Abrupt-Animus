[System.Serializable]
public class LevelBullets
{
    public BulletSaveData[] level1;
    public LevelBullets()
    {
        level1 = new BulletSaveData[50]; // Each level can hold a maximum of 50 bullets
    }
}
[System.Serializable]
public class BulletSaveData
{
    public SerVector3 position;
    public int timePlacedIntoSave;
    public BulletSaveData()
    { // Create empty bullet data
        position = new SerVector3(0, 0, 0);
        timePlacedIntoSave = -1;
    }
    public BulletSaveData(SerVector3 position, int currentTime)
    { // Set the data of the bullets
        this.position = position;
        timePlacedIntoSave = currentTime;
    }
}
