using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour
{
    private PlayerManager manager;
    private PlayerInputs playerInputs;
    bool paused = false;

    void Start()
    {
        manager = PlayerManager.Instance; // Get the game manager
        playerInputs = manager.inputs; // Get the player's inputs
        playerInputs.Player.Pause.performed += Pause; // Run Pause() when the player pauses the game
        playerInputs.Menu.Exit.performed += Pause; // Run Pause() when the player unpauses the game
    }

    private void Pause(InputAction.CallbackContext context)
    {
        if (paused)
        { // If the game is paused
            playerInputs.Player.Enable(); // Enable the normal inputs
            playerInputs.Menu.Disable(); // Disable the menu inputs
            manager.menuManager.ChangeMenu("HUD"); // Change to the HUD
            Time.timeScale = 1; // Resume the game
            paused = false; // The game is no longer paused
        }
        else
        {
            playerInputs.Menu.Enable(); // Enable the menu inputs
            playerInputs.Player.Disable(); // Disable the normal inputs
            manager.menuManager.ChangeMenu("Pause"); // Change to the pause menu
            Time.timeScale = 0; // Pause the game
            paused = true; // The game is currently paused
        }
    }
}