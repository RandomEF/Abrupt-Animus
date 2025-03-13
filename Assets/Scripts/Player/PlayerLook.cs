using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerLook : MonoBehaviour
{
    public PlayerManager manager;
    private InputAction lookAction;
    [SerializeField] private Rigidbody playerBody;
    [SerializeField] private float mouseSensitivity;
    private float verticalRotation = 0f;

    void Start()
    {
        manager = PlayerManager.Instance;
        lookAction = manager.inputs.Player.Look;
        //gameObject.transform.SetParent(playerBody.transform, true);
        SceneManager.sceneLoaded += SetCursorLock;
        Cursor.lockState = CursorLockMode.Locked;
        SetCursorLock(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    // Update is called once per frame
    void Update()
    {
        Look();
    }
    void Look(){
        Vector2 lookMotion = lookAction.ReadValue<Vector2>() * mouseSensitivity / 20;
        playerBody.rotation = playerBody.rotation * Quaternion.Euler(Vector3.up * lookMotion.x);

        verticalRotation -= lookMotion.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
    void SetCursorLock(Scene __, LoadSceneMode _){
        if (manager.allowedToMove){
            Cursor.lockState = CursorLockMode.Locked;
        } else {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
