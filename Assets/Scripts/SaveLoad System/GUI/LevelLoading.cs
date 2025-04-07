using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoading : MonoBehaviour
{
    public Slider progressBar;
    public Button button;
    private AsyncOperation op;

    public void Load(string sceneName)
    {
        button.gameObject.SetActive(false); // Make sure that the button is disabled
        StartCoroutine(DisplayProgress(sceneName)); // Start loading
    }

    public void ShowButton(){
        if (button == null){ // If the button is not assigned
            button = transform.GetChild(2).GetComponent<Button>(); // Assign the button
        }
        button.gameObject.SetActive(true); // Enable the button
    }
    public void FinishLoad() {
        op.allowSceneActivation = true; // Switch to the new scene
    }

    IEnumerator DisplayProgress(string sceneName)
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName); // Begin loading the scene in the background
        op = load; // Store so that the button can use later
        load.allowSceneActivation = false; // Make sure the level does not automatically replace the one
        Debug.Log($"Started scene load of scene '{sceneName}'");
        while (progressBar.value < 1) // While the bar has not finished - makes sure that the player sees a filled in bar
        {
            float progress = Mathf.Clamp01(load.progress / 0.9f); // Get the current progress
            
            progressBar.value = progress; // Assign the progress to the bar
            yield return new WaitForEndOfFrame(); // Wait for the end of frame
            if (progressBar == null){ // If the progress bar has been dereferenced
                progressBar = transform.GetChild(1).GetComponent<Slider>(); // Get the slider object
            }
        }
        ShowButton(); // Show the button
    }
}