using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{/*
    [Header("Controller")]
    [SerializeField] private float maxSlopeAngle = 30;
    [SerializeField] private float height = 2;
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private float skinWidth = 0.08f;
    [SerializeField] private float stepOffset = 0.3f;
    [SerializeField] private Vector3 velocity = Vector3.zero;
    [SerializeField] private float gravity = -4.905f;

    [Header("Collision Response")]
    [SerializeField] private int maxBounces = 5;

    [Header("Ground Checks")]
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float groundDistance = 0.1f;

    [Header("Movement Controls")]
    private Vector2 inputDirection;
    private Vector2 horizontalMovement;
    [SerializeField] private float jumpHeight = 3.5f;
    [SerializeField] private float friction = 0.8f;
    [SerializeField] private float drag = 0.1f;
    [SerializeField] private float overflowReductionMultiplier = 0.8f;
    [SerializeField] private float speedCapToDisableOverflow = 300f;
    [SerializeField] private float movementAcceleration = 2f;
    [SerializeField] private float boostMultiplier = 10f;
    [SerializeField] private float maxMoveSpeed = 10f; 

    private Transform player;
    private CapsuleCollider playerCollider;
    private PlayerInputs playerInputs; // Use if using the C# Class
    // private PlayerInput playerInput; // Use if using the Unity Interface
    void Start()
    {
        playerLayer = LayerMask.GetMask("Standable");

        player = GetComponent<Transform>();
        playerCollider = GetComponent<CapsuleCollider>();
        playerCollider.height = height;

        playerInputs = new PlayerInputs();
        playerInputs.Player.Enable(); // Enabling only the Player map
        playerInputs.Player.Jump.performed += Jump;
        playerInputs.Player.Boost.performed += Boost;
    }

    // Update is called once per frame
    void Update()
    {
        inputDirection = playerInputs.Player.Movement.ReadValue<Vector2>();
        isGrounded = GroundTest();
        Movement();
        MovePlayer();
    }
    private bool GroundTest(){
        return Physics.CheckSphere(
            position: new Vector3(player.position.x, player.position.y - skinWidth + radius - height/2 - groundDistance, player.position.z),
            radius: radius + skinWidth,
            layerMask: playerLayer
        );
    }

    private Vector3 CollideAndSlide(Vector3 movementVector, Vector3 origin, bool inGravityPass, int bounceCount = 0){
        if (bounceCount > maxBounces){
            return Vector3.zero;
        }
        float collisionDistance = movementVector.magnitude + skinWidth;

        RaycastHit capsuleHit;
        if (Physics.CapsuleCast(
            point1: player.position + new Vector3(0f, height/2 - radius),
            point2: player.position - new Vector3(0f, height/2 - radius),
            radius: playerCollider.radius,
            direction: movementVector.normalized,
            maxDistance: collisionDistance,
            hitInfo: out capsuleHit,
            layerMask: playerLayer
        )){
            RaycastHit rayHit;
            RaycastHit hitInfo = capsuleHit;
            float diffBetweenHits = 5f;

            if (Physics.Raycast(
                origin: player.position,
                direction: movementVector.normalized,
                maxDistance: collisionDistance,
                hitInfo: out rayHit,
                layerMask: playerLayer
            )){
                diffBetweenHits = Vector3.Angle(capsuleHit.normal, rayHit.normal);
            }
            if (diffBetweenHits < 5f){
                hitInfo.normal = rayHit.normal;
            }
            Vector3 movePlayerToSurface = movementVector.normalized * (hitInfo.distance - skinWidth);
            Debug.Log("Move Player to Surface: " + movePlayerToSurface);
            Vector3 remainder = movementVector - movePlayerToSurface;
            
            if (movePlayerToSurface.magnitude <= skinWidth){
                movePlayerToSurface = Vector3.zero;
            }
            
            float surfaceAngle = Vector3.Angle(Vector3.up, hitInfo.normal);
            if (surfaceAngle <= maxSlopeAngle){
                Debug.Log("Surface Angle: " + surfaceAngle);
                if(inGravityPass){
                    Debug.Log("Gravity Slope: " + movePlayerToSurface);
                    return movePlayerToSurface;
                }
                remainder = Vector3.ProjectOnPlane(remainder, hitInfo.normal);
            } else {
                float movementScaler = 1 - Vector3.Dot(
                    new Vector3(hitInfo.normal.x, 0f, hitInfo.normal.z),
                    -new Vector3(movementVector.x, 0f, movementVector.z).normalized
                );

                if(isGrounded && !inGravityPass){
                    remainder = Vector3.ProjectOnPlane(
                        new Vector3(remainder.x, 0f, remainder.z),
                        new Vector3(hitInfo.normal.x, 0f, hitInfo.normal.z)
                    ).normalized * remainder.magnitude * movementScaler;
                } else{
                    remainder = movementScaler * remainder.magnitude * Vector3.ProjectOnPlane(remainder, hitInfo.normal).normalized;
                }
            }
            Debug.Log($"Remainder: {remainder}");
            return movePlayerToSurface + CollideAndSlide(remainder, origin + movePlayerToSurface, inGravityPass, bounceCount + 1);
        }
        return movementVector;
    }
    private void Movement(){
        if (isGrounded){
            Friction();
        }
        horizontalMovement = new Vector2(velocity.x, velocity.z);
        if (horizontalMovement.magnitude < 0.001f){
            horizontalMovement = Vector2.zero;
        }
        horizontalMovement += new Vector2(inputDirection.x, inputDirection.y) * movementAcceleration;
        velocity = new Vector3(horizontalMovement.x, velocity.y, horizontalMovement.y);
        velocity -= velocity * drag;
    }
    private void Friction(){
        velocity *= 1 - friction;
    }
    
    private void Jump(InputAction.CallbackContext inputType){
        if(isGrounded){
            velocity.y = jumpHeight * -2f * gravity;
        }
    }
    private void Boost(InputAction.CallbackContext context){
        inputDirection = playerInputs.Player.Movement.ReadValue<Vector2>();
        if(inputDirection.magnitude != 0){
            velocity.x = Mathf.Abs(velocity.x) * Mathf.Sign(inputDirection.x);
            velocity.z = Mathf.Abs(velocity.z) * Mathf.Sign(inputDirection.y);
        }
        velocity.x *= boostMultiplier;
        velocity.z *= boostMultiplier;
    }
    private void MovePlayer(){
        Vector3 moveAmount = velocity * Time.deltaTime;
        Debug.Log("Before: " + moveAmount);
        moveAmount = CollideAndSlide(moveAmount, player.position, false);
        Debug.Log("Between: " + moveAmount);
        moveAmount += CollideAndSlide(new Vector3(0, gravity), player.position + moveAmount, true);
        Debug.Log("After: " + moveAmount);
        player.position += moveAmount;
    }*/
}
