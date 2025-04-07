using UnityEngine;

public class Drone : EnemyEntity
{
    public override float MaxMoveSpeed => maxMoveSpeed;
    public override float FarTargetRange => 25f;
    public override float NearTargetRange => 5f;
    public override float DetectionRange => 30f;
    public override int merit => 15;

    public override void Start()
    {
        Health = MaxHealth;
        rb = GetComponent<Rigidbody>();
        playerLayer = LayerMask.GetMask("Player");
    }
    protected override void FixedUpdate()
    {
        EnemyLoop();
    }
    protected override void EnemyLoop()
    {
        /* search for enemies and move around
        If enemies found switch to combat and alert manager
        */
        velocity = rb.linearVelocity; // Store a copy of the current velocity for debugging
        Debug.DrawRay(rb.transform.position, rb.transform.rotation * Vector3.forward, Color.yellow);
        if (state != EnemyManager.MemberState.None)
        {
            // Required for the first frame when no target is there or other cases
            if (target != null)
            {
                RotateToTarget(target.transform); // Rotate towards the target
            }
            if (state == EnemyManager.MemberState.Combat)
            {
                // Move towards and fire
                Vector3 movementTarget = target.transform.position - (target.transform.position - rb.transform.position).normalized * midRange;
                ApplyMovement(movementTarget); // Calculate the new movement to the target
                Combat(); // Check if the weapon can be fired
            }
            else if (state == EnemyManager.MemberState.Searching)
            {
                // Search target is reached if they are within 1 metre of it
                if ((searchingTarget - rb.transform.position).magnitude < 1 || !searchTargetSet)
                {
                    SelectTarget(); // Select the next searching target
                    ResetInitialisation(); // Skip calculating the derivative for this frame
                    searchTargetSet = true; // Skips a potential problem on the first frame
                }
                ApplyMovement(searchingTarget); // Calculate the new movement to the target
                Searching(); // Search at the new position
            }
        }
        lastPosition = rb.transform.position; // Store a copy of the current position
    }

    protected override void ApplyMovement(Vector3 target)
    {
        float movementX = PIDUpdate(Time.fixedDeltaTime, rb.transform.position.x, target.x, ref lastPosition.x, ref lastError.x, ref storedIntegral.x);
        float movementY = PIDUpdate(Time.fixedDeltaTime, rb.transform.position.y, target.y, ref lastPosition.y, ref lastError.y, ref storedIntegral.y);
        float movementZ = PIDUpdate(Time.fixedDeltaTime, rb.transform.position.z, target.z, ref lastPosition.z, ref lastError.z, ref storedIntegral.z);
        Vector3 movementTotal = new Vector3(movementX, movementY, movementZ); // Combine the movement
        if (movementTotal.magnitude > MaxMoveSpeed)
        { // Limit the movement
            movementTotal *= MaxMoveSpeed / movementTotal.magnitude; // set movementTotal's magnitude to the maxMoveSpeed
        }
        Debug.DrawRay(rb.transform.position, new Vector3(movementX, 0f, 0f).normalized, Color.red); // X direction
        Debug.DrawRay(rb.transform.position, new Vector3(0f, movementY, 0f).normalized, Color.green); // Y direction
        Debug.DrawRay(rb.transform.position, new Vector3(0f, 0f, movementZ).normalized, Color.blue); // Z direction
        Debug.DrawRay(rb.transform.position, movementTotal.normalized, Color.black); // Total direction
        rb.AddForce(movementTotal, ForceMode.VelocityChange); // Apply movement
    }

    protected override void RotateToTarget(Transform target)
    {
        Vector3 targetDirection = target.transform.position - weapon.transform.position; // Get the direction from the weapon to the target
        //weapon.transform.rotation = Quaternion.Slerp(weapon.transform.rotation, Quaternion.LookRotation(targetDirection.normalized, Vector3.up), RotationSpeed * Time.fixedDeltaTime);
        Quaternion dirInQuaternion = Quaternion.LookRotation(targetDirection.normalized, Vector3.up); // Rotation for the next step 
        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, dirInQuaternion, RotationSpeed * Time.fixedDeltaTime); // Rotate the enemy so that the gun is in position to fire the player
    }
}
