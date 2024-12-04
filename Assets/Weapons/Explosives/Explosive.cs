using UnityEngine;

public class Explosive: Weapon
{
    public override string WeaponType => "Explosive";
    public override int AmmoInClip { get; set;}
    protected float maxDamage = 80;

    public override void Fire()
    {
        // overlap sphere, raycast if accessible, damage based on distance
    }
}
