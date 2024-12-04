using UnityEngine;

public class Blade: Weapon
{
    [SerializeField] protected MeshCollider bladeCollider;
    public override string WeaponType => "Blade";
    public override int AmmoInClip { get => 1;}
    public override int TotalAmmo { get => 1;}
    protected float damage = 25;

    public override void Fire()
    {
        // have the collider slash and check if anything touches/passes through it
    }
}
