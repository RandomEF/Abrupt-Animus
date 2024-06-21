using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
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
    [SerializeField] private float overflowReductionMultiplier = 0.8f;
    [SerializeField] private float speedCapToDisableOverflow = 300f;
    [SerializeField] private float movementAcceleration = 2f;
    [SerializeField] private float boostMultiplier = 10f;
    [SerializeField] private float maxWalkSpeed = 5f; // Usain Bolt speeds, average human pace is a quarter of this
    [SerializeField] private float maxSprintSpeed = 10f; // Usain Bolt speeds, average human pace is a quarter of this

    private Rigidbody player;
    private CapsuleCollider playerCollider;
    private PlayerInputs playerInputs; // Use if using the C# Class
    // private PlayerInput playerInput; // Use if using the Unity Interface

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

        playerInputs = new PlayerInputs();
        playerInputs.Player.Enable(); // Enabling only the Player input map
        playerInputs.Player.Jump.performed += Jump;
        playerInputs.Player.Boost.performed += Boost;

        movementState = PlayerMovementState.idle;
    }
    private void Update() {
        velocity = player.velocity;
        horizontalVelocity = new Vector2(player.velocity.x, player.velocity.z);
        if ((horizontalVelocity.magnitude < preBoostVelocity - 1 && horizontalVelocity.magnitude < maxWalkSpeed) && movementState == PlayerMovementState.boosting){
            movementState = PlayerMovementState.walking;
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
        switch (movementState){
            case PlayerMovementState.walking:
                float clampedMagnitude = Vector2.ClampMagnitude(horizontalVelocity, maxWalkSpeed).magnitude;
                return SpeedFunction(clampedMagnitude, 0.5f, 20);
            default:
                return 1;
        }
    }
    
    private void Movement() {
        inputDirection = playerInputs.Player.Movement.ReadValue<Vector2>().normalized;
        if (inputDirection.magnitude != 0) {
            if (movementState != PlayerMovementState.boosting){
                movementState = PlayerMovementState.walking;
            }
            float speedMultiplier = CalculateAccelerationMultiplier();
            Vector3 multipliedVelocity = new Vector2(inputDirection.x, inputDirection.y) * movementAcceleration * speedMultiplier * Time.deltaTime;
            Vector3 directedVelocity = new Vector2(inputDirection.x, inputDirection.y) * movementAcceleration * 20 * Time.deltaTime;
            float alignment = Vector3.Dot(player.velocity.normalized, directedVelocity.normalized);
            Vector3 lerpedVelocity = Vector3.Slerp(directedVelocity, multipliedVelocity, alignment);
            horizontalVelocity += new Vector2(lerpedVelocity.x, lerpedVelocity.y);
            //player.AddForce(new Vector3(inputDirection.x, 0, inputDirection.y) * movementAcceleration * speedMultiplier * Time.deltaTime, ForceMode.Impulse);
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
        preBoostVelocity = velocity.magnitude;
        inputDirection = playerInputs.Player.Movement.ReadValue<Vector2>().normalized;
        Vector2 horizontalMovement = horizontalVelocity.normalized;
        if (inputDirection.x != 0) {
            horizontalMovement.x = Mathf.Abs(horizontalMovement.x) * Mathf.Sign(inputDirection.x);
        }
        if (inputDirection.y != 0){
            horizontalMovement.y = Mathf.Abs(horizontalMovement.y) * Mathf.Sign(inputDirection.y);
        }
        velocity += new Vector3(horizontalMovement.x, 0, horizontalMovement.y) * boostMultiplier;
        //player.AddForce(new Vector3(horizontalMovement.x, 0, horizontalMovement.y) * boostMultiplier, ForceMode.VelocityChange);
    }
}
