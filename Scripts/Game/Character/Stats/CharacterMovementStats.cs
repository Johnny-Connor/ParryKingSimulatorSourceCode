using UnityEngine;

public class CharacterMovementStats : MonoBehaviour
{
    // Horizontal Movement.
    [Header("Horizontal Movement")]

    [Tooltip("Walk speed in m/s.")]
    [SerializeField] private float _walkSpeed = 4.0f;
    public float WalkSpeed => _walkSpeed;

    [Tooltip("Sprint speed in m/s.")]
    [SerializeField] private float _sprintSpeed = 6.0f;
    public float SprintSpeed => _sprintSpeed;

    [Tooltip("Roll speed in m/s.")]
    [SerializeField] private float _rollSpeed = 8.0f;
    public float RollSpeed => _rollSpeed;

    [Tooltip("Back step speed in m/s.")]
    [SerializeField] private float _backStepSpeed = 5.0f;
    public float BackStepSpeed => _backStepSpeed;

    [Tooltip("The speed in m/s at which the character slides when stepping on non-ground layered objects.")]
    [SerializeField] private float _slideSpeed = 10.0f;
    public float SlideSpeed => _slideSpeed;

    [Tooltip("The speed in m/s at which the character will be pushed by parry stabber during parried stabbed animation.")]
    [SerializeField] private float _parriedStabbedPushMotionSpeed = 2.5f;
    public float ParriedStabbedPushMotionSpeed => _parriedStabbedPushMotionSpeed;

    [Tooltip("The minimum horizontal speed limit in m/s used in air motion states.")]
    [SerializeField] private float _minimumHorizontalAirSpeedLimit = 2.0f;
    public float MinimumHorizontalAirSpeedLimit => _minimumHorizontalAirSpeedLimit;

    [Tooltip("Acceleration and deceleration in m/s² used in ground motion states.")]
    [SerializeField] private float _groundSpeedChangeRate = 10.0f;
    public float GroundSpeedChangeRate => _groundSpeedChangeRate;

    [Tooltip("Acceleration and deceleration in m/s² used in air motion states.")]
    [SerializeField] private float _airSpeedChangeRate = 1.5f;
    public float AirSpeedChangeRate => _airSpeedChangeRate;

    [Tooltip("The duration in seconds during which the character can control his target horizontal speed while in air motion states before it is forcefully set to 0.")]
    [SerializeField] [Range(0.0f, 1.0f)] private float _airHorizontalTargetSpeedControlDuration = 0.75f;
    public float AirHorizontalTargetSpeedControlDuration => _airHorizontalTargetSpeedControlDuration;

    public float HorizontalVelocityMagnitude
    {
        get
        {
            Vector3 _velocity = new Vector3(
                _characterController.velocity.x, 
                0f, 
                _characterController.velocity.z
            );

            return _velocity.magnitude;
        }
    }


    // Vertical Movement.
    [Header("Vertical Movement")]

    [Tooltip("Gravity applied when character is in-air. The engine's default is -9.81f.")]
    [SerializeField] private float _airGravity = -15.0f;
    public float AirGravity => _airGravity;

    [Tooltip("Gravity applied when character is grounded.")]
    [SerializeField] private float _groundGravity = -2f;
    public float GroundGravity => _groundGravity;

    [Tooltip("The height the character can jump.")]
    [SerializeField] private float _jumpHeight = 1.2f;
    public float JumpHeight => _jumpHeight;

    [Tooltip("Time required to pass in jump state before searching ground. Used when character jumps while going up stairs.")]
    [SerializeField] [Range(0.0f, 0.3f)] private float _jumpGroundDetectionTimeout = 0.15f;
    public float JumpGroundDetectionTimeout => _jumpGroundDetectionTimeout;

    [Tooltip("Time required to pass before being able to jump again.")]
    [SerializeField] [Range(0.0f, 0.5f)] private float _jumpCooldown = 0.25f;
    public float JumpCooldown => _jumpCooldown;

    [Tooltip("Time required to pass before entering the fall state. Used to go down stairs.")]
    [SerializeField] [Range(0.0f, 0.4f)] private float _fallTimeout = 0.2f;
    public float FallTimeout => _fallTimeout;

    [Tooltip("The maximum speed at which the character can fall.")]
    [SerializeField] private float _fallSpeedLimit = -53f;
    public float FallSpeedLimit => _fallSpeedLimit;
    

    // Rotation.
    [Header("Rotation")]

    [Tooltip("How smoothly the character turns to face movement direction. Higher values result in slower rotations.")]
    [SerializeField] [Range(0.0f, 0.2f)] private float _rotationSmoothness = 0.1f;
    public float RotationSmoothness => _rotationSmoothness;

    [Tooltip("How many times smoother than rotationSmoothness the character will rotate in-air.")]
    [SerializeField] private int _airRotationSmoothnessMultiplier = 3;
    public float AirRotationSmoothnessMultiplier => _rotationSmoothness * _airRotationSmoothnessMultiplier;

    [Tooltip("Influence of speed when turning in-air. Higher values result in speed having a greater influence in decreasing the turning motion.")]
    [SerializeField] [Range(0.0f, 0.15f)] private float _airRotationSpeedInfluenceFactor = 0.075f;
    public float AirRotationSpeedInfluenceFactor => _airRotationSpeedInfluenceFactor;

    [Tooltip("The time window in seconds during which the character can make a direction choice for rotation after initiating actions that limits turning motion.")]
    [SerializeField] [Range(0.0f, 0.1f)] private float _actionRotationDirectionChoiceTime = 0.05f;
    public float ActionRotationDirectionChoiceTime => _actionRotationDirectionChoiceTime;

