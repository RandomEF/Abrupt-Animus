using UnityEngine;

public class EnemyEntity : Entity
{
    public EnemyManager manager;
    public GameObject target;
    public Vector3 searchingTarget;
    [SerializeField] protected bool searchTargetSet = false;
    public EnemyManager.MemberState state;
    protected Rigidbody rb;
    public float maxMoveSpeed = 100;
    public virtual float MaxMoveSpeed => maxMoveSpeed;
    public virtual float RotationSpeed => 10f;
    public virtual float FarTargetRange => 15f;
    public virtual float NearTargetRange => 2.5f;
    protected float midRange;
    public virtual float DetectionRange => 20f;
    protected int rayCount;
    protected float rayAngle;
    protected float maxSlope;
    // should probably also have detection angle as well for long distance
    protected LayerMask playerLayer;
    //protected Vector3 groundNormal = Vector3.up;

    [SerializeField] protected GameObject weapon;
    protected Weapon weaponScript;

    [SerializeField] protected Vector3 velocity;

    public override int merit => 10;

    [SerializeField] protected float proportionalGain = 1;
    [SerializeField] protected float derivativeGain = 0.1f;
    [SerializeField] protected float integralGain = 2;
    protected Vector3 lastError = Vector3.zero;
    protected Vector3 lastPosition = Vector3.zero;
    [SerializeField] protected Vector3 storedIntegral = Vector3.one;
    [SerializeField] protected float maxStoredIntegral = 2;
    public enum DerivativeType
    {
        Velocity,
        Error,
    }
    [SerializeField] public DerivativeType derivativeType = DerivativeType.Error;
    protected bool initialised = false;

    protected virtual void Awake()
    {
        SetAngles(); // work out the amount of rays and the angle between them
        //AssignConsts(); // Fetch constats
        midRange = (FarTargetRange + NearTargetRange) / 2; // Calculate the position at which the enemy should be at
    }

