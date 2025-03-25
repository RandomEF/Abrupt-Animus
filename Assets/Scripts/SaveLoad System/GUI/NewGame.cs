using UnityEngine;
using UnityEngine.UI;

public class NewGame : MonoBehaviour
{
    private void Start()
    {
        Button button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(NewGameButton);
    }
    public void NewGameButton()
    {
        PlayerManager.Instance.saveData = SaveSystem.NewSave();
    }
}
