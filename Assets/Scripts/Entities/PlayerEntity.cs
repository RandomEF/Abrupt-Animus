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
        {
            if (asyncOp.Status == AsyncOperationStatus.Succeeded)
            {
                death = asyncOp.Result;
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
        MaxHealth += extra;
    }

    private void Update()
    {
        if (run)
        {
            slider.value = Health / MaxHealth;
            healthText.text = Health.ToString();
        }
    }

    public override void Kill()
    {
        PlayerManager.Instance.inputs.Player.Disable();
        PlayerManager.Instance.PlayerDeath();
        MenuManager.Instance.ChangeMenu("Death");
        AudioManager.Instance.PlaySFXClip(death, transform, 1f);
    }
}
