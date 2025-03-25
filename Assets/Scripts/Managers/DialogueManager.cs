using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public Canvas subtitleCanvas;
    public TMP_Text textObj;
    public float fadeAwayTimer;
    public int maxLines;
    public List<string> lines = new List<string>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Update()
    {
        if (lines.Count == 0)
        {
            subtitleCanvas.GetComponent<CanvasGroup>().alpha = 0;
            return;
        }
        string text = "";
        foreach (string line in lines)
        {
            text += line + "\n";
        }
        textObj.text = text;
    }
    public void DisplayLine(string text)
    {
        subtitleCanvas.GetComponent<CanvasGroup>().alpha = 1;
        if (lines.Count == maxLines)
        {
            lines.Remove(lines[0]);
            StopCoroutine(Timer(text));
        }
        lines.Add(text);
        StartCoroutine(Timer(text));
    }
    private IEnumerator Timer(string text)
    {
        yield return new WaitForSeconds(fadeAwayTimer);
        lines.Remove(text);
    }
}
