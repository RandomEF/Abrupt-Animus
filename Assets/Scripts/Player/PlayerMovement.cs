using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public GameObject manager;
    private PlayerInputs playerInputs; // Use if using the C# Class
    // private PlayerInput playerInput; // Use if using the Unity Interface

    [Header("Character Dimensions")]
    [SerializeField] public float playerRadius = 0.5f;
    [SerializeField] public float standingHeight = 1.7f;
    [SerializeField] public float crouchingHeight = 1f;

    [Header("Ground Checks")]
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] public float groundDistance = 0.03f;
    [SerializeField] public float maxSlope = 60f;
    [SerializeField] public Vector3 groundNormal = Vector3.up;

    [Header("Jumping")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpForce = 3.5f;

    [Header("Movement")]
    [SerializeField] public PlayerMovementState movementState;
    [SerializeField] public PlayerMovementState lastMovementState = PlayerMovementState.idle;
    [SerializeField] private float friction = 0.9f;
    [SerializeField] private float drag = 0.01f;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private Vector3 groundVelocity;
    [SerializeField] private Vector2 inputDirection;
    [SerializeField] private float preBoostVelocity;
    [SerializeField] private float baseMovementAcceleration = 10f;
    [SerializeField] private float boostMultiplier = 2f;

    [Header("Crouching")]
    [SerializeField] private bool lastHoldCrouchState = false;
    [SerializeField] private bool holdCrouch = false;
    [SerializeField] private bool toggleCrouch = false;
    [SerializeField] private bool wasSliding = false;
    [SerializeField] private float startedSliding;
    [SerializeField] private bool slideCapSet;

    [Header("Speed Caps")]
    [SerializeField] private float maxCrouchSpeed = 4.5f;
    [SerializeField] private float maxSlidingSpeed = 15f;
    [SerializeField] private float maxWalkSpeed = 7.5f;
    [SerializeField] private float maxSprintSpeed = 15f;
    [SerializeField] private float maxBoostSpeed;

    [Header("Objects")]
    [SerializeField] private GameObject body;
    [SerializeField] private Rigidbody player;
    [SerializeField] private CapsuleCollider playerCollider;
    const float minSpeed = 1e-4f;

    public enum PlayerMovementState
    {
        idle,
        crouching,
        sliding,
        walking,
        sprinting,
        boosting,
        wallrunning,
    } 
    private void Start() {
        playerLayer = LayerMask.GetMask("Standable");

        player = body.GetComponent<Rigidbody>();
        playerCollider = body.GetComponent<CapsuleCollider>();
        SetPlayerDimensions(standingHeight, playerRadius);

        playerInputs = manager.GetComponent<PlayerManager>().inputs;

        playerInputs.Player.Jump.performed += Jump;
        playerInputs.Player.Boost.performed += Boost;

        movementState = PlayerMovementState.idle;
    }
    private void Update() {
        velocity = player.linearVelocity;
        groundVelocity = Vector3.ProjectOnPlane(velocity, groundNormal);
        inputDirection = playerInputs.Player.Movement.ReadValue<Vector2>().normalized;
        bool crouching = CrouchControlState();

        //isGrounded = GroundCheck();
        SetMovementState(crouching);
        Crouch();
        Gravity();
        Movement();
        lastMovementState = movementState;
    }
    private void SetMovementState(bool isCrouched){
        if (groundVelocity.magnitude <= maxWalkSpeed && isCrouched && movementState != PlayerMovementState.sliding){
            movementState = PlayerMovementState.crouching;
            wasSliding = false;
        } else if (groundVelocity.magnitude > maxWalkSpeed && isCrouched){
            movementState = PlayerMovementState.sliding;
            wasSliding = false;
        } else if ((groundVelocity.magnitude < preBoostVelocity - 1 && groundVelocity.magnitude > minSpeed && groundVelocity.magnitude < maxWalkSpeed && !playerInputs.Player.Sprint.inProgress && movementState == PlayerMovementState.boosting) || (groundVelocity.magnitude > minSpeed && groundVelocity.magnitude <= maxWalkSpeed && !playerInputs.Player.Sprint.inProgress && movementState != PlayerMovementState.boosting)){
            movementState = PlayerMovementState.walking;
            wasSliding = false;
        } else if ((groundVelocity.magnitude < preBoostVelocity - 1 && groundVelocity.magnitude > minSpeed && groundVelocity.magnitude < maxSprintSpeed && playerInputs.Player.Sprint.inProgress && movementState == PlayerMovementState.boosting) || (groundVelocity.magnitude > minSpeed && groundVelocity.magnitude <= maxSprintSpeed && playerInputs.Player.Sprint.inProgress && movementState != PlayerMovementState.boosting)){
            movementState = PlayerMovementState.sprinting;
            if (!wasSliding){
                wasSliding = true;
                startedSliding = Time.time;
                maxSlidingSpeed = groundVelocity.magnitude + 50f;
            }
            if (slideCapSet){
                maxSlidingSpeed = groundVelocity.magnitude;
            }
        } else if (inputDirection.magnitude == 0){
            movementState = PlayerMovementState.idle;
            wasSliding = false;
        }
    }
    private void SetPlayerDimensions(float height, float radius){
        // float previousHeight = player.transform.localScale.y * playerCollider.height;
        // player.transform.localScale = new Vector3(radius/playerCollider.radius, height/playerCollider.height, radius/playerCollider.radius);
        // player.transform.position -= new Vector3(0, (previousHeight - height)/2, 0);
        float previousHeight = playerCollider.height;
        playerCollider.height = height;
        player.transform.position -= new Vector3(0, (previousHeight - height)/2, 0);
    }
    private bool CrouchControlState(){
        holdCrouch = playerInputs.Player.HoldCrouch.inProgress;
        if (playerInputs.Player.ToggleCrouch.triggered){
            toggleCrouch = toggleCrouch ? false : true;
        }
        if (holdCrouch){
            lastHoldCrouchState = true;
            return true;
        } else {
            if (lastHoldCrouchState){
                toggleCrouch = false;
            }
            lastHoldCrouchState = false;
            return toggleCrouch;
        }
    }
    private void Crouch(){
        if (lastMovementState != movementState){
            if (movementState == PlayerMovementState.crouching || movementState == PlayerMovementState.sliding){
                SetPlayerDimensions(crouchingHeight, playerRadius);
            } else {
                SetPlayerDimensions(standingHeight, playerRadius);
            }
        }
    }
    private void Gravity(){
        if (!isGrounded){
            player.AddForce((Physics.gravity / -9.81f * gravity) * Time.deltaTime, ForceMode.VelocityChange);
        }
    }
    private float FrictionMultiplier(){
        if (!isGrounded){
            return 1 - drag;
        } else if (movementState == PlayerMovementState.boosting){
            return 1 - friction/10;
        } else if (movementState == PlayerMovementState.sliding){
            return 1 - friction;
        } else if (inputDirection.magnitude == 0) {
            return SpeedFunction(Mathf.Clamp(groundVelocity.magnitude, 0, 1.414f), 1, 1) - 1;
        } else {
            return SpeedFunction(Mathf.Clamp(groundVelocity.magnitude, 0, 1f), 1, 1) - 1;
        }
    }
    private float MaxSpeed(){
        switch (movementState){
            case PlayerMovementState.crouching:
                return maxCrouchSpeed;
            case PlayerMovementState.sliding:
                return maxSlidingSpeed;
            case PlayerMovementState.walking:
                return maxWalkSpeed;
            case PlayerMovementState.sprinting:
                return maxSprintSpeed;
            case PlayerMovementState.boosting:
                return maxBoostSpeed;
            case PlayerMovementState.idle:
                return maxWalkSpeed;
            default:
                return groundVelocity.magnitude;
        }
    }
    private float Pow4(float num){
        return num*num*num*num;
    }
    private float SpeedFunction(float speed, float a, float b){
        return -(Pow4(speed/a)/(b*b*b))+b;
    }
    private float CalculateAccelerationMultiplier(Vector2? speed = null){
        if (speed == null){
            speed = groundVelocity;
        }
        float clampedMagnitude;
        float acceleration;
        switch (movementState){
            case PlayerMovementState.walking:
                clampedMagnitude = Vector2.ClampMagnitude((Vector2)speed, maxWalkSpeed).magnitude;
                acceleration = SpeedFunction(clampedMagnitude, 0.75f, 10);
                break;
            case PlayerMovementState.sprinting:
                clampedMagnitude = Vector2.ClampMagnitude((Vector2)speed, maxSprintSpeed).magnitude;
                acceleration = SpeedFunction(clampedMagnitude, 1, 15f);
                break;
            case PlayerMovementState.boosting:
                acceleration = 10f;
                break;
            case PlayerMovementState.crouching:
                clampedMagnitude = Vector2.ClampMagnitude((Vector2)speed, maxSprintSpeed).magnitude;
                acceleration = SpeedFunction(clampedMagnitude, 0.45f, 10f);
                break;
            case PlayerMovementState.sliding:
                // clampedMagnitude = Vector2.ClampMagnitude((Vector2)speed, maxSprintSpeed).magnitude;
                // acceleration = SpeedFunction(clampedMagnitude, 1.2f, 12.5f) - 35f;
                // acceleration = -baseMovementAcceleration + 0.1f;
                float currentTime = Time.time;
                float difference = currentTime - startedSliding;
                if (difference < 0.1){
                    acceleration = -5 * difference + 20;
                    slideCapSet = false;
                } else {
                    acceleration = 0.5f;
                    slideCapSet = true;
                }
                break;
            default:
                acceleration = 0f;
                break;
        }
        return acceleration + baseMovementAcceleration;
    }
    private void Movement() {
        float maxSpeed = MaxSpeed();
        float acceleration = CalculateAccelerationMultiplier();
        Vector3 target = player.rotation * new Vector3(inputDirection.x, 0, inputDirection.y);
        float alignment = Vector3.Dot(groundVelocity.normalized, target.normalized);

        if (groundVelocity.magnitude > maxSpeed) {
            acceleration *= groundVelocity.magnitude / maxSpeed;
        }
        Vector3 direction = target * maxSpeed - groundVelocity;
        float directionMag = direction.magnitude;
        direction = Vector3.ProjectOnPlane(direction, groundNormal).normalized * directionMag;

        if (direction.magnitude < 0.5f) // If moving very slowly
        {
            acceleration *= direction.magnitude / 0.5f;
        }

        if (alignment <= 0){ // If attempting to move in a wildly opposite direction
            acceleration *= 2;
        }

        direction = direction.normalized * acceleration;
        direction -= direction * FrictionMultiplier();

        player.AddForce(direction * Time.deltaTime, ForceMode.VelocityChange);
    }
    private void Jump(InputAction.CallbackContext inputType){
        /*
        Jumping should provide a forward boost in the input direction held.
        Jumping in mid air should also you to change direction.
        TODO Test if double jumping backwards at high speeds changes directions
        */
        if (isGrounded){
            player.AddForce(Vector3.up * jumpForce + new Vector3(0, velocity.y, 0), ForceMode.VelocityChange);
        }
    }
    private void Boost(InputAction.CallbackContext inputType){
        movementState = PlayerMovementState.boosting;
        preBoostVelocity = groundVelocity.magnitude < maxSprintSpeed ? maxSprintSpeed : groundVelocity.magnitude;
        inputDirection = playerInputs.Player.Movement.ReadValue<Vector2>().normalized;

        Vector3 movement = player.rotation * new Vector3(inputDirection.x, 0, inputDirection.y) * groundVelocity.magnitude * (boostMultiplier - 1);
        maxBoostSpeed = velocity.magnitude + movement.magnitude;
        player.AddForce(movement, ForceMode.VelocityChange);
    }
    public void CollisionDetected(Collision collision){ // This function is called externally by the body
        if (collision.contacts.Length > 0) {
            foreach (ContactPoint contact in collision.contacts) {
                float slopeAngle = Vector3.Angle(contact.normal, Vector3.up);
                
                if (slopeAngle > maxSlope){
                    // uh do something
                } else{
                    isGrounded = true;
                    groundNormal = contact.normal;
                }
            }
        } else if (collision.contacts.Length == 0) {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
    }
    /*
    private bool GroundCheck(){
        return Physics.CheckSphere(
            new Vector3(player.transform.position.x, player.transform.position.y - groundDistance + (0.99f * player.transform.localScale.x * playerCollider.radius) - (player.transform.localScale.y * playerCollider.height)/2, player.transform.position.z),
            player.transform.localScale.x * playerCollider.radius * 0.99f,
            playerLayer);
    }
    */
}