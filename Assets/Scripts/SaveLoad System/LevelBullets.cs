[System.Serializable]
public class LevelBullets
{
    public BulletSaveData[] level1;
    public LevelBullets(){
        level1 = new BulletSaveData[50];
    }
    public void UpdateData(){
    }
}
[System.Serializable]
public class BulletSaveData{
    public SerVector3 position;
    public int timePlacedIntoSave;
    public BulletSaveData(){
        position = new SerVector3(0,0,0);
        timePlacedIntoSave = -1;
    }
    public BulletSaveData(SerVector3 position, int currentTime){
        this.position = position;
        timePlacedIntoSave = currentTime;
    }
}
