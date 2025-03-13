using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public PlayerManager manager;
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
    [SerializeField] public float maxSlope;
    [SerializeField] public Vector3 groundNormal = Vector3.up;

    [Header("Jumping")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpForce = 3.5f;
    [SerializeField] private int airJumpsTotal = 1;
    [SerializeField] private int airJumpsLeft = 1;

    [Header("Movement")]
    [SerializeField] public Vector3 movement;
    [SerializeField] public PlayerMovementState movementState;
    [SerializeField] public PlayerMovementState lastMovementState = PlayerMovementState.idle;
    [SerializeField] private float friction = 0.9f;
    [SerializeField] private float drag = 0.01f;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private Vector3 surfaceVelocity;
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

    [Header("Wallrunning")]
    [SerializeField] private Vector3 wallNormal;
    [SerializeField] private bool stuckToWall;
    [SerializeField] private float wallSeparationAngle = 30f;
    [SerializeField] private float wallFallSpeed = 2f;
    [SerializeField] private float separationBoost = 2f;
    [SerializeField] private float gravityMultiplier = 0.1f;


    [Header("Speed Caps")]
    [SerializeField] private float maxCrouchSpeed = 4.5f;
    [SerializeField] private float maxSlidingSpeed = 15f;
    [SerializeField] private float maxWalkSpeed = 7.5f;
    [SerializeField] private float maxSprintSpeed = 15f;
    [SerializeField] private float maxBoostSpeed;
    [SerializeField] private float maxWallrunningSpeed = 30f;

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
        manager = PlayerManager.Instance;
        playerLayer = LayerMask.GetMask("Standable");

        player = body.GetComponent<Rigidbody>();
        playerCollider = body.GetComponent<CapsuleCollider>();
        SetPlayerDimensions(standingHeight, playerRadius);

        playerInputs = manager.inputs;

        playerInputs.Player.Jump.performed += Jump;
        playerInputs.Player.Boost.performed += Boost;

        maxSlope = GlobalConstants.maxSlope;

        movementState = PlayerMovementState.idle;
        airJumpsLeft = airJumpsTotal;
    }
    private void Update() {
        velocity = player.linearVelocity;
        if (movementState == PlayerMovementState.wallrunning){
            surfaceVelocity = Vector3.ProjectOnPlane(velocity, wallNormal);
        } else{
            surfaceVelocity = Vector3.ProjectOnPlane(velocity, groundNormal);
        }
        inputDirection = playerInputs.Player.Movement.ReadValue<Vector2>().normalized;
        bool crouching = CrouchControlState();

        //isGrounded = GroundCheck();
        SetMovementState(crouching);
        
        if (movementState == PlayerMovementState.wallrunning){
            holdCrouch = false;
            toggleCrouch = false;
            lastHoldCrouchState = false;
            Wallrun();
        } else {
            Crouch();
            Movement();
        }
        if (inputDirection.magnitude == 0 && surfaceVelocity.magnitude < 0.1f){
            velocity.x = 0;
            velocity.z = 0;
        }
        lastMovementState = movementState;
    }
    private void FixedUpdate() {
        Gravity();
        player.AddForce(movement, ForceMode.VelocityChange);
        movement = Vector3.zero;
    }
    private void SetMovementState(bool isCrouched){
        if (stuckToWall){
            movementState = PlayerMovementState.wallrunning;
        } else if (surfaceVelocity.magnitude <= maxWalkSpeed && isCrouched && movementState != PlayerMovementState.sliding){
            movementState = PlayerMovementState.crouching;
            wasSliding = false;
        } else if (surfaceVelocity.magnitude > maxWalkSpeed && isCrouched){
            movementState = PlayerMovementState.sliding;
            wasSliding = false;
        } else if ((surfaceVelocity.magnitude < preBoostVelocity - 1 && surfaceVelocity.magnitude > minSpeed && surfaceVelocity.magnitude < maxWalkSpeed && !playerInputs.Player.Sprint.inProgress && movementState == PlayerMovementState.boosting) || (surfaceVelocity.magnitude > minSpeed && surfaceVelocity.magnitude <= maxWalkSpeed && !playerInputs.Player.Sprint.inProgress && movementState != PlayerMovementState.boosting)){
            movementState = PlayerMovementState.walking;
            wasSliding = false;
        } else if ((surfaceVelocity.magnitude < preBoostVelocity - 1 && surfaceVelocity.magnitude > minSpeed && surfaceVelocity.magnitude < maxSprintSpeed && playerInputs.Player.Sprint.inProgress && movementState == PlayerMovementState.boosting) || (surfaceVelocity.magnitude > minSpeed && surfaceVelocity.magnitude <= maxSprintSpeed && playerInputs.Player.Sprint.inProgress && movementState != PlayerMovementState.boosting)){
            movementState = PlayerMovementState.sprinting;
            if (!wasSliding){
                wasSliding = true;
                startedSliding = Time.time;
                maxSlidingSpeed = surfaceVelocity.magnitude + 10f;
            }
            if (slideCapSet){
                maxSlidingSpeed = surfaceVelocity.magnitude;
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
            movement += (Physics.gravity / -9.81f * gravity) * Time.fixedDeltaTime;
            //player.AddForce((Physics.gravity / -9.81f * gravity) * Time.deltaTime, ForceMode.VelocityChange);
        } else if (stuckToWall){
            movement += (Physics.gravity / -9.81f * gravity * gravityMultiplier) * Time.fixedDeltaTime;
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
            return 1-friction;//SpeedFunction(Mathf.Clamp(surfaceVelocity.magnitude, 0, 1.414f), 1, 1) - 1;
        } else {
            return SpeedFunction(Mathf.Clamp(surfaceVelocity.magnitude, 0, 1f), 1, 1) - 1;
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
                return surfaceVelocity.magnitude;
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
            speed = surfaceVelocity;
        }
        float clampedMagnitude = Vector2.ClampMagnitude((Vector2)speed, MaxSpeed()).magnitude;
        float acceleration;
        switch (movementState){
            case PlayerMovementState.walking:
                acceleration = SpeedFunction(clampedMagnitude, 0.75f, 10);
                break;
            case PlayerMovementState.sprinting:
                acceleration = SpeedFunction(clampedMagnitude, 1, 15f);
                break;
            case PlayerMovementState.boosting:
                acceleration = 10f;
                break;
            case PlayerMovementState.crouching:
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
            case PlayerMovementState.wallrunning:
                acceleration = SpeedFunction(clampedMagnitude, 1.5f, 20f);
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
        //float alignment = Vector3.Dot(surfaceVelocity.normalized, target.normalized);

        //* Above checked for jittering cause
        if (surfaceVelocity.magnitude > maxSpeed) {
            acceleration *= surfaceVelocity.magnitude / maxSpeed;
        }
        Vector3 direction = target * maxSpeed - surfaceVelocity;
        float directionMag = direction.magnitude;
        direction = Vector3.ProjectOnPlane(direction, groundNormal).normalized * directionMag;
        /*
        if (direction.magnitude < 0.5f) // If moving very slowly
        {
            acceleration *= direction.magnitude / 0.5f;
        } 

        if (alignment <= 0){ // If attempting to move in a wildly opposite direction
            acceleration *= 2;
        }*/

        direction = direction.normalized * acceleration;
        direction -= direction * FrictionMultiplier();
        /*
        Vector2 directionGround = new Vector2(direction.x, direction.z);
        if (directionGround.magnitude * Time.deltaTime < 0 && -directionGround.magnitude * Time.deltaTime > surfaceVelocity.magnitude){
            Debug.Log("did this");
            direction.x = -surfaceVelocity.x/Time.deltaTime;
            direction.z = -surfaceVelocity.y/Time.deltaTime;
        }*/

        movement += direction * Time.deltaTime;
        //player.AddForce(direction * Time.deltaTime, ForceMode.VelocityChange);
    }
    private void Wallrun(){
        Vector3 target = player.rotation * new Vector3(inputDirection.x, 0, inputDirection.y);
        if (Mathf.Abs(Vector3.Angle(wallNormal, target)) < wallSeparationAngle){
            stuckToWall = false;
            movement += wallNormal * separationBoost * Time.deltaTime;
            movementState = PlayerMovementState.walking;
            return;
        } else if (player.linearVelocity.magnitude < wallFallSpeed){
            stuckToWall = false;
            movement += wallNormal * separationBoost * Time.deltaTime * 0.1f;
            movementState = PlayerMovementState.walking;
            return;
        }

        //TODO add movement on wall
        target = Vector3.ProjectOnPlane(target, wallNormal);
        Vector3 wallVelocity = Vector3.ProjectOnPlane(player.linearVelocity, wallNormal);
        
        movement += wallVelocity * Time.deltaTime;

        //todo rotate camera away from wall
    }
    private void Jump(InputAction.CallbackContext inputType){
        /*
        Vector3 direction = inputDirection == Vector2.zero ? surfaceVelocity.normalized : player.rotation * new Vector3(inputDirection.x, 0, inputDirection.y);
        direction.y += 1;
        
        direction *= jumpForce;
        if (movementState == PlayerMovementState.wallrunning){
            direction += -wallNormal * jumpForce * 2;
        }
        float alignment = Vector3.Dot(surfaceVelocity.normalized, inputDirection);
        if (alignment < 0){
            direction += -surfaceVelocity;
        }*/
        Vector3 direction = Vector3.up * jumpForce;
        if (movementState == PlayerMovementState.wallrunning){
            direction += wallNormal * jumpForce; // Make sure to push the player away from the wall
            direction = direction.normalized * jumpForce;
            if (velocity.y < 0){
                direction.y -= velocity.y;
            }
            movement += direction; //you should probably comment the rest of your code, they kinda expect you to do that you know for marks
            return;
        }

        if (velocity.y < 0){ // If the player is moving downwards, provide enough force to move them back upwards
            direction.y -= velocity.y;
        }
        if (isGrounded){
            Debug.Log("Jumped");
            movement += direction;
            //player.AddForce(direction, ForceMode.VelocityChange);
        } else if (airJumpsLeft > 0){
            movement += direction;
            //player.AddForce(direction, ForceMode.VelocityChange);
            airJumpsLeft--;
            Debug.Log($"Air jumped, jumps left: {airJumpsLeft}");
        }
    }
    private void Boost(InputAction.CallbackContext inputType){
        movementState = PlayerMovementState.boosting;
        preBoostVelocity = surfaceVelocity.magnitude < maxSprintSpeed ? maxSprintSpeed : surfaceVelocity.magnitude;
        inputDirection = playerInputs.Player.Movement.ReadValue<Vector2>().normalized;

        Vector3 boostMovement = player.rotation * new Vector3(inputDirection.x, 0, inputDirection.y) * surfaceVelocity.magnitude * (boostMultiplier - 1);
        maxBoostSpeed = velocity.magnitude + boostMovement.magnitude;
        movement += boostMovement;
        //player.AddForce(boostMovement, ForceMode.VelocityChange);
    }
    public void CollisionDetected(Collision collision){ // This function is called externally by the body
        if (collision.contacts.Length > 0) {
            foreach (ContactPoint contact in collision.contacts) {
                float slopeAngle = Vector3.Angle(contact.normal, Vector3.up);
                
                airJumpsLeft = airJumpsTotal;
                if (slopeAngle <= maxSlope){
                    isGrounded = true;
                    groundNormal = contact.normal;
                    break;
                } else{
                    stuckToWall = true;
                    wallNormal = contact.normal;
                    Wallrun();
                }
            }
        } else {
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