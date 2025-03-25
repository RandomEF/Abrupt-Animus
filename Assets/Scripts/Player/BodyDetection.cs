using UnityEngine;

public class BodyDetection : MonoBehaviour
{
    public PlayerMovement bodyScript;

    private void OnCollisionStay(Collision collision)
    {
        bodyScript.CollisionDetected(collision);
    }
    private void OnCollisionExit(Collision collision)
    {
        bodyScript.CollisionDetected(collision);
    }
}
