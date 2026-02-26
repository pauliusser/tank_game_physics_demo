using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class UniversalArcIK : MonoBehaviour
{
    [Header("Leg Hierarchy")]
    public Transform[] segments;
    public Transform footTarget;
    private float[] segmentAngles;   
    private float[] segmentLengths;
    private float totalLength;

    void Start()
    {
        calculateSegmentLengths();
        setSegmentsAnglesAtStart();
    }

    void Update()
    {
        // segments[0].rotation = Quaternion.identity; 
        foreach (var seg in segments)
        {
            seg.rotation = Quaternion.identity; 
        }
        LookAtAxisOnly(segments[0], transform.position, "Y");
        CalculateIK();
        
    }
    void CalculateIK()
    {
        LookAtAxisOnly(segments[0], transform.position, "X");
        if (totalLength < Vector3.Distance(segments[0].position, transform.position)) return;
        segmentLengths[segmentLengths.Length - 1] = Vector3.Distance(segments[0].position, transform.position);
        segmentAngles = CyclicPolygonSolver.ComputeInteriorAngles(segmentLengths);
        for (int i = 0; i < segmentAngles.Length; i++)
        {
            if (i == 0)
            {
                segments[i].localEulerAngles = new Vector3(segments[i].localEulerAngles.x - segmentAngles[i] , segments[i].localEulerAngles.y, segments[i].localEulerAngles.z);
                continue;
            }
            segments[i].localEulerAngles = new Vector3(180 - segmentAngles[i], 0f, 0f);
        }
    }
    void setSegmentsAnglesAtStart()
    {
        segmentAngles = new float[segments.Length]; // +1 for ik target distance

        for (int i = 0; i < segments.Length; i++)
        {
            segmentAngles[i] = 0f;
        }
    }

    void calculateSegmentLengths()
    {
        if (segments == null || segments.Length == 0)
            return;
        
        segmentLengths = new float[segments.Length];
        Debug.Log($"Segment array size: {segments.Length}");
        Debug.Log($"Segment lengths array size: {segmentLengths.Length}");
        totalLength = 0f;

        for (int i = 0; i < segmentLengths.Length; i++)
        {
            if (i == segments.Length - 1)
            {
                segmentLengths[i] = Vector3.Distance(segments[i].position, transform.position);
            }
            else
            {
                segmentLengths[i] = Vector3.Distance(segments[i].position, segments[i+1].position);
                totalLength += segmentLengths[i];
            }


            Debug.Log($"Segment {i} length: {segmentLengths[i]}");
        }
        Debug.Log($"Total leg length: {totalLength}");
    }
    
    // public void LookAtAxisOnly(Transform t, Vector3 target, string axis)
    // {
    //     Vector3 rotAxis = axis.ToUpper() == "X" ? t.right : axis.ToUpper() == "Y" ? t.up : t.forward;
    //     Vector3 dirProj = Vector3.ProjectOnPlane(target - t.position, rotAxis);
    //     Vector3 fwdProj = Vector3.ProjectOnPlane(t.forward, rotAxis);
        
    //     if (dirProj.magnitude < 0.001f || fwdProj.magnitude < 0.001f) return;
        
    //     float angle = Vector3.SignedAngle(fwdProj, dirProj, rotAxis);
    //     Vector3 euler = t.localEulerAngles;
    //     if (axis.ToUpper() == "X") euler.x += angle;
    //     else if (axis.ToUpper() == "Y") euler.y += angle;
    //     else euler.z += angle;
    //     t.localEulerAngles = euler;
    // }   
    public void LookAtAxisOnly(Transform t, Vector3 target, string axis)
    {
        string upperAxis = axis.ToUpper();
        
        // Store current rotation
        Vector3 currentEuler = t.localEulerAngles;
        
        // Make the object look at target (all axes)
        t.LookAt(target);
        
        // Get the new rotation
        Vector3 newEuler = t.localEulerAngles;
        
        // Restore locked axes to their original values
        if (upperAxis == "X")
        {
            // Only keep X rotation, restore Y and Z
            newEuler.y = currentEuler.y;
            newEuler.z = currentEuler.z;
        }
        else if (upperAxis == "Y")
        {
            // Only keep Y rotation, restore X and Z
            newEuler.x = currentEuler.x;
            newEuler.z = currentEuler.z;
        }
        else // "Z"
        {
            // Only keep Z rotation, restore X and Y
            newEuler.x = currentEuler.x;
            newEuler.y = currentEuler.y;
        }
        
        // Apply the filtered rotation
        t.localEulerAngles = newEuler;
    }
}