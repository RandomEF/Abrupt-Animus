using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public float MaxHealth { get; set;}
    public float Health {get; set;}
    public float DefenseMultiplier { get; set;}
    public float HealingMultiplier { get; set;}

    private void Start() {
        Health = MaxHealth;    
    }

    public void Damage(float attack){
        Health -= attack * (1 - DefenseMultiplier);
        Health = Mathf.Clamp(Health, 0, MaxHealth);
        if (Health < 0) {
            Kill();
        }
    }

    public void Healing(float heal){
        Health += heal * (1 + HealingMultiplier);
        Health = Mathf.Clamp(Health, 0, MaxHealth);
    }
    public void Kill(){
        Destroy(gameObject);
    }
}
