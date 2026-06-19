using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages multiple AI-driven cars that drive along road waypoints.
/// Cars loop continuously without stopping. Each car follows a path.
/// Attach to an empty "TrafficManager" GameObject.
/// </summary>
public class TrafficManager : MonoBehaviour
{
    [Header("Traffic Settings")]
    [Tooltip("Prefabs to use as traffic cars (will be instantiated).")]
    public GameObject[] carPrefabs;

    [Tooltip("Number of traffic cars to spawn.")]
    public int carCount = 10;

    [Tooltip("Minimum speed for traffic cars.")]
    public float minSpeed = 8f;

    [Tooltip("Maximum speed for traffic cars.")]
    public float maxSpeed = 18f;

    [Header("Road Paths")]
    [Tooltip("Manually assign waypoint paths. Each path is a Transform array.")]
    public RoadPath[] roadPaths;

    [Header("Spawn Settings")]
    [Tooltip("Spawn cars at random positions on paths if no spawn points set.")]
    public Transform[] spawnPoints;

    private List<TrafficCar> activeCars = new List<TrafficCar>();

    [System.Serializable]
    public class RoadPath
    {
        public string pathName = "Road";
        public Transform[] waypoints;
    }

    [System.Serializable]
    private class TrafficCar
    {
        public GameObject gameObject;
        public Transform transform;
        public int currentPathIndex;
        public int currentWaypointIndex;
        public float speed;
        public float progress;
    }

    void Start()
    {
        SpawnTraffic();
    }

    void SpawnTraffic()
    {
        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            Debug.LogWarning("TrafficManager: No car prefabs assigned!");
            return;
        }

        if (roadPaths == null || roadPaths.Length == 0)
        {
            Debug.LogWarning("TrafficManager: No road paths assigned!");
            return;
        }

        for (int i = 0; i < carCount; i++)
        {
            // Pick a random path and random prefab
            int pathIdx = i % roadPaths.Length;
            int prefabIdx = Random.Range(0, carPrefabs.Length);

            // Spawn at a random waypoint on the path
            RoadPath path = roadPaths[pathIdx];
            if (path.waypoints == null || path.waypoints.Length < 2) continue;

            int startWaypoint = Random.Range(0, path.waypoints.Length);
            Vector3 spawnPos = path.waypoints[startWaypoint].position;

            GameObject car = Instantiate(carPrefabs[prefabIdx], spawnPos, Quaternion.identity);
            car.name = $"Traffic_{i}_{path.pathName}";

            TrafficCar trafficCar = new TrafficCar
            {
                gameObject = car,
                transform = car.transform,
                currentPathIndex = pathIdx,
                currentWaypointIndex = startWaypoint,
                speed = Random.Range(minSpeed, maxSpeed),
                progress = 0f
            };

            activeCars.Add(trafficCar);
        }

        Debug.Log($"TrafficManager: Spawned {activeCars.Count} cars on {roadPaths.Length} paths");
    }

    void Update()
    {
        foreach (var car in activeCars)
        {
            if (car.gameObject == null) continue;
            MoveCar(car);
        }
    }

    void MoveCar(TrafficCar car)
    {
        RoadPath path = roadPaths[car.currentPathIndex];
        if (path.waypoints.Length < 2) return;

        int nextIdx = (car.currentWaypointIndex + 1) % path.waypoints.Length;
        Transform current = path.waypoints[car.currentWaypointIndex];
        Transform next = path.waypoints[nextIdx];

        // Move toward next waypoint
        float segmentLength = Vector3.Distance(current.position, next.position);
        if (segmentLength < 0.01f)
        {
            // Skip zero-length segments
            car.currentWaypointIndex = nextIdx;
            return;
        }

        car.progress += (car.speed * Time.deltaTime) / segmentLength;

        if (car.progress >= 1f)
        {
            car.progress -= 1f;
            car.currentWaypointIndex = nextIdx;
        }

        // Interpolate position
        Vector3 newPos = Vector3.Lerp(current.position, next.position, car.progress);
        newPos.y = 0f; // Keep on ground
        car.transform.position = newPos;

        // Face direction of travel
        Vector3 direction = (next.position - current.position).normalized;
        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            car.transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    /// <summary>
    /// Stop all traffic (e.g., for cutscenes or pause).
    /// </summary>
    public void StopAllTraffic()
    {
        foreach (var car in activeCars)
        {
            if (car.gameObject != null)
                car.speed = 0f;
        }
    }

    /// <summary>
    /// Resume all traffic.
    /// </summary>
    public void ResumeAllTraffic()
    {
        foreach (var car in activeCars)
        {
            if (car.gameObject != null)
                car.speed = Random.Range(minSpeed, maxSpeed);
        }
    }

    /// <summary>
    /// Add a car to an existing path at runtime.
    /// </summary>
    public void AddCar(GameObject prefab, int pathIndex, int startWaypoint = 0)
    {
        if (pathIndex < 0 || pathIndex >= roadPaths.Length) return;

        RoadPath path = roadPaths[pathIndex];
        if (path.waypoints.Length < 2) return;

        Vector3 spawnPos = path.waypoints[startWaypoint].position;
        GameObject car = Instantiate(prefab, spawnPos, Quaternion.identity);

        TrafficCar trafficCar = new TrafficCar
        {
            gameObject = car,
            transform = car.transform,
            currentPathIndex = pathIndex,
            currentWaypointIndex = startWaypoint,
            speed = Random.Range(minSpeed, maxSpeed),
            progress = 0f
        };

        activeCars.Add(trafficCar);
    }

    /// <summary>
    /// Get the count of active traffic cars.
    /// </summary>
    public int GetActiveCarCount()
    {
        return activeCars.Count;
    }
}
