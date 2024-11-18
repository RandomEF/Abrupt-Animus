using UnityEngine;

public class NewMonoBehaviourScript : EnemyEntity
{
    public override float BaseMovementAcceleration => 8f;
    public override float MaxMoveSpeed => 10f;
    // Update is called once per frame
    void Update()
    {
        
    }
}
