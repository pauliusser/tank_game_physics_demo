using UnityEngine;

public class NavPatrolOnWaypoints : MonoBehaviour
{
    public GameObject patrolGameObject;  // The tank to patrol
    public Transform[] waypoints;        // Array of waypoint positions
    public int currentWaypointIndex = 0;
    
    // Reference to the tank's movement script
    private TankMovement tankMovement;
    
    void Start()
    {
        // Get reference to the tank's movement script
        if (patrolGameObject != null)
        {
            tankMovement = patrolGameObject.GetComponent<TankMovement>();
        }
    }
    
    // void OnTriggerEnter(Collider other)
    void OnTriggerEnter(Collider other)
    {
        // Check if the tank entered the trigger
        if (other.gameObject == patrolGameObject && waypoints.Length > 0)
        {
            // Debug.Log($"arived at: {currentWaypointIndex} waypoint");
            // Move to next waypoint
            currentWaypointIndex++;
            
            // Loop back to start if at the end
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
            transform.position = waypoints[currentWaypointIndex].transform.position;
        }
    }
}