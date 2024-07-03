using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform barrel;

    public void Fire(){
        Instantiate(bullet, barrel.transform.position, barrel.rotation);
        Debug.Log("Fired " + gameObject.name);
    }
}