    /*
    The speed limit (inclusive) for triggering combat rotation while targeting a target in an air motion
    state.
    */
    public float AirCombatRotationSpeedLimit => _walkSpeed;


    // Ground Check.
    [Header("Ground Check")]

    [Tooltip("The vertical offset of the ground check sphere from the character's transform position. Use this to fine-tune the position of the ground detection.")]
    [SerializeField] private float _groundedOffSet = -0.14f;

    [Tooltip("The radius of the grounded's check sphere. Should match the radius of the CharacterController.")]
    [SerializeField] private float _groundedRadius = 0.28f;

    // The layer the ground check sphere considers as ground.
    private LayerMask _groundLayer = 1;

    public bool IsGrounded {
        get
        {
            Vector3 groundedSpherePosition = new Vector3(
                transform.position.x, 
                transform.position.y - _groundedOffSet,
                transform.position.z
            );

            return Physics.CheckSphere(
                groundedSpherePosition, 
                _groundedRadius, 
                _groundLayer,
                QueryTriggerInteraction.Ignore
            );
        }
    }

    private void DebugIsGrounded()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (IsGrounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // When selected, draw a gizmo in the position of, and matching radius of, the grounded collider.
        Gizmos.DrawSphere(
            new Vector3(
                transform.position.x, 
                transform.position.y - _groundedOffSet, 
                transform.position.z
            ),
            _groundedRadius
        );
    }


    // Slide Check.
    [Header("Slide Check")]

    [Tooltip("The radius of the slide check's check sphere.")]
    [SerializeField] private float _shouldSlideRadius = 0.28f;

    public bool ShouldBeSliding
    {
        get
        {
            int playerLayerID = 7;
            LayerMask playerLayerMask = 1 << playerLayerID;

            int ignoreCollisionsLayerID = 3;
            LayerMask ignoreCollisionsMask = 1 << ignoreCollisionsLayerID;

            LayerMask notSlideLayerMask = _groundLayer | playerLayerMask | ignoreCollisionsMask;

            Vector3 groundedSpherePosition = new Vector3(
                transform.position.x, 
                transform.position.y,
                transform.position.z
            );

            return Physics.CheckSphere(
                groundedSpherePosition, 
                _shouldSlideRadius, 
                ~notSlideLayerMask, // tilde (~) means bitwise flip.
                QueryTriggerInteraction.Ignore
            );
        }
    }

    private void DebugShouldBeSliding()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (ShouldBeSliding) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // When selected, draw a gizmo in the position of, and matching radius of, the grounded collider.
        Gizmos.DrawSphere(
            new Vector3(
                transform.position.x, 
                transform.position.y - _groundedOffSet, 
                transform.position.z
            ),
            _groundedRadius
        );
    }


    // Values that should not be reset when swithing states.
    // Note: floats are passed by value, so these variables must be passed by reference or set directly.

    // Keeps track of the current velocity at which the object is rotating.
    private float _rotationVelocity;
    public ref float RotationVelocity { get => ref _rotationVelocity; }

    // Keeps track of the current vertical velocity at which the character is moving.
    private float _verticalVelocity;
    public float VerticalVelocity {
        get => _verticalVelocity;
        set => _verticalVelocity = value;
    }

    // Keeps track of the current air slide velocity at which the character is moving.
    private float _airSlideVelocity;
    public float AirSlideVelocity {
        get => _airSlideVelocity;
        set => _airSlideVelocity = value;
    }

    /*
    Keeps track of the remaining time during which the character can control his target speed while in an
    air motion state.
    */
    private float _airHorizontalTargetSpeedControlDurationDelta;
    public float AirHorizontalTargetSpeedControlDurationDelta {
        get => _airHorizontalTargetSpeedControlDurationDelta; 
        set => _airHorizontalTargetSpeedControlDurationDelta = value;
    }

    // Keeps track of the fall timeout time left.
    private float _fallTimeoutDelta;
    public float FallTimeoutDelta {
        get => _fallTimeoutDelta;
        set => _fallTimeoutDelta = value;
    }

    // Keeps track of the jump cooldown time left.
    private float _jumpCooldownDelta;
    public float JumpCooldownDelta {
        get => _jumpCooldownDelta;
        set => _jumpCooldownDelta = value;
    }

    /*
    Keeps track of the remaining time during which the character can control his rotation direction after
    initiating certain actions.
    */
    private float _actionRotationDirectionChoiceTimeDelta;
    public float ActionRotationDirectionChoiceTimeDelta {
        get => _actionRotationDirectionChoiceTimeDelta;
        set => _actionRotationDirectionChoiceTimeDelta = value;
    }


    // Components.
    private CharacterController _characterController;
    public CharacterController CharacterController => _characterController;


    // Debug.
    private enum GroundDebugMode
    {
        DebugIsGrounded,
        DebugShouldBeSliding,
    }
    
    [Header("Debug")]
    [SerializeField] private GroundDebugMode _groundDebugMode;


    // MonoBehaviour.
    private void Awake()
    {
        // Setting delta variables.
        _actionRotationDirectionChoiceTimeDelta = _actionRotationDirectionChoiceTime;
        _airHorizontalTargetSpeedControlDurationDelta = _airHorizontalTargetSpeedControlDuration;
        _fallTimeoutDelta = _fallTimeout;
        _jumpCooldownDelta = 0;

        // Setting components.
        _characterController = GetComponent<CharacterController>();
    }

    private void OnDrawGizmosSelected()
    {
        if (_groundDebugMode == GroundDebugMode.DebugIsGrounded)
        {
            DebugIsGrounded();
        }
        else
        {
            DebugShouldBeSliding();
        }
    }
}
