using UnityEngine;

/// <summary>
/// Top-down car controller with WASD/Arrow key input.
/// Movement is physics-based (Rigidbody) with road detection.
/// Attach to the car GameObject (needs Rigidbody + Collider).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Forward/backward speed.")]
    public float moveSpeed = 15f;

    [Tooltip("How fast the car accelerates.")]
    public float acceleration = 20f;

    [Tooltip("How fast the car decelerates when no input.")]
    public float deceleration = 15f;

    [Tooltip("Maximum speed.")]
    public float maxSpeed = 25f;

    [Header("Turning")]
    [Tooltip("How fast the car rotates to face movement direction.")]
    public float turnSpeed = 8f;

    [Tooltip("Minimum speed before the car starts rotating (prevents spinning in place).")]
    public float minTurnSpeed = 0.5f;

    [Header("Road Constraint")]
    [Tooltip("Penalty multiplier when off-road (0 = stop, 0.3 = slow).")]
    [Range(0f, 1f)]
    public float offRoadSpeedMultiplier = 0.3f;

    [Tooltip("Extra drag applied when off-road.")]
    public float offRoadDrag = 3f;

    [Header("Debug")]
    [ReadOnly] public bool isOnRoad = true;
    [ReadOnly] public float currentSpeed = 0f;

    private Rigidbody rb;
    private RoadDetector roadDetector;
    private Vector3 moveInput;
    private float currentRotationY;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Lower center of mass to prevent flipping
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);

        // Setup rigidbody for top-down movement
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;

        // Add road detector if not already present
        roadDetector = GetComponent<RoadDetector>();
        if (roadDetector == null)
            roadDetector = gameObject.AddComponent<RoadDetector>();

        // Tag as Player for camera follow
        if (!gameObject.CompareTag("Player"))
            gameObject.tag = "Player";

        currentRotationY = transform.eulerAngles.y;
    }

    void Update()
    {
        // Gather input in Update for responsiveness
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Normalize so diagonal movement isn't faster
        moveInput = new Vector3(horizontal, 0f, vertical).normalized;
    }

    void FixedUpdate()
    {
        // Check road status
        isOnRoad = roadDetector.CheckOnRoad(transform.position);

        // Calculate effective speed
        float speedMultiplier = isOnRoad ? 1f : offRoadSpeedMultiplier;
        float effectiveMaxSpeed = maxSpeed * speedMultiplier;

        // Apply movement
        if (moveInput.magnitude > 0.1f)
        {
            // Accelerate toward input direction
            Vector3 desiredVelocity = moveInput * moveSpeed * speedMultiplier;
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                desiredVelocity,
                acceleration * Time.fixedDeltaTime
            );

            // Rotate to face movement direction
            float targetAngle = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg;
            currentRotationY = Mathf.LerpAngle(
                currentRotationY,
                targetAngle,
                turnSpeed * Time.fixedDeltaTime
            );
        }
        else
        {
            // Decelerate when no input
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                Vector3.zero,
                deceleration * Time.fixedDeltaTime
            );
        }

        // Clamp speed
        if (rb.linearVelocity.magnitude > effectiveMaxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * effectiveMaxSpeed;

        // Apply off-road drag
        if (!isOnRoad)
        {
            rb.linearDamping = offRoadDrag;
        }
        else
        {
            rb.linearDamping = 0f;
        }

        // Apply rotation (only when moving)
        currentSpeed = rb.linearVelocity.magnitude;
        if (currentSpeed > minTurnSpeed)
        {
            transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
        }

        // Keep car on the ground plane
        Vector3 pos = transform.position;
        pos.y = 0f;
        transform.position = pos;
    }

    /// <summary>
    /// Set the car's position and rotation programmatically.
    /// Useful for spawning or resetting.
    /// </summary>
    public void SetPosition(Vector3 position, float rotationY)
    {
        transform.position = new Vector3(position.x, 0f, position.z);
        currentRotationY = rotationY;
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
        rb.linearVelocity = Vector3.zero;
    }

    /// <summary>
    /// Force the car to a specific position on a road.
    /// Snaps to the nearest road surface.
    /// </summary>
    public void SnapToRoad(Vector3 worldPosition)
    {
        // Raycast down to find the road surface
        Ray ray = new Ray(worldPosition + Vector3.up * 10f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 20f))
        {
            SetPosition(hit.point, transform.eulerAngles.y);
        }
        else
        {
            SetPosition(worldPosition, transform.eulerAngles.y);
        }
    }
}
