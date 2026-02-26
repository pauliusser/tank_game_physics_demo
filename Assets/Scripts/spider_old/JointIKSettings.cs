using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class JointAxisConstraint
{
    [Range(-180, 180)] public float minAngle = -90f;
    [Range(-180, 180)] public float maxAngle = 90f;
    [Range(0, 1)] public float stiffness = 0.5f;
    
    public bool IsConstrained => minAngle > -180 || maxAngle < 180;
}

[ExecuteInEditMode]
public class JointIKSettings : MonoBehaviour
{
    [Header("Rotation Constraints")]
    public JointAxisConstraint xAxis = new JointAxisConstraint();
    public JointAxisConstraint yAxis = new JointAxisConstraint();
    public JointAxisConstraint zAxis = new JointAxisConstraint();
    
    [Header("Spring Properties")]
    [Range(0, 1)] public float positionStiffness = 0.5f;
    [Range(0, 1)] public float rotationStiffness = 0.5f;
    
    [Header("Debug")]
    public bool showGizmos = true;
    [Range(0, 1)] public float gizmoSize = 0.1f;
    
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // Draw rotation limit cones
        DrawRotationLimits();
    }
    
    private void DrawRotationLimits()
    {
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        
        if (xAxis.IsConstrained)
            DrawAngleCone(Vector3.right, xAxis.minAngle, xAxis.maxAngle);
        
        if (yAxis.IsConstrained)
            DrawAngleCone(Vector3.up, yAxis.minAngle, yAxis.maxAngle);
        
        if (zAxis.IsConstrained)
            DrawAngleCone(Vector3.forward, zAxis.minAngle, zAxis.maxAngle);
    }
    
    private void DrawAngleCone(Vector3 axis, float minAngle, float maxAngle)
    {
        float range = Mathf.Abs(maxAngle - minAngle);
        if (range < 0.1f) return;
        
        Vector3 localAxis = transform.TransformDirection(axis).normalized;
        Vector3 up = Mathf.Abs(Vector3.Dot(localAxis, Vector3.up)) > 0.9f ? Vector3.forward : Vector3.up;
        Vector3 baseDir = Vector3.ProjectOnPlane(up, localAxis).normalized;
        
        int segments = 24;
        float radius = Mathf.Tan(range * 0.5f * Mathf.Deg2Rad) * gizmoSize;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * 360f * Mathf.Deg2Rad;
            float angle2 = ((i + 1) / (float)segments) * 360f * Mathf.Deg2Rad;
            
            Quaternion rot1 = Quaternion.AngleAxis(minAngle + range * 0.5f, localAxis) *
                            Quaternion.AngleAxis(0, baseDir);
            Quaternion rot2 = Quaternion.AngleAxis(minAngle + range * 0.5f, localAxis) *
                            Quaternion.AngleAxis(360f / segments, baseDir);
            
            Vector3 dir1 = rot1 * baseDir;
            Vector3 dir2 = rot2 * baseDir;
            
            Gizmos.DrawLine(transform.position, transform.position + dir1 * gizmoSize);
            Gizmos.DrawLine(transform.position, transform.position + dir2 * gizmoSize);
            Gizmos.DrawLine(transform.position + dir1 * gizmoSize, 
                           transform.position + dir2 * gizmoSize);
        }
    }
}