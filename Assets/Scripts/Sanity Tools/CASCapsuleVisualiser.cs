using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideAndSlideCapsuleVisualiser : MonoBehaviour
{
    public Rigidbody player;
    public GameObject playerObj;
    public CapsuleCollider playerCollider;
    public float playerHeight;
    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere((player.position + new Vector3(0f, playerCollider.height*playerHeight/2f - playerCollider.radius)), playerCollider.radius);
        Gizmos.DrawWireSphere((player.position - new Vector3(0f, playerCollider.height*playerHeight/2f - playerCollider.radius)), playerCollider.radius);
    }
}
