using UnityEngine;
using TMPro;

public class PosAndVelocity : MonoBehaviour
{
    public TMP_Text text;
    public Rigidbody player;

    void Update()
    {
        text.text = $"Pos: x {player.transform.position.x}, y {player.transform.position.y}, z {player.transform.position.z}<br>Velocity: {player.linearVelocity}";
    }
}
