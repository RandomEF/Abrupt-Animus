using UnityEngine;

public class Gun : Weapon
{
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform barrel;
    public override string WeaponType { get => "Gun";}

    override public void Fire(){
        Instantiate(bullet, barrel.transform.position, barrel.rotation);
        Debug.Log("Fired " + gameObject.name);
    }
}
