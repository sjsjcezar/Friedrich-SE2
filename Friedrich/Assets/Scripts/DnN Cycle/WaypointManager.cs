using UnityEngine;
using System.Collections.Generic;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance { get; private set; }

    [Tooltip("List of all waypoints in the scene")]
    public List<Transform> allWaypoints = new List<Transform>();

    private Dictionary<string, Transform> waypointDictionary = new Dictionary<string, Transform>();

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
       // DontDestroyOnLoad(this.gameObject);

        // Populate the dictionary with waypoint names and their Transforms
        foreach (Transform waypoint in allWaypoints)
        {
            if (waypoint != null && !waypointDictionary.ContainsKey(waypoint.name))
            {
                waypointDictionary.Add(waypoint.name, waypoint);
            }
            else
            {
                Debug.LogWarning($"WaypointManager: Duplicate or null waypoint name detected: {waypoint?.name}");
            }
        }
    }

    /// <summary>
    /// Retrieves a waypoint Transform by name.
    /// </summary>
    /// <param name="name">Name of the waypoint.</param>
    /// <returns>Transform of the waypoint, or null if not found.</returns>
    public Transform GetWaypointByName(string name)
    {
        if (waypointDictionary.TryGetValue(name, out Transform waypoint))
        {
            return waypoint;
        }
        else
        {
            Debug.LogError($"WaypointManager: Waypoint with name '{name}' not found.");
            return null;
        }
    }
}