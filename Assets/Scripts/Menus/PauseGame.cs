using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour
{
    private PlayerManager manager;
    private PlayerInputs playerInputs;
    private Canvas pauseCanvas;
    bool paused = false;

    void Start()
    {
        manager = PlayerManager.Instance;
        playerInputs = manager.inputs;
        playerInputs.Player.Pause.performed += Pause;
        playerInputs.Menu.Exit.performed += Pause;
    }

    private void Pause(InputAction.CallbackContext context)
    {
        if (manager.GetMoveability(SceneManager.GetActiveScene().name))
        {
            if (paused)
            {
                playerInputs.Player.Enable();
                playerInputs.Menu.Disable();
                manager.menuManager.ChangeMenu("HUD");
                Time.timeScale = 1;
                paused = false;
            }
            else
            {
                playerInputs.Menu.Enable();
                playerInputs.Player.Disable();
                manager.menuManager.ChangeMenu("Pause");
                Time.timeScale = 0;
                paused = true;
            }
        }
        else
        {
            if (paused)
            {
                pauseCanvas.gameObject.SetActive(false);
                paused = false;
            }
            else
            {
                pauseCanvas.gameObject.SetActive(true);
                paused = true;
            }
        }
    }
}
