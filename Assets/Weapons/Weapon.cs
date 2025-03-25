using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    virtual public string WeaponType { get; }
    virtual public int TotalAmmo { get; set; }
    virtual public int AmmoInClip { get; set; }
    virtual public int ClipCapacity { get; set; }
    abstract public void Fire();
}
