using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public (GameObject, MemberState)[] squad;
    public enum MemberState{
        None,
        Searching,
        Combat,
        Dead
    }
    
    public void MemberDeath(GameObject member){
        for (int i = 0; i < squad.Length; i++)
        {
            if (squad[i].Item1 == member){
                squad[i].Item2 = MemberState.Dead;
                break;
            }
        }
    }
    public void MemberStateChange(MemberState state){
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
