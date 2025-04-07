using UnityEngine;

public class BodyDetection : MonoBehaviour
{
    public PlayerMovement bodyScript;

    private void OnCollisionStay(Collision collision)
    { // When the body touches something
        bodyScript.CollisionDetected(collision); // Send all collision data to the movement script
    }
    private void OnCollisionExit(Collision collision)
    { // When the body stops touching something
        bodyScript.CollisionDetected(collision); // Send all collision data to the movement script
    }
}
