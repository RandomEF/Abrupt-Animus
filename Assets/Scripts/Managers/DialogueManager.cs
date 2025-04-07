using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public Canvas subtitleCanvas;
    public TMP_Text textObj;
    public float fadeAwayTimer = 2;
    public int maxLines = 5;
    public List<string> lines = new List<string>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        { // Assign the static reference to this instance
            Instance = this;
        }
    }
    void Update()
    {
        if (lines.Count == 0)
        { // If no lines are shown
            subtitleCanvas.GetComponent<CanvasGroup>().alpha = 0; // Hide the canvas
            return;
        }
        string text = "";
        foreach (string line in lines)
        { // For every line
            text += line + "\n"; // Construct the line that goes in the text object
        }
        textObj.text = text; // Assign the text inside
    }
    public void DisplayLine(string text)
    {
        Debug.Log($"Sent text: '{text}'");
        subtitleCanvas.GetComponent<CanvasGroup>().alpha = 1; // Show the subtitle box
        if (lines.Count >= maxLines)
        { // If too many lines are shown
            lines.Remove(lines[0]); // Remove the starting one
            StopCoroutine(Timer(lines[0])); // Stop the timer for that piece of text
        }
        lines.Add(text); // Add the new line
        StartCoroutine(Timer(text)); // Start a fade out time for that line
    }
    private IEnumerator Timer(string text)
    {
        yield return new WaitForSeconds(fadeAwayTimer); // Wait some seconds before the line is removed
        lines.Remove(text); // Remove the line
    }
}