    public void SetAngles()
    { // Finds the angle needed to make sure that the player is not missed between rays
        float quarterCircumference = 2 * Mathf.PI * DetectionRange * 0.25f; // Find the arc length
        rayCount = Mathf.CeilToInt(quarterCircumference); // Calculate the number of rays
        rayAngle = 90 / quarterCircumference; // Calculate the angle between each ray
    }
    public override void Start()
    {
        Health = MaxHealth;
        rb = GetComponent<Rigidbody>();
        playerLayer = LayerMask.GetMask("Player");
    }
    protected virtual void FixedUpdate()
    {
        EnemyLoop();
    }
    protected virtual void EnemyLoop()
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
    /*
    To improve the system, it should check whether the player is still visible to the enemy each time, and it should also simultaneously update the searching target.
    This will be used as the backup when the player is out of view, and will be used as the last known location of the player
    Also start a timer which will have range of how long it is (random time within) which determines when the enemy loses aggro
    Once the player has been seen and the manager has been updated, raise the awareness of the enemies in that group - might search more aggressively or detect the player faster
    Could implement a sound detection system with priority levels
    */
    protected virtual void Combat()
    { // Fire weapon
        RaycastHit hitInfo;
        if (Physics.Raycast( // Check if there the player is in the weapon's view
            origin: rb.transform.position,
            direction: weapon.transform.GetChild(0).forward,
            hitInfo: out hitInfo,
            maxDistance: DetectionRange,
            layerMask: ~LayerMask.GetMask("Weapon")
        ))
        {
            GameObject hit = hitInfo.collider.gameObject; // Grab the hit gameobject
            Debug.Log($"Enemy {gameObject.GetInstanceID()} ({gameObject.name}) facing {hit.GetInstanceID()} ({hit.gameObject.name})");
            Entity entityClass = hit.GetComponent<Entity>(); // Get the entity class from the object
            if (entityClass != null)
            { // If the object is damageable
                weaponScript.Fire(); // Fire the weapon
                Debug.Log($"Enemy {gameObject.GetInstanceID()} ({gameObject.name}) fired its {weaponScript.WeaponType}");
            }
        }
    }
    // At some point absolutely replace this with the NavMesh
    protected virtual void ApplyMovement(Vector3 target)
    { // Fetch changes in movement
        float movementX = PIDUpdate(Time.fixedDeltaTime, rb.transform.position.x, target.x, ref lastPosition.x, ref lastError.x, ref storedIntegral.x);
        float movementY = 0;
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
    //TODO Change to a overlap sphere checking angles instead and a singular downwards raycast with no max distance
    // Allows for changing of the looking direction
    protected void Searching()
    {
        for (int i = 0; i < rayCount; i++)
        {
            RaycastHit hitInfo;
            Vector3 rayDirection = Quaternion.AngleAxis(-45f + rayAngle * i, Vector3.up) * Vector3.forward; // Calculate ray direction
            rayDirection = rb.transform.rotation * rayDirection; // Make it relative to the entity
            Debug.DrawRay(rb.transform.position, rayDirection, Color.red);
            if (Physics.Raycast( // Check if a ray has intersected with a target
                origin: rb.transform.position,
                direction: rayDirection,
                hitInfo: out hitInfo,
                maxDistance: DetectionRange,
                layerMask: playerLayer
            ))
            {
                Debug.DrawRay(rb.transform.position, rayDirection, Color.green);
                state = EnemyManager.MemberState.Combat; // Set state to combat
                target = hitInfo.collider.gameObject; // Set target to the player's gameobject
                break;
            }
        }
    }
    //UpdateManager(EnemyManager.MemberState.Combat);
    protected void SelectTarget()
    {
        bool found;
        Vector3 point = GetClosestPoint(out found);
        if (!found)
        { // If piece of level geometry is not found within the entities detection range
            searchingTarget.y = rb.transform.position.y - DetectionRange; // Set the target to be below it.
            return;
        }
        Vector3 entityToPoint = rb.transform.position - point; // Calculate the distance to the found point
        if (entityToPoint.magnitude > DetectionRange)
        { // If the point is outside the range of the entity
            searchingTarget = point - entityToPoint.normalized * Random.Range(0, DetectionRange); // Set the target distance as a random distance towards the closest point.
            return;
        }

        bool accessible = false;
        while (!accessible)
        { // While the target is not accessible, generate a new point
            searchingTarget.x = Random.Range(-DetectionRange, DetectionRange); // Generate X point
            searchingTarget.y = Random.Range(-DetectionRange, DetectionRange); // Generate Y point
            searchingTarget.z = Random.Range(-DetectionRange, DetectionRange); // Generate Z point
            searchingTarget = searchingTarget + rb.transform.position; // Make the point relative to the current position of the player
            RaycastHit hitInfo;
            if (Physics.Raycast( // Check that there is no object between the target and the entity
                origin: rb.transform.position,
                direction: (searchingTarget - rb.transform.position).normalized,
                hitInfo: out hitInfo,
                maxDistance: DetectionRange,
                layerMask: playerLayer
            ))
            {
                if ((hitInfo.point - rb.transform.position).magnitude < 1f)
                { // If the point is too close to the entity
                    continue;
                }
                else
                { // The point is set
                    searchingTarget = hitInfo.point;
                    accessible = true; // Used to exit the loop
                }
            }
        }
        ResetInitialisation(); // Reset the derivative
    }
    protected Vector3 GetClosestPoint(out bool found)
    {
        Collider[] colliders = Physics.OverlapSphere(
            position: rb.transform.position,
            radius: DetectionRange * 2
        ); // Get list of colliders nearby
        found = false;
        Vector3 point = Vector3.zero;
        float distSqrd = Mathf.Infinity; // Start off with an infinite distance
        foreach (Collider collider in colliders)
        {
            found = true;
            if ((rb.transform.position - collider.transform.position).sqrMagnitude < distSqrd)
            { // if the distance to this collider is less than the current smallest
                distSqrd = (rb.transform.position - collider.transform.position).sqrMagnitude; // Set its distance as the new smallest one
                point = collider.ClosestPoint(rb.transform.position); // Get the exact distance to the point
            }
        }
        return point;
    }
    private void UpdateManager(EnemyManager.MemberState state)
    {
        // send new state to manager
    }

    protected virtual float PIDUpdate(float timeStep, float currentValue, float targetValue, ref float lastPosition, ref float lastError, ref float storedIntegral)
    {
        float error = targetValue - currentValue; // Calculate the difference
        float force = proportionalGain * error; // Calculate the P term
        float derivative = 0; // Default derivative value
        if (initialised)
        { // If the target has not recently changed
            if (derivativeType == DerivativeType.Error)
            { // If using the error to calculate the derivative
                if (error - lastError != 0)
                { // Verify that an error will not occur
                    derivative = (error - lastError) / timeStep; // Rate of change
                }
            }
            else
            { // If using the position to calculate the derivative
                if (currentValue - lastPosition != 0)
                { // Verify that an error will not occur
                    derivative = (currentValue - lastPosition) / timeStep; // Rate of change
                }
            }
        }
        else
        { // If the target has recently changed
            initialised = true; // Allows skipping of the error
        }
        derivative *= derivativeGain;
        storedIntegral = Mathf.Clamp(storedIntegral + error * timeStep, -maxStoredIntegral, maxStoredIntegral); // Make sure the stored errors do not exceed the limit
        float integral = integralGain * storedIntegral; // Calculate the I term

        lastError = error; // Store the current error value for the next iteraction
        return force + integral + derivative; // Return the total
    }

    public void ResetInitialisation() => initialised = false;
    protected virtual void RotateToTarget(Transform target)
    {
        Vector3 targetDirection = target.transform.position - weapon.transform.position; // Get the direction from the weapon to the target
        //weapon.transform.rotation = Quaternion.Slerp(weapon.transform.rotation, Quaternion.LookRotation(targetDirection.normalized, Vector3.up), RotationSpeed * Time.fixedDeltaTime);
        targetDirection.y = 0;
        Quaternion dirInQuaternion = Quaternion.LookRotation(targetDirection.normalized, Vector3.up); // Rotation for the next step 
        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, dirInQuaternion, RotationSpeed * Time.fixedDeltaTime); // Rotate the enemy so that the gun is in position to fire the player
    }
    /*
    public virtual void AssignConsts(){ // Fetches constants used across many different classes
        maxSlope = GlobalConstants.maxSlope;
    }
    private void OnCollisionStay(Collision other) {
        foreach (ContactPoint contact in other.contacts){
            float slopeAngle = Vector3.Angle(contact.normal, Vector3.up);
            if (slopeAngle < maxSlope){
                groundNormal = contact.normal;
            }
        }
    }
    protected void OnCollisionExit(Collision other){
        if (other.contacts.Length > 0){
            foreach (ContactPoint contact in other.contacts){
                float slopeAngle = Vector3.Angle(contact.normal, Vector3.up);
                if (slopeAngle < maxSlope){
                    groundNormal = contact.normal;
                }
            }
        } else {
            groundNormal = Vector3.up;
        }
    }
    */
    public override void Kill()
    {
        weapon.GetComponent<Rigidbody>().useGravity = true; // Allow the weapon to react normally
        weapon.GetComponent<Rigidbody>().detectCollisions = true; // Allow it to detect collision
        weapon.GetComponent<Rigidbody>().isKinematic = true; // Allow it to be moved by the Physics system
        weapon.transform.SetParent(null); // Release the weapon
        PlayerManager.Instance.AddMerit(merit); // Award merit on death
        Destroy(gameObject); // Destroy self
    }
}
