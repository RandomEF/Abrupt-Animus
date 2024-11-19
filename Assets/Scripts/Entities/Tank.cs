using UnityEngine;

public class Tank : EnemyEntity
{
    public override float BaseMovementAcceleration => 3f;
    public override float MaxMoveSpeed => 1f;
    // Update is called once per frame
    void Update()
    {
        
    }
}
