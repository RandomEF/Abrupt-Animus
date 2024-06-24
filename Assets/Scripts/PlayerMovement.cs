using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public GameObject manager;
    private PlayerInputs playerInputs; // Use if using the C# Class
    // private PlayerInput playerInput; // Use if using the Unity Interface

    [Header("Character Dimensions")]
    [SerializeField] public float playerHeight = 1.7f;
    [SerializeField] public float radius = 0.5f;

    [Header("Ground Checks")]
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] public float groundDistance = 0.1f;

    [Header("Jumping")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpForce = 3.5f;

    [Header("Movement")]
    [SerializeField] public PlayerMovementState movementState;
    [SerializeField] private float friction = 0.8f;
    [SerializeField] private float drag = 0.1f;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private Vector2 horizontalVelocity;
    [SerializeField] private Vector2 inputDirection;
    [SerializeField] private float preBoostVelocity;
    [SerializeField] private float movementAcceleration = 2f;
    [SerializeField] private float boostMultiplier = 10f;

    [Header("Speed Caps")]
    [SerializeField] private float maxWalkSpeed = 7.5f;
    [SerializeField] private float maxSprintSpeed = 20f;
    [SerializeField] private float maxBoostSpeed;

    private Rigidbody player;
    private CapsuleCollider playerCollider;

    public enum PlayerMovementState
    {
        idle,
        crouched,
        walking,
        sprinting,
        boosting,
        falling,
    } 
    private void Start() {
        playerLayer = LayerMask.GetMask("Standable");

        player = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        player.transform.localScale = new Vector3(radius/playerCollider.radius, playerHeight/playerCollider.height, radius/playerCollider.radius);

        playerInputs = manager.GetComponent<PlayerManager>().inputs;

        playerInputs.Player.Jump.performed += Jump;
        playerInputs.Player.Boost.performed += Boost;

        movementState = PlayerMovementState.idle;
    }
    private void Update() {
        velocity = player.velocity;
        horizontalVelocity = new Vector2(player.velocity.x, player.velocity.z);
        if (horizontalVelocity.magnitude < preBoostVelocity - 1 && horizontalVelocity.magnitude < maxWalkSpeed && !playerInputs.Player.Sprint.inProgress && movementState == PlayerMovementState.boosting){
            movementState = PlayerMovementState.walking;
        } else if (horizontalVelocity.magnitude < preBoostVelocity - 1 && horizontalVelocity.magnitude < maxSprintSpeed && playerInputs.Player.Sprint.inProgress && movementState == PlayerMovementState.boosting){
            movementState = PlayerMovementState.sprinting;
        }
        if (velocity == Vector3.zero){
            movementState = PlayerMovementState.idle;
        }
        isGrounded = GroundCheck();
        Gravity();
        Movement();
        horizontalVelocity = ClampSpeed();
        velocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.y);
        player.AddForce(velocity - player.velocity, ForceMode.VelocityChange);
    }
    private bool GroundCheck(){
        return Physics.CheckSphere(
            new Vector3(player.transform.position.x, player.transform.position.y - groundDistance + (0.99f * radius) - playerHeight/2, player.transform.position.z),
            radius * 0.99f,
            playerLayer); 
    }
    private void Gravity(){
        if (!isGrounded){
            velocity.y += gravity * Time.deltaTime;
        }
    }
    private float FrictionMultiplier(){
        if (!isGrounded){
            return 1 - drag;
        } else if (inputDirection.magnitude == 0 || movementState == PlayerMovementState.boosting){
            return 1 - friction;
        } else {
            return 1;
        }
    }
    private Vector2 ClampSpeed(){
        switch (movementState){
            case PlayerMovementState.walking:
                return Vector2.ClampMagnitude(horizontalVelocity, maxWalkSpeed);
            case PlayerMovementState.sprinting:
                return Vector2.ClampMagnitude(horizontalVelocity, maxSprintSpeed);
            case PlayerMovementState.boosting:
                return Vector2.ClampMagnitude(horizontalVelocity, maxBoostSpeed);
            default:
                return horizontalVelocity;
        }
    }
    private float Pow4(float num){
        return num*num*num*num;
    }
    private float SpeedFunction(float speed, float a, float b){
        return -(Pow4(speed/a)/(b*b*b))+b;
    }
    private float CalculateAccelerationMultiplier(){
        float clampedMagnitude;
        switch (movementState){
            case PlayerMovementState.walking:
                clampedMagnitude = Vector2.ClampMagnitude(horizontalVelocity, maxWalkSpeed).magnitude;
                return SpeedFunction(clampedMagnitude, 0.75f, 10);
            case PlayerMovementState.sprinting:
                clampedMagnitude = Vector2.ClampMagnitude(horizontalVelocity, maxSprintSpeed).magnitude;
                return SpeedFunction(clampedMagnitude, 1, 15f);
            case PlayerMovementState.boosting:
                return 0;
            default:
                return 1;
        }
    }
    
    private void Movement() {
        inputDirection = playerInputs.Player.Movement.ReadValue<Vector2>().normalized;
        if (inputDirection.magnitude != 0) {
            if (movementState != PlayerMovementState.boosting){
                if (playerInputs.Player.Sprint.inProgress){
                    movementState = PlayerMovementState.sprinting;
                } else {
                    movementState = PlayerMovementState.walking;
                }
            }
            float speedMultiplier = CalculateAccelerationMultiplier();
            Vector3 multipliedVelocity = new Vector3(inputDirection.x, 0, inputDirection.y) * movementAcceleration * speedMultiplier * Time.deltaTime;
            Vector3 directedVelocity = new Vector3(inputDirection.x, 0, inputDirection.y) * movementAcceleration * 20 * Time.deltaTime;
            float alignment = Vector3.Dot(player.velocity.normalized, directedVelocity.normalized);
            Vector3 lerpedVelocity = player.rotation * Vector3.Slerp(directedVelocity, multipliedVelocity, alignment);
            horizontalVelocity += new Vector2(lerpedVelocity.x, lerpedVelocity.z);
        } else {
            horizontalVelocity *= FrictionMultiplier();
        }
    }
    private void Jump(InputAction.CallbackContext inputType){
        if (isGrounded){
            player.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }
    private void Boost(InputAction.CallbackContext inputType){
        movementState = PlayerMovementState.boosting;
        preBoostVelocity = velocity.magnitude < 1 ? 1 : velocity.magnitude;
        inputDirection = playerInputs.Player.Movement.ReadValue<Vector2>().normalized;
        Vector2 horizontalMovement = inputDirection * horizontalVelocity.magnitude * (boostMultiplier - 1);
        Vector3 movement = player.rotation * new Vector3(horizontalMovement.x, 0, horizontalMovement.y);
        maxBoostSpeed = (velocity + movement).magnitude;
        player.AddForce(movement, ForceMode.VelocityChange);
    }
}
