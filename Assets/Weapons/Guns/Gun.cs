using UnityEngine;

public class Gun : Weapon
{
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform barrel;
    public override string WeaponType { get => "Gun";} // Reassigns the weapons ID to Gun

    override public void Fire(){ // Instantiates a bullet at the gun traveling outwards
        Instantiate(bullet, barrel.transform.position, barrel.rotation);
        Debug.Log("Fired " + gameObject.name);
    }
}
