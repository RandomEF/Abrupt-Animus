using Unity.IO.LowLevel.Unsafe;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class EnemyEntity : Entity
{
    public EnemyManager manager;
    public GameObject target;
    public enemyState state;
    protected Rigidbody rb;
    public virtual float BaseMovementAcceleration => 8f;
    public virtual float MaxMoveSpeed => 10f;
    public virtual float RotationSpeed => 10f;
    public virtual float FarTargetRange => 15f;
    public virtual float NearTargetRange => 2.5f;
    public virtual float DetectionRange => 20f;
    protected int rayCount;
    protected float rayAngle;
    protected float maxSlope;
    // should probably also have detection angle as well for long distance
    protected virtual float frictionMultiplier => 0.1f;
    protected LayerMask playerLayer;
    protected Vector3 groundNormal = Vector3.up;
    public enum enemyState{
        Searching,
        Combat,
        None
    }
    protected virtual void Awake() {
        SetAngles();
        AssignConsts();
    }
    public virtual void AssignConsts(){
        maxSlope = GlobalConstants.maxSlope;
    }
    public void SetAngles(){
        float quarterCircumference = 2 * Mathf.PI * DetectionRange * 0.25f;
        rayCount = Mathf.CeilToInt(quarterCircumference);
        rayAngle = 90 / quarterCircumference;
    }
    public override void Start() {
        Health = MaxHealth;
        rb = GetComponent<Rigidbody>();
        playerLayer = LayerMask.GetMask("Player");
    }
    private void Update() {
        /* search for enemies and move around
        If enemies found switch to combat and alert manager
        */
        Debug.DrawRay(rb.transform.position, rb.transform.rotation * Vector3.forward, Color.yellow);
        if (state != enemyState.None){
            if (target != null){
                RotateToTarget(target.transform);
            }
            if (state == enemyState.Combat){
                Move();
                // move towards and fire
            } else if (state == enemyState.Searching){
                // Move around
                Searching();
            }
        }
    }
    private void Searching(){
        for (int i = 0; i < rayCount; i++)
        {
            // Do an circumference to split it in 1 metre sections, then take upper bound then run that many raycasts
            RaycastHit hitInfo;
            Vector3 rayDirection = Quaternion.AngleAxis(-45f + rayAngle * i, Vector3.up) * Vector3.forward;
            rayDirection = rb.transform.rotation * rayDirection;
            Debug.DrawRay(rb.transform.position, rayDirection, Color.red);
            if (Physics.Raycast(
                origin: rb.transform.position,
                direction: rayDirection,
                hitInfo: out hitInfo,
                maxDistance: DetectionRange,
                layerMask: playerLayer
            )){
                Debug.DrawRay(rb.transform.position, rayDirection, Color.green);
                state = enemyState.Combat;
                UpdateManager(enemyState.Combat);
                target = hitInfo.collider.gameObject;
                break;
            }
        }
    }
    private void UpdateManager(enemyState enemyState){
        // send new state to manager
    }
    protected virtual void Move(){
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
        
        float directionMag = targetDirection.magnitude;
        targetDirection = Vector3.ProjectOnPlane(direction, groundNormal).normalized * acceleration;

        targetDirection -= targetDirection * frictionMultiplier;
        rb.AddForce(targetDirection * Time.deltaTime, ForceMode.VelocityChange);
    }
    protected virtual void RotateToTarget(Transform target){
        Vector3 targetDirection = target.transform.position - rb.transform.position;
        targetDirection.y = 0;
        Quaternion dirInQuaternion = Quaternion.Euler(targetDirection.normalized);
        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, dirInQuaternion, RotationSpeed * Time.deltaTime);
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
