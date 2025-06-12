using UnityEngine;

public class PathManager : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform[] waypoints;
    public bool showPath = true;
    public Color pathColor = Color.red;
    
    void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            CreateDefaultPath();
        }
    }
    
    void CreateDefaultPath()
    {
        GameObject waypointParent = new GameObject("Waypoints");
        waypointParent.transform.SetParent(transform);
        
        Vector3[] defaultPositions = {
            new Vector3(-8, 0, 0),
            new Vector3(-4, 0, 0),
            new Vector3(-4, 0, 4),
            new Vector3(0, 0, 4),
            new Vector3(0, 0, -4),
            new Vector3(4, 0, -4),
            new Vector3(4, 0, 0),
            new Vector3(8, 0, 0)
        };
        
        waypoints = new Transform[defaultPositions.Length];
        
        for (int i = 0; i < defaultPositions.Length; i++)
        {
            GameObject waypoint = new GameObject("Waypoint_" + i);
            waypoint.transform.position = defaultPositions[i];
            waypoint.transform.SetParent(waypointParent.transform);
            waypoints[i] = waypoint.transform;
        }
    }
    
    public Transform[] GetWaypoints()
    {
        return waypoints;
    }
    
    void OnDrawGizmos()
    {
        if (!showPath || waypoints == null || waypoints.Length < 2) return;
        
        Gizmos.color = pathColor;
        
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);
            }
        }
        
        if (waypoints[waypoints.Length - 1] != null)
        {
            Gizmos.DrawWireSphere(waypoints[waypoints.Length - 1].position, 0.3f);
        }
    }
}