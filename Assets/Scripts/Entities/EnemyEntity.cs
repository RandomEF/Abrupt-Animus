using UnityEngine;

public class EnemyEntity : Entity
{
    public EnemyManager manager;
    public GameObject target;
    public Vector3 searchingTarget;
    protected bool searchTargetSet = false;
    public EnemyManager.MemberState state;
    protected Rigidbody rb;
    public virtual float BaseMovementAcceleration => 8f;
    public virtual float MaxMoveSpeed => 5f;
    public virtual float RotationSpeed => 10f;
    public virtual float FarTargetRange => 15f;
    public virtual float NearTargetRange => 2.5f;
    protected float midRange;
    public virtual float DetectionRange => 20f;
    protected int rayCount;
    protected float rayAngle;
    protected float maxSlope;
    // should probably also have detection angle as well for long distance
    protected virtual float frictionMultiplier => 0.1f;
    protected LayerMask playerLayer;
    protected Vector3 groundNormal = Vector3.up;
    protected GameObject weapon;
    [SerializeField] protected Vector3 velocity;

    public override int value => 10;
    
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

    protected virtual void Awake() {
        SetAngles();
        AssignConsts();
        midRange = (FarTargetRange + NearTargetRange) / 2;
    }
    public virtual void AssignConsts(){ // Fetches constants used across many different classes
        maxSlope = GlobalConstants.maxSlope;
    }
    public void SetAngles(){ // Finds the angle needed to make sure that the player is not missed between rays
        float quarterCircumference = 2 * Mathf.PI * DetectionRange * 0.25f;
        rayCount = Mathf.CeilToInt(quarterCircumference);
        rayAngle = 90 / quarterCircumference;
    }
    public override void Start() {
        Health = MaxHealth;
        rb = GetComponent<Rigidbody>();
        playerLayer = LayerMask.GetMask("Player");
    }
    private void FixedUpdate() {
        /* search for enemies and move around
        If enemies found switch to combat and alert manager
        */
        velocity = rb.linearVelocity;
        Debug.DrawRay(rb.transform.position, rb.transform.rotation * Vector3.forward, Color.yellow);
        if (state != EnemyManager.MemberState.None){
            // Required for the first frame when no target is there or other cases
            if (target != null){
                RotateToTarget(target.transform);
            }
            if (state == EnemyManager.MemberState.Combat){
                // Move towards and fire
                Vector3 movementTarget = target.transform.position - (target.transform.position - rb.transform.position).normalized * midRange;
                ApplyMovement(movementTarget);
                Combat();
            } else if (state == EnemyManager.MemberState.Searching){
                // Search target is reached if they are within 1 metre of it
                if ((searchingTarget - rb.transform.position).magnitude < 1 || !searchTargetSet){
                    SelectTarget();
                    ResetInitialisation();
                    searchTargetSet = true;
                }
                ApplyMovement(searchingTarget);
                Searching();
            }
        }

        lastPosition = rb.transform.position;
    }
    protected virtual void Combat(){ // Fire weapon
        
    }
    protected virtual void ApplyMovement(Vector3 target){ // Fetch changes in movement
        float movementX = PIDUpdate(Time.fixedDeltaTime, rb.transform.position.x, target.x, ref lastPosition.x, ref lastError.x, ref storedIntegral.x);
        float movementY = 0f;
        float movementZ = PIDUpdate(Time.fixedDeltaTime, rb.transform.position.z, target.z, ref lastPosition.z, ref lastError.z, ref storedIntegral.z);
        Vector3 movementTotal = new Vector3(movementX, movementY, movementZ);
        if (movementTotal.magnitude > MaxMoveSpeed){ // Clamp movement
            movementTotal = movementTotal * (MaxMoveSpeed/movementTotal.magnitude);
        }
        Debug.DrawRay(rb.transform.position, new Vector3(movementX, 0f, 0f).normalized, Color.red);
        Debug.DrawRay(rb.transform.position, new Vector3(0f, movementY, 0f).normalized, Color.green);
        Debug.DrawRay(rb.transform.position, new Vector3(0f, 0f, movementZ).normalized, Color.blue);
        Debug.DrawRay(rb.transform.position, movementTotal.normalized, Color.black);
        rb.AddForce(movementTotal);
    }
    protected void Searching(){
        //TODO Change to a overlap sphere checking angles instead and a singular downwards raycast with no max distance
        //TODO Allows for changing of the looking direction

        for (int i = 0; i < rayCount; i++)
        {
            // Do an circumference to split it in 1 metre sections, then take upper bound then run that many raycasts
            RaycastHit hitInfo;
            Vector3 rayDirection = Quaternion.AngleAxis(-45f + rayAngle * i, Vector3.up) * Vector3.forward;
            rayDirection = rb.transform.rotation * rayDirection;
            Debug.DrawRay(rb.transform.position, rayDirection, Color.red);
            if (Physics.Raycast( // Check if a ray has intersected with a target
                origin: rb.transform.position,
                direction: rayDirection,
                hitInfo: out hitInfo,
                maxDistance: DetectionRange,
                layerMask: playerLayer
            )){
                Debug.DrawRay(rb.transform.position, rayDirection, Color.green);
                state = EnemyManager.MemberState.Combat;
                UpdateManager(EnemyManager.MemberState.Combat);
                target = hitInfo.collider.gameObject;
                break;
            }
        }
    }
    protected void SelectTarget(){
        bool found;
        Vector3 point = GetClosestPoint(out found);
        if (!found){
            searchingTarget.y = rb.transform.position.y - DetectionRange;
            return;
        }
        Vector3 entityToPoint = rb.transform.position - point;
        if (entityToPoint.magnitude > DetectionRange){
            searchingTarget = point - entityToPoint.normalized * Random.Range(0, DetectionRange);
            return;
        }

        bool accessible = false;
        while (!accessible){
            searchingTarget.x = Random.Range(-DetectionRange, DetectionRange);
            searchingTarget.y = Random.Range(-DetectionRange, DetectionRange);
            searchingTarget.z = Random.Range(-DetectionRange, DetectionRange);
            searchingTarget = searchingTarget + rb.transform.position;
            RaycastHit hitInfo;
            if (Physics.Raycast(
                origin: rb.transform.position,
                direction: (searchingTarget - rb.transform.position).normalized,
                hitInfo: out hitInfo,
                maxDistance: DetectionRange
            )){
                if ((hitInfo.point - rb.transform.position).magnitude < 1f){
                    continue;
                } else {
                    searchingTarget = hitInfo.point;
                }
            }
            accessible = true;
        }
        ResetInitialisation();
    }
    protected Vector3 GetClosestPoint(out bool found){
        Collider[] colliders = Physics.OverlapSphere(
            position: rb.transform.position,
            radius: DetectionRange
        );
        found = false;
        Vector3 point = Vector3.zero;
        float distSqrd = Mathf.Infinity;
        foreach (Collider collider in colliders){
            found = true;
            if ((rb.transform.position - collider.transform.position).sqrMagnitude < distSqrd){
                distSqrd = (rb.transform.position - collider.transform.position).sqrMagnitude;
                point = collider.ClosestPoint(rb.transform.position);
            }
        }
        return point;
    }
    private void UpdateManager(EnemyManager.MemberState state){
        // send new state to manager
    }

