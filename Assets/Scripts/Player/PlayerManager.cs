using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerInputs inputs;

    void Awake()
    {
        inputs = new PlayerInputs();
        inputs.Player.Enable(); // Enabling only the Player input map
    }
}
