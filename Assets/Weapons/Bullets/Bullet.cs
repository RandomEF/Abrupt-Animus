using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    private void Update()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(origin: transform.position, direction: transform.rotation * Vector3.up, hitInfo: out hitInfo))
        {
            GameObject hit = hitInfo.collider.gameObject;
            Entity entityClass = hit.GetComponent<Entity>();
            if (entityClass != null)
            {
                hit.SendMessage("Damage", damage);
            }
        }
        Destroy(gameObject);
    }
}
