using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Helper class to find the closest point on NavMesh to any GameObject
/// </summary>
public static class NavMeshPointFinder
{
    /// <summary>
    /// Finds the closest point on NavMesh to the given GameObject
    /// </summary>
    /// <param name="gameObject">The GameObject to find closest NavMesh point for</param>
    /// <param name="searchRadius">How far to search for NavMesh (default: 2f, same as your AINavigationController)</param>
    /// <param name="areaMask">Which NavMesh areas to consider (default: NavMesh.AllAreas)</param>
    /// <returns>Closest point on NavMesh, or original position if none found</returns>
    public static Vector3 GetClosestNavMeshPoint(GameObject gameObject, float searchRadius = 2f, int areaMask = NavMesh.AllAreas)
    {
        if (gameObject == null)
        {
            Debug.LogError("NavMeshPointFinder: GameObject is null!");
            return Vector3.zero;
        }

        Vector3 worldPosition = gameObject.transform.position;
        
        // Sample position on NavMesh (same method your AINavigationController uses)
        if (NavMesh.SamplePosition(worldPosition, out NavMeshHit hit, searchRadius, areaMask))
        {
            return hit.position;
        }
        
        // Optional: Try raycast to ground as fallback
        Debug.LogWarning($"NavMeshPointFinder: No NavMesh found within {searchRadius} units of {gameObject.name}. Returning original position.");
        return worldPosition;
    }
    
    /// <summary>
    /// Finds the closest point on NavMesh with option to return success status
    /// </summary>
    /// <param name="gameObject">The GameObject to find closest NavMesh point for</param>
    /// <param name="result">The closest NavMesh point if found</param>
    /// <param name="searchRadius">How far to search for NavMesh</param>
    /// <param name="areaMask">Which NavMesh areas to consider</param>
    /// <returns>True if NavMesh point found, false otherwise</returns>
    public static bool TryGetClosestNavMeshPoint(GameObject gameObject, out Vector3 result, float searchRadius = 2f, int areaMask = NavMesh.AllAreas)
    {
        result = gameObject != null ? gameObject.transform.position : Vector3.zero;
        
        if (gameObject == null)
        {
            Debug.LogError("NavMeshPointFinder: GameObject is null!");
            return false;
        }
        
        if (NavMesh.SamplePosition(gameObject.transform.position, out NavMeshHit hit, searchRadius, areaMask))
        {
            result = hit.position;
            return true;
        }
        
        return false;
    }
}