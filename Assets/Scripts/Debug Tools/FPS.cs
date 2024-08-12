using UnityEngine;
using TMPro;

public class FPS : MonoBehaviour
{
    public TMP_Text text;
    int number = 0;
    float currentAvg = 0;
    public bool log;

    void Update()
    {
        float displayVal = 1F/Time.deltaTime;
        text.text = displayVal.ToString();
        if (log){
            Debug.Log(displayVal.ToString());
        }
    }

    float UpdateAverage(float currentFPS){
        number++;
        currentAvg += (currentFPS - currentAvg)/number;
        return currentAvg;
    }
}
