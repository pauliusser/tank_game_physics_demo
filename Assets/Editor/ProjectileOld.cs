using UnityEngine;

public class ProjectileOld : MonoBehaviour
{
    public Transform endPoint;
    public Transform dirPoint;
    void Start()
    {
        // Time.timeScale = 0.25f;
        // Time.fixedDeltaTime = 0.02f * Time.timeScale;
        Vector3 start = transform.position;
        Vector3 end = endPoint.position;
        Vector3 dir = dirPoint.position;
        float deltaX = end.x - start.x;
        float deltaZ = end.z - start.z;
        float horDist = Mathf.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
        Debug.Log($"horizontal distance D start to end: {horDist}");
        float vertH = dir.y - start.y;
        Debug.Log($"distance H start to dir: {vertH}");
        float deltaH = dir.y - end.y;
        Debug.Log($"delta h dir to end: {deltaH}");
        float gravity = -Physics.gravity.y;
        Debug.Log($"gravity: {gravity}");
        float velocity = Mathf.Sqrt(gravity * (horDist * horDist + vertH * vertH) / (2 * deltaH));
        Rigidbody rb = GetComponent<Rigidbody>();
        float mass = rb.mass;
        Debug.Log($"mass: {mass}");
        float forceMagnitude = velocity * mass;
        Debug.Log($"force magnitude: {forceMagnitude}");
        Vector3 velocityVector = (dir - start).normalized * velocity;
        rb.linearVelocity = velocityVector;

    }

}
