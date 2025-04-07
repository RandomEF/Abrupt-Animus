using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public (GameObject, MemberState)[] squad; // Array of every sqad member and their state
    public enum MemberState
    {
        None,
        Searching,
        Combat,
        Dead
    }

    public void MemberDeath(GameObject member)
    {
        for (int i = 0; i < squad.Length; i++)
        { // Go through every member in the squad
            if (squad[i].Item1 == member)
            { // If the currently selected member is the one that died
                squad[i].Item2 = MemberState.Dead; // Mark it interally as dead
                break;
            }
        }
    }
    public void MemberStateChange(MemberState state)
    { // Whenever a member changes state, alert the other members

    }
}
