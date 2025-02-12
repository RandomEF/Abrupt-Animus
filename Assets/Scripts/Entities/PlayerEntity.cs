using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEntity : Entity
{
    public Slider slider;
    public TMP_Text healthText;
    bool run = true;

    public override void Start() {
        Health = MaxHealth;
        if (slider == null){
            Debug.Log("Health slider not assigned");
            run = false;
        } else if (healthText == null){
            Debug.Log("Health text not assigned.");
            run = false;
        }
    }

    public void UpgradeHealth(float extra){
        MaxHealth += extra;
    }
    
    private void Update() {
        if (!run){
            slider.value = Health / MaxHealth;
            healthText.text = Health.ToString();
        }
    }
}
