using UnityEngine;

public class Spike : MonoBehaviour
{
    public float damage = 10;
    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            GameObject hit = contact.otherCollider.gameObject;
            Entity entityClass = hit.GetComponent<Entity>();
            if (entityClass != null)
            {
                hit.SendMessage("Damage", damage);
            }
        }
    }
}
