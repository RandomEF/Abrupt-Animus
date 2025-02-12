using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoading : MonoBehaviour
{
    Slider progressBar;
    private void Start() {
        progressBar = transform.GetChild(0).GetComponent<Slider>();
    }

    public void Load(string sceneName, GameObject caller){
        StartCoroutine(DisplayProgress(sceneName));
    }
    
    IEnumerator DisplayProgress(string sceneName){
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        while (!load.isDone){
            float progress = Mathf.Clamp01(load.progress / 0.9f);
            progressBar.value = progress;
            yield return null;
        }
    }
}
