public class Tank : EnemyEntity
{
    public override float BaseMovementAcceleration => 3f;
    public override float MaxMoveSpeed => 1f;
    public override int value => 100;
}
