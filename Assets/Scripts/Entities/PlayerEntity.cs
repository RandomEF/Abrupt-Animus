using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PlayerEntity : Entity
{
    public Slider slider;
    public TMP_Text healthText;
    bool run = true;

    [SerializeField] private AudioClip death;

    void Awake()
    {
        Addressables.LoadAssetAsync<AudioClip>("Assets/Audio/SFX/PlayerDeath.wav").Completed += (asyncOp) =>
        { // Load the death sound effect
            if (asyncOp.Status == AsyncOperationStatus.Succeeded)
            { // If succeeded
                death = asyncOp.Result; // Assign the death sound
            }
            else
            {
                Debug.LogError("Failed to load player death audio.");
            }
        };
    }

    public override void Start()
    {
        Health = MaxHealth;
        if (slider == null)
        {
            Debug.Log("Health slider not assigned");
            run = false;
        }
        else if (healthText == null)
        {
            Debug.Log("Health text not assigned.");
            run = false;
        }
    }

    public void UpgradeHealth(float extra)
    {
        MaxHealth += extra; // Add onto the maximum health the added amount
    }

    private void Update()
    {
        if (run)
        {
            slider.value = Health / MaxHealth; // Set the slider value to be the fraction of the health out of the maximum health
            healthText.text = Health.ToString(); // Set the health text to the current remaining health
        }
    }

    public override void Kill()
    {
        PlayerManager.Instance.inputs.Player.Disable(); // Disable the Player input map
        PlayerManager.Instance.PlayerDeath(); // Tell the manager that the player has died
        MenuManager.Instance.ChangeMenu("Death"); // Change to the death menu
        AudioManager.Instance.PlaySFXClip(death, transform, 1f); // Play a death sound effect
    }
}
