using UnityEngine;

public class Drone : EnemyEntity
{
    public override float BaseMovementAcceleration => 12f;
    public override float MaxMoveSpeed => 10f;
    public override float FarTargetRange => 25f;
    public override float NearTargetRange => 5f;
    public override float DetectionRange => 30f;
    protected override float frictionMultiplier => 0.5f;

    public override void Start() {
        Health = MaxHealth;
        rb = GetComponent<Rigidbody>();
        playerLayer = LayerMask.GetMask("Player");
    }
    protected override Vector3 Move(){
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

        return targetDirection -= targetDirection * frictionMultiplier;
        //rb.AddForce(targetDirection * Time.deltaTime, ForceMode.VelocityChange);
    }
    protected override void ApplyMovement(Vector3 target)
    {
        float movementX = PIDUpdate(Time.fixedDeltaTime, rb.transform.position.x, target.x, ref lastPosition.x, ref lastError.x, ref storedIntegral.x);
        float movementY = PIDUpdate(Time.fixedDeltaTime, rb.transform.position.y, target.y, ref lastPosition.y, ref lastError.y, ref storedIntegral.y);
        float movementZ = PIDUpdate(Time.fixedDeltaTime, rb.transform.position.z, target.z, ref lastPosition.z, ref lastError.z, ref storedIntegral.z);
        Vector3 movementTotal = new Vector3(movementX, movementY, movementZ);
        Debug.DrawRay(rb.transform.position, new Vector3(movementX, 0f, 0f).normalized, Color.red);
        Debug.DrawRay(rb.transform.position, new Vector3(0f, movementY, 0f).normalized, Color.green);
        Debug.DrawRay(rb.transform.position, new Vector3(0f, 0f, movementZ).normalized, Color.blue);
        Debug.DrawRay(rb.transform.position, movementTotal.normalized, Color.black);
        rb.AddForce(movementTotal);
    }
    protected override void RotateToTarget(Transform target){
        Vector3 targetDirection = (target.transform.position - rb.transform.position).normalized;
        Quaternion dirInQuaternion = Quaternion.LookRotation(targetDirection);
        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, dirInQuaternion, RotationSpeed * Time.deltaTime);
    }
}
