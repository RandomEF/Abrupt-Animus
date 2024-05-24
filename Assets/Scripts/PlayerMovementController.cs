using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Collision Response")]
    [SerializeField] private int maxBounces = 0;
    [SerializeField] private float cASMargin = 0.015f;
    [SerializeField] private float maxSlopeAngle = 30;
    private CapsuleCollider playerCollider;
    private Bounds colliderBounds;

    [Header("Ground Checks")]
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float groundDistance = 0.1f;

    [Header("Jumping")]
    [SerializeField] private float gravity = -4.905f;
    [SerializeField] private float jumpForce = 3.5f;
    [SerializeField] private float playerHeight = 0.85f * 2;

    [Header("Movement")]
    [SerializeField] private float friction = 10f;
    [SerializeField] private float overflowReductionMultiplier = 0.8f;
    [SerializeField] private float speedCapToDisableOverflow = 300f;
    [SerializeField] private float movementAcceleration = 2f;
    [SerializeField] private float boostMultiplier = 10f;
    [SerializeField] private float maxMoveSpeed = 10f; // Usain Bolt speeds, average human pace is a quarter of this

    private Rigidbody player;
    private PlayerInputs playerInputs; // Use if using the C# Class
    // private PlayerInput playerInput; // Use if using the Unity Interface

    private void Start() {
        playerLayer = LayerMask.GetMask("Standable");

        player = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        player.transform.localScale = new Vector3(player.transform.localScale.x, playerHeight, player.transform.localScale.z);

        playerInputs = new PlayerInputs();
        playerInputs.Player.Enable(); // Enabling only the Player map
        playerInputs.Player.Jump.performed += Jump;
        playerInputs.Player.Boost.performed += Boost;
    }
    private void Update() {
        isGrounded = Physics.CheckSphere(
            new Vector3(player.transform.position.x, player.transform.position.y - (playerHeight), player.transform.position.z),
            groundDistance,
            playerLayer);
        //Gravity();
        colliderBounds = playerCollider.bounds;
        colliderBounds.Expand(-2 * cASMargin);
        player.velocity = CollideAndSlide(player.velocity, player.transform.position, false, player.velocity);
        player.velocity += CollideAndSlide(new Vector3(0, gravity), player.transform.position + player.velocity, true, new Vector3(0, gravity));
        Movement();
        //WarpToCollision();
    }
    private void Gravity(){
        if (!isGrounded){
            player.AddForce(new Vector3(0, -9.81f, 0), ForceMode.Impulse);
        }
    }
    private void WarpToCollision(){
        // Work out predictive location in one frame
        Vector3 predictedDistance =  player.velocity * Time.deltaTime;
        // Vector3 predictedDirection = Vector3.Normalize(predictedDistance);
        RaycastHit hitInfo = new RaycastHit();
        Ray ray = new Ray(player.transform.position, player.velocity.normalized);
        if(Physics.Raycast(ray, out hitInfo, predictedDistance.magnitude, playerLayer)){
            Debug.Log(hitInfo.point);
            //player.transform.position = hitInfo.point + hitInfo.normal / 0.5f;
            RaycastHit playerPoint = new RaycastHit();
            //Ray playerRay = new Ray(player.transform.position,)
            //Collider.Raycast(player.transform.position, hitInfo.normal, out playerPoint);
            Debug.Log(playerPoint.point);
            Vector3 movementDisplacement = player.transform.position - playerPoint.point;
            player.transform.position = hitInfo.point - movementDisplacement;
            if(Vector3.Dot(player.velocity.normalized, hitInfo.normal) > 0.005f){
                Vector3 face = Vector3.Cross(predictedDistance, hitInfo.normal);
                Quaternion rotation = Quaternion.AngleAxis(-90, hitInfo.normal);
                player.velocity -= Vector3.Dot(hitInfo.normal, player.velocity) * hitInfo.normal;
                player.velocity = (rotation * face) * (Vector3.Dot(hitInfo.normal, player.velocity));
            } else {
                player.velocity = Vector3.zero;
            }
        }
    }
    private Vector3 MagnitudeMaintainedProjection(Vector3 vector, Vector3 projectionNormal){
        float magnitude = vector.magnitude;
        vector = Vector3.ProjectOnPlane(vector, projectionNormal).normalized;
        return vector * magnitude;
    }
    private Vector3 CollideAndSlide(Vector3 velocity, Vector3 origin, bool inGravityPass, Vector3 initialVelocity, int bounceCount = 0){
        /* Courtesy of Poke Dev in https://www.youtube.com/watch?v=YR6Q7dUz2uk */
        if (bounceCount >= maxBounces){
            return Vector3.zero;
        }
        float collisionDistance = (velocity.magnitude + cASMargin);
        RaycastHit hitInfo;
        if (Physics.CapsuleCast(
            point1: player.position + new Vector3(0f, playerCollider.height/4f*playerHeight),
            point2: player.position - new Vector3(0f, playerCollider.height/4f*playerHeight),
            radius: playerCollider.radius,
            direction: velocity.normalized,
            maxDistance: collisionDistance,
            hitInfo: out hitInfo,
            layerMask: playerLayer
        )){
            Vector3 movePlayerToSurface = velocity.normalized * (hitInfo.distance);// - cASMargin
            Vector3 remainder = velocity - movePlayerToSurface;
            
            if (movePlayerToSurface.magnitude <= cASMargin){
                movePlayerToSurface = Vector3.zero;
            }
            
            float surfaceAngle = Vector3.Angle(Vector3.up, hitInfo.normal);
            if (surfaceAngle <= maxSlopeAngle){
                if(inGravityPass){
                    return movePlayerToSurface;
                }
                remainder = MagnitudeMaintainedProjection(remainder, hitInfo.normal);
            } else {
                float velocityScaler = 1 - Vector3.Dot(
                    new Vector3(hitInfo.normal.x, 0f, hitInfo.normal.z).normalized,
                    -new Vector3(initialVelocity.x, 0f, initialVelocity.z)
                );

                if(isGrounded && !inGravityPass){
                    remainder = MagnitudeMaintainedProjection(
                        new Vector3(remainder.x, 0f, remainder.z).normalized,
                        new Vector3(hitInfo.normal.x, 0f, hitInfo.normal.z)
                    ).normalized * velocityScaler;
                } else{
                    remainder = velocityScaler * MagnitudeMaintainedProjection(remainder, hitInfo.normal);
                }
            }
            return movePlayerToSurface + CollideAndSlide(remainder, origin + movePlayerToSurface, inGravityPass, initialVelocity, bounceCount + 1);
        }
        return velocity;
    }

    private float CalculateAccelerationMultiplier(){
        return Mathf.Pow(2, -0.05f*Mathf.Abs(player.velocity.magnitude)+3.169f) + 1;
    }
    
    private void Movement() {
        Vector2 inputDirection = playerInputs.Player.Movement.ReadValue<Vector2>().normalized;
        if (inputDirection.magnitude == 0) {
            if (isGrounded) {
                player.AddForce(new Vector3(-player.velocity.x, 0, -player.velocity.z) * friction * Time.deltaTime, ForceMode.VelocityChange);
            }
        } else {
            float speedDecelerator = CalculateAccelerationMultiplier();
            player.AddForce(new Vector3(inputDirection.x, 0, inputDirection.y) * movementAcceleration * player.mass * Time.deltaTime * speedDecelerator, ForceMode.Impulse);
        }
        if (player.velocity.magnitude < speedCapToDisableOverflow){
            Vector2 horizontalMovement = new Vector2(player.velocity.x, player.velocity.z);
            float overflowReduction = (Mathf.Abs(horizontalMovement.magnitude) - maxMoveSpeed) * overflowReductionMultiplier;
            if (overflowReduction > 0.1f && overflowReduction < 2f) {
                horizontalMovement.x -= Mathf.Abs(horizontalMovement.x/horizontalMovement.magnitude) * overflowReduction * Mathf.Sign(horizontalMovement.x);
                horizontalMovement.y -= Mathf.Abs(horizontalMovement.y/horizontalMovement.magnitude) * overflowReduction * Mathf.Sign(horizontalMovement.y);
            }
            player.velocity = new Vector3(horizontalMovement.x, player.velocity.y, horizontalMovement.y);
        }
    }
    private void Jump(InputAction.CallbackContext inputType){
        if (isGrounded){
            player.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }
    private void Boost(InputAction.CallbackContext inputType){
        Vector2 inputDirection = playerInputs.Player.Movement.ReadValue<Vector2>().normalized;
        Vector2 horizontalMovement = new Vector2(player.velocity.x, player.velocity.z).normalized;
        if (inputDirection.x != 0) {
            horizontalMovement.x = Mathf.Abs(horizontalMovement.x) * Mathf.Sign(inputDirection.x);
        }
        if (inputDirection.y != 0){
            horizontalMovement.y = Mathf.Abs(horizontalMovement.y) * Mathf.Sign(inputDirection.y);
        }
        player.AddForce(new Vector3(horizontalMovement.x, 0, horizontalMovement.y) * boostMultiplier, ForceMode.VelocityChange);
    }
}
