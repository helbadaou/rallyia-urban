using UnityEngine;

/// <summary>
/// Detects whether an object is on a road surface using raycasting.
/// Attach to any object that needs to know if it's on a road.
/// </summary>
public class RoadDetector : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("How far above the ground to cast the ray down.")]
    public float rayOriginHeight = 2f;

    [Tooltip("Maximum raycast distance downward.")]
    public float maxRayDistance = 5f;

    [Tooltip("Layer mask for road surfaces. If empty, detects all colliders.")]
    public LayerMask roadLayerMask = ~0; // Everything by default

    [Header("State")]
    [ReadOnly] public bool isOnRoad = false;
    [ReadOnly] public float roadFriction = 1f;  // 1.0 = road, 0.3 = off-road

    private Vector3 lastRoadNormal = Vector3.up;

    /// <summary>
    /// Check if the object is currently on a road surface.
    /// Call in FixedUpdate or when you need to check.
    /// </summary>
    public bool CheckOnRoad(Vector3 worldPosition)
    {
        Vector3 origin = worldPosition + Vector3.up * rayOriginHeight;
        Ray ray = new Ray(origin, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance + rayOriginHeight, roadLayerMask))
        {
            // Check if the hit object has a "Road" tag or layer
            bool onRoadSurface = hit.collider.CompareTag("Road")
                               || hit.collider.gameObject.layer == LayerMask.NameToLayer("Road")
                               || HasRoadMaterial(hit.collider);

            isOnRoad = onRoadSurface;
            roadFriction = onRoadSurface ? 1f : 0.3f;
            lastRoadNormal = hit.normal;

            return onRoadSurface;
        }

        isOnRoad = false;
        roadFriction = 0.3f;
        return false;
    }

    /// <summary>
    /// Get the surface normal at the last raycast hit.
    /// Useful for aligning the car to slopes.
    /// </summary>
    public Vector3 GetSurfaceNormal()
    {
        return lastRoadNormal;
    }

    /// <summary>
    /// Check if a collider has a road-related material name.
    /// Fallback detection when tags/layers aren't set up.
    /// </summary>
    private bool HasRoadMaterial(Collider col)
    {
        Renderer renderer = col.GetComponent<Renderer>();
        if (renderer == null || renderer.material == null) return false;

        string matName = renderer.material.name.ToLower();
        return matName.Contains("road") || matName.Contains("asphalt")
            || matName.Contains("street") || matName.Contains("pavement")
            || matName.Contains("sidewalk") || matName.Contains("lane");
    }

    void OnDrawGizmosSelected()
    {
        // Visualize the raycast in the editor
        Vector3 origin = transform.position + Vector3.up * rayOriginHeight;
        Gizmos.color = isOnRoad ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * (maxRayDistance + rayOriginHeight));
    }
}

/// <summary>
/// Custom attribute to make fields read-only in the Inspector.
/// </summary>
public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : UnityEditor.PropertyDrawer
{
    public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        UnityEditor.EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}
#endif
