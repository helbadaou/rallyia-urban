using UnityEngine;

/// <summary>
/// Smooth top-down camera that follows a target (car/character) from above.
/// Attach to an empty GameObject or the Main Camera.
/// Uses Vector3.SmoothDamp for jitter-free following.
/// </summary>
public class TopDownCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The car or character to follow. If null, searches for 'Player' tag.")]
    public Transform target;

    [Header("Camera Settings")]
    [Tooltip("Orthographic size (half the vertical view height in world units).")]
    public float orthographicSize = 30f;

    [Tooltip("Height above the ground plane.")]
    public float height = 50f;

    [Header("Smoothing")]
    [Tooltip("How quickly the camera catches up to the target. Lower = smoother.")]
    public float smoothTime = 0.15f;

    [Tooltip("Offset from the target's position.")]
    public Vector3 offset = Vector3.zero;

    [Header("Bounds (optional)")]
    [Tooltip("Clamp camera to stay within city bounds. Uncheck for no limits.")]
    public bool useBounds = false;
    public float minX = -100f, maxX = 100f;
    public float minZ = -100f, maxZ = 100f;

    private Camera cam;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = gameObject.AddComponent<Camera>();

        // Setup orthographic camera for top-down view
        cam.orthographic = true;
        cam.orthographicSize = orthographicSize;
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Straight down

        // Auto-find target if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Desired position: directly above the target
        Vector3 desiredPos = target.position + offset;
        desiredPos.y = height;

        // Smooth follow
        Vector3 smoothed = Vector3.SmoothDamp(
            transform.position, desiredPos, ref velocity, smoothTime
        );

        // Optional: clamp to city bounds
        if (useBounds)
        {
            smoothed.x = Mathf.Clamp(smoothed.x, minX, maxX);
            smoothed.z = Mathf.Clamp(smoothed.z, minZ, maxZ);
        }

        // Keep fixed Y (height)
        smoothed.y = height;

        transform.position = smoothed;

        // Keep looking straight down
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    /// <summary>
    /// Call this to snap the camera instantly (no smoothing) to the target.
    /// Useful when starting a scene or respawning.
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;
        Vector3 pos = target.position + offset;
        pos.y = height;
        transform.position = pos;
        velocity = Vector3.zero;
    }

    /// <summary>
    /// Update camera bounds based on the city model's actual size.
    /// Call after loading/importing the city model.
    /// </summary>
    public void SetBoundsFromRenderer(Bounds cityBounds)
    {
        minX = cityBounds.min.x;
        maxX = cityBounds.max.x;
        minZ = cityBounds.min.z;
        maxZ = cityBounds.max.z;
        useBounds = true;

        // Auto-adjust orthographic size to fit the city
        float halfHeight = cityBounds.size.z * 0.5f;
        float halfWidth = (cityBounds.size.x * 0.5f) / cam.aspect;
        orthographicSize = Mathf.Max(halfHeight, halfWidth) * 1.1f;
        cam.orthographicSize = orthographicSize;
    }
}
