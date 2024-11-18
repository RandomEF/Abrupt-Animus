using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    private void Update() {
        RaycastHit hitInfo;
        //Debug.DrawLine(transform.position, transform.position + (transform.rotation * Vector3.up), Color.red, 100f);
        if (Physics.Raycast(origin: transform.position, direction: transform.rotation * Vector3.up, hitInfo: out hitInfo)){
            GameObject hit = hitInfo.collider.gameObject;
            Entity entityClass = hit.GetComponent<Entity>();
            if (entityClass != null){
                hit.SendMessage("Damage", damage);
            }
        }
        Debug.Log("Dealt damage");
        Destroy(gameObject);
    }
}
