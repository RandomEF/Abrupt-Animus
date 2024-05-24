using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGroundedTest : MonoBehaviour
{
    public Rigidbody player;
    public GameObject playerObj;
    private void OnDrawGizmos() {
        Debug.DrawLine(player.transform.position, new Vector3(player.transform.position.x, player.transform.position.y - (0.85f), player.transform.position.z));
        //Gizmos.DrawWireSphere(new Vector3(player.transform.position.x, player.transform.position.y - (0.85f), player.transform.position.z), playerObj.GetComponent<PlayerMovement>().groundDistance);
        // For some reason, in the actual code the distance to the ground has to be multiplied by 2 so that it is 0.85f * 2
    }
}
