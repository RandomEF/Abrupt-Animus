using UnityEngine;

public class Gun : Weapon
{
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform barrel;
    public override string WeaponType { get => "Gun"; } // Reassigns the weapon's ID to Gun
    public override int ClipCapacity => 10;
    public override int AmmoInClip => 10;
    public override int TotalAmmo => 100;

    override public void Fire()
    { // Instantiates a bullet at the gun's barrel traveling outwards
        if (AmmoInClip > 0)
        {
            Instantiate(bullet, barrel.transform.position, barrel.rotation);
            Debug.Log("Fired " + gameObject.name);
            TotalAmmo -= 1;
            AmmoInClip -= 1;
        }
        else
        {
            Reload();
        }
    }
    public virtual void Reload()
    { // Reloads the clip
        int excess = ClipCapacity - AmmoInClip;
        Debug.Log("Attempting reload");
        if (TotalAmmo >= excess)
        { // Check if there is enough ammo to fill the clip back to full
            TotalAmmo -= excess;
            AmmoInClip = ClipCapacity;
        }
        else
        {
            AmmoInClip = TotalAmmo;
            TotalAmmo = 0;
        }
    }
}
