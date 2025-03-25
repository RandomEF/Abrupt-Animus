using System.Collections.Generic;

[System.Serializable]
public class LevelHighscores
{
    public HighscoreData level1;
    public LevelHighscores()
    {
        level1 = new HighscoreData();
    }
}

[System.Serializable]
public class HighscoreData
{
    public int time;
    public int enemiesKilled;
    public bool[] secretsFound;
    public int deaths;
    public List<string> weaponsUsed;
    public HighscoreData()
    {
        time = 0;
        enemiesKilled = 0;
        secretsFound = new bool[5];
        deaths = 0;
        weaponsUsed = new List<string>();
    }
    public HighscoreData(int time, int enemiesKilled, bool[] secretsFound, int deaths, List<string> weaponsUsed)
    {
        this.time = time;
        this.enemiesKilled = enemiesKilled;
        this.secretsFound = secretsFound;
        this.deaths = deaths;
        this.weaponsUsed = weaponsUsed;
    }
}