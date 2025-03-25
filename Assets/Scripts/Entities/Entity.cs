using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    PlayerManager gameManager;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float health = 100;
    [SerializeField] private float defenseMultiplier = 0;
    [SerializeField] private float healingMultiplier = 0;
    public float MaxHealth { get { return maxHealth; } set { maxHealth = value; } }
    public float Health { get { return health; } set { health = value; } }
    public float DefenseMultiplier { get { return defenseMultiplier; } set { defenseMultiplier = value; } }
    public float HealingMultiplier { get { return healingMultiplier; } set { healingMultiplier = value; } }
    public virtual int merit => 1;

    public virtual void Start()
    {
        Health = MaxHealth; // Sets the health to the maximum health
        gameManager = PlayerManager.Instance; // Fetches a reference to the game manager
    }

    public void Damage(float attack)
    {
        Debug.Log("Ouch " + attack);
        Health -= attack * (1 - DefenseMultiplier); // Reduce the damage taken by 1 - defenseMultiplier
        Health = Mathf.Clamp(Health, 0, MaxHealth); // Make sure health cannot go below zero or above maximum health
        if (Health <= 0)
        { // Only kills when it takes damage
            Kill(); // Kill the object
        }
    }

    public void Healing(float heal)
    {
        Health += heal * (1 + HealingMultiplier); // Increases healing based on the healing multiplier
        Health = Mathf.Clamp(Health, 0, MaxHealth); // Makes sure that the entity does not heal above what it can handle
    }
    public virtual void Kill()
    {
        Debug.Log($"Kill requested on {gameObject}");
        gameManager.AddMerit(merit); // Award merit on death
        Destroy(gameObject); // Destroy self
    }
}
