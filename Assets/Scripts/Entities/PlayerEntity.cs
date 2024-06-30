public class PlayerEntity : Entity
{
    public void UpgradeHealth(float extra){
        MaxHealth += extra;
    }
    private void Update() {
    }
}
