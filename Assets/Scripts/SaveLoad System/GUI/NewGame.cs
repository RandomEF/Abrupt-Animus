using UnityEngine;
using UnityEngine.UI;

public class NewGame : MonoBehaviour
{
    private void Start()
    {
        Button button = gameObject.GetComponent<Button>(); // Get the button this is attached on
        button.onClick.AddListener(NewGameButton); // Listen for the player clicking it
    }
    public void NewGameButton()
    { // Make a new save
        PlayerManager.Instance.saveData = SaveSystem.NewSave();
    }
}
