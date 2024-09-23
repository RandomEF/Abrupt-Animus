using UnityEngine;
using UnityEngine.UI;

public class NewGame : MonoBehaviour
{
    public PlayerManager manager;
    private void Start() {
        Button button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(NewGameButton);
    }
    public void NewGameButton(){
        manager.saveData = SaveSystem.NewSave();
    }
}
