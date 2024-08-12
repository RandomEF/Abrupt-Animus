using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGroundedWithContacts : MonoBehaviour
{
    GameObject player;
    PlayerMovementController movementController;
    // Start is called before the first frame update
    void Start()
    {
        movementController = player.GetComponent<PlayerMovementController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
//     private void OnDrawGizmos() {
//         foreach(ContactPoint contact in movementController.collisions.contacts){
//             Gizmos.DrawWireSphere(contact.point, 0.1f);
//         }
//     }
}
