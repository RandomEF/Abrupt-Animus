using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerLook : MonoBehaviour
{
    public PlayerManager manager;
    private InputAction lookAction;
    [SerializeField] private Rigidbody playerBody;
    private float verticalRotation = 0f;
    public float sensitivity = 1;
    public float normalSensitivity = 1;
    public float zoomSensitivity = 0.5f;

    void Start()
    {
        manager = PlayerManager.Instance; // Fetch a reference to the game manager
        lookAction = manager.inputs.Player.Look; // Fetch a reference to the looking input for quick access
        //gameObject.transform.SetParent(playerBody.transform, true);
        SceneManager.sceneLoaded += SetCursorLock; // Run SetCursorLuck() every time a scene loads
        //Cursor.lockState = CursorLockMode.Locked;
        SetCursorLock(SceneManager.GetActiveScene(), new LoadSceneMode()); //
    }

    // Update is called once per frame
    void Update()
    {
        Look();
    }
    void Look()
    {
        Vector2 lookMotion = lookAction.ReadValue<Vector2>() * sensitivity / 20;
        playerBody.rotation = playerBody.rotation * Quaternion.Euler(Vector3.up * lookMotion.x);

        verticalRotation -= lookMotion.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
    public void SetCursorLock(Scene scene, LoadSceneMode _)
    {
        if (manager.GetMoveability(scene.name) && !MenuManager.Instance.GetCurrentCanvasInteractability())
        {
            //Debug.Log(manager.GetMoveability(scene.name));
            //Debug.Log(MenuManager.Instance.GetCurrentCanvasInteractability());
            Cursor.lockState = CursorLockMode.Locked;
            Debug.Log("Locked cursor.");
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Debug.Log("Unlocked cursor.");
        }
    }
}
