using UnityEngine;

public class Drone : EnemyEntity
{
    public override float BaseMovementAcceleration => 12f;
    public override float MaxMoveSpeed => 20f;
    public override float FarTargetRange => 25f;
    public override float NearTargetRange => 5f;
    public override float DetectionRange => 30f;
    protected override float frictionMultiplier => 0.5f;

    public override void Start() {
        Health = MaxHealth;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        playerLayer = LayerMask.GetMask("Player");
    }
    protected override void Move(){
        Vector3 distanceToTarget = target.transform.position - gameObject.transform.position;
        Vector3 targetDirection = Vector3.forward;

        if (distanceToTarget.magnitude < NearTargetRange){
            targetDirection = Vector3.back;
        } else if (distanceToTarget.magnitude > FarTargetRange){
            targetDirection = Vector3.forward;
        } // Will auto slow down once within ranges
        targetDirection = rb.transform.rotation * targetDirection;

        float acceleration = BaseMovementAcceleration;
        if (rb.linearVelocity.magnitude > MaxMoveSpeed){
            acceleration *= rb.linearVelocity.magnitude / MaxMoveSpeed;
        }
        Vector3 direction = targetDirection * MaxMoveSpeed - rb.linearVelocity;
        targetDirection = targetDirection.normalized * acceleration;

        targetDirection -= targetDirection * frictionMultiplier;
        rb.AddForce(targetDirection * Time.deltaTime, ForceMode.VelocityChange);
    }
    protected override void RotateToTarget(Transform target){
        Vector3 targetDirection = (target.transform.position - rb.transform.position).normalized;
        Quaternion dirInQuaternion = Quaternion.LookRotation(targetDirection);
        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, dirInQuaternion, RotationSpeed * Time.deltaTime);
    }
}
