using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    PlayerManager gameManager;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float health = 100;
    [SerializeField] private float defenseMultiplier = 0;
    [SerializeField] private float healingMultiplier = 0;
    public float MaxHealth { get {return maxHealth;} set {maxHealth = value;}}
    public float Health {get {return health;} set {health = value;}}
    public float DefenseMultiplier { get {return defenseMultiplier;} set {defenseMultiplier = value;}}
    public float HealingMultiplier { get {return healingMultiplier;} set {healingMultiplier = value;}}
    public virtual int value => 1;

    public virtual void Start(){
        Health = MaxHealth;
        gameManager = GameObject.Find("Game Manager").GetComponent<PlayerManager>();
    }

    public void Damage(float attack){
        Debug.Log("Ouch " + attack);
        Health -= attack * (1 - DefenseMultiplier);
        Health = Mathf.Clamp(Health, 0, MaxHealth);
        if (Health == 0) { // Only kills when it takes damage
            Kill();
        }
    }

    public void Healing(float heal){
        Health += heal * (1 + HealingMultiplier);
        Health = Mathf.Clamp(Health, 0, MaxHealth); // Makes sure that the entity does not heal above what it can handle
    }
    public virtual void Kill(){
        gameManager.AddMerit(value); // Award merit on death
        Destroy(gameObject); // Destroy self
    }
}