    protected virtual float PIDUpdate(float timeStep, float currentValue, float targetValue, ref float lastPosition, ref float lastError, ref float storedIntegral){
        float error = targetValue - currentValue;
        float force = proportionalGain * error;
        float derivative = 0;
        if (initialised){
            if (derivativeType == DerivativeType.Error){
                if (error - lastError != 0){
                    derivative = (error - lastError) / timeStep;
                }
            } else {
                if (currentValue - lastPosition != 0){
                    derivative = (currentValue - lastPosition) / timeStep;
                }
            }
        } else{
            initialised = true;
        }
        derivative *= derivativeGain;
        storedIntegral = Mathf.Clamp(storedIntegral + error * timeStep, -maxStoredIntegral, maxStoredIntegral);
        float integral = integralGain * storedIntegral;

        lastError = error;
        return force + integral + derivative;
    }
    public void ResetInitialisation() => initialised = false;
    protected virtual void RotateToTarget(Transform target){
        Vector3 targetDirection = target.transform.position - rb.transform.position;
        targetDirection.y = 0;
        Quaternion dirInQuaternion = Quaternion.Euler(targetDirection.normalized);
        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, dirInQuaternion, RotationSpeed * Time.fixedDeltaTime);
    }
    private void OnCollisionEnter(Collision other) {
        if (other.contacts.Length > 0){
            foreach (ContactPoint contact in other.contacts){
                float slopeAngle = Vector3.Angle(contact.normal, Vector3.up);
                if (slopeAngle < maxSlope){
                    groundNormal = contact.normal;
                }
            }
        } else if (other.contacts.Length == 0) {
            groundNormal = Vector3.up;
        }
    }
}
