using UnityEngine;

public class IKJointConstraint : MonoBehaviour
{
    [Header("Rotation Authority")]
    [Range(0.1f, 2f)]
    public float rotationAuthority = 1f;
    
    [Header("Angle Constraints (Local Space)")]
    public Vector3 minAngles = new Vector3(-90, -90, -90);
    public Vector3 maxAngles = new Vector3(90, 90, 90);
    
    void OnValidate()
    {
        // Ensure min <= max
        minAngles.x = Mathf.Min(minAngles.x, maxAngles.x);
        minAngles.y = Mathf.Min(minAngles.y, maxAngles.y);
        minAngles.z = Mathf.Min(minAngles.z, maxAngles.z);
    }
    
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;
        
        // Visualize constraint limits
        float radius = 0.1f;
        Vector3 position = transform.position;
        
        // Draw local axes with constraint limits
        Gizmos.color = Color.red;
        Vector3 xAxis = transform.TransformDirection(Vector3.right);
        Gizmos.DrawLine(position, position + Quaternion.Euler(minAngles.x, 0, 0) * xAxis * radius);
        Gizmos.DrawLine(position, position + Quaternion.Euler(maxAngles.x, 0, 0) * xAxis * radius);
        
        Gizmos.color = Color.green;
        Vector3 yAxis = transform.TransformDirection(Vector3.up);
        Gizmos.DrawLine(position, position + Quaternion.Euler(0, minAngles.y, 0) * yAxis * radius);
        Gizmos.DrawLine(position, position + Quaternion.Euler(0, maxAngles.y, 0) * yAxis * radius);
        
        Gizmos.color = Color.blue;
        Vector3 zAxis = transform.TransformDirection(Vector3.forward);
        Gizmos.DrawLine(position, position + Quaternion.Euler(0, 0, minAngles.z) * zAxis * radius);
        Gizmos.DrawLine(position, position + Quaternion.Euler(0, 0, maxAngles.z) * zAxis * radius);
    }
}
