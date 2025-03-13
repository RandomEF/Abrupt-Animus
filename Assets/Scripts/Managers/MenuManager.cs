using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    public static Canvas main;
    public static Canvas slots;
    public static Canvas options;
    public static Canvas pause;
    public static Canvas hud;
    public static Canvas death;

    public string currentMenu;
    public string lastMenu = null;
    
    #nullable enable
    [SerializeField] public Dictionary<string, Canvas> currentCanvas = new Dictionary<string,Canvas>(){
        {"Main", main},
        {"Slots", slots},
        {"Pause", pause},
        {"HUD", hud},
        {"Options", options},
        {"Death", death}
    };
    #nullable disable

    void Awake()
    {
        if (Instance == null){
            Instance = this;
        }
    }
    void Start()
    {
        FetchCanvasi(new Scene(), new LoadSceneMode());
        SceneManager.sceneLoaded += FetchCanvasi;
    }
    public GameObject FindGameObject(string name){
        List<GameObject> list = new List<GameObject>();
        SceneManager.GetActiveScene().GetRootGameObjects(list);
        foreach(GameObject go in list){
            if (go.name == name){
                return go;
            }
        }
        return null;
    }
    public void FetchCanvasi(Scene _, LoadSceneMode __){
        foreach (string canvas in currentCanvas.Keys.ToList()){
            Debug.Log(canvas);
            currentCanvas[canvas] = FindGameObject(canvas).GetComponent<Canvas>();
        }
        DisableAll();
    }

    public void DisableAll(){
        foreach (Canvas canvas in currentCanvas.Values.ToList()){
            if (canvas != null){
                canvas.gameObject.SetActive(false);
            }
        }
    }

    public void ChangeMenu(string menu){
        if (currentCanvas != null){
            currentCanvas[currentMenu].gameObject.SetActive(false);
            lastMenu = currentMenu;
        }
        currentCanvas[menu].gameObject.SetActive(true);
        currentMenu = menu;
    }
    public void GoBack(){
        if (lastMenu != null){
            currentCanvas[lastMenu].gameObject.SetActive(true);
            currentCanvas[currentMenu].gameObject.SetActive(false);
            string temp;
            temp = currentMenu;
            currentMenu = lastMenu;
            lastMenu = temp;
        } else {
            Debug.LogWarning("No back menu option set!");
        }
    }
}
