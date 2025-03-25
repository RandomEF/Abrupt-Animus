using UnityEngine;

public class IsGroundedTest : MonoBehaviour
{
    public Rigidbody player;
    public CapsuleCollider playerCollider;
    public GameObject playerObj;
    private void OnDrawGizmos()
    { // Draws the overlap sphere used for depreciated ground check function
        float groundDistance = player.GetComponent<PlayerMovement>().groundDistance;
        float radius = player.transform.localScale.x * playerCollider.radius;
        float playerHeight = player.transform.localScale.y * playerCollider.height;
        Debug.DrawLine(player.transform.position, new Vector3(player.transform.position.x, player.transform.position.y - (0.85f), player.transform.position.z));
        Gizmos.DrawWireSphere(new Vector3(player.transform.position.x, player.transform.position.y - groundDistance + radius - playerHeight / 2, player.transform.position.z), radius);
        // For some reason, in the actual code the distance to the ground has to be multiplied by 2 so that it is 0.85f * 2
    }
}
