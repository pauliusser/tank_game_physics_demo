using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

public class MyIKSolver : MonoBehaviour
{
    public int debug = 0;
    [Header("Chain Configuration")]
    public int links = 6;

    [Header("IK Targets")]
    public Transform target;

    [Header("Solver Properties")]
    [Range(0.001f, 0.5f)] public float tolerance = 0.01f;
    [Range(0f, 1f)] public float posePreservation = 1f;
    public bool startAlignedToTarget = true;
    [Range(1, 20)] public int iterationsPerFrame = 10;

    [Header("Debug")]
    public bool drawDebugGizmos = true;
    public Color chainColor = Color.cyan;
    public Color boneLengthColor = Color.yellow;

    // Internal state
    private List<Vector3> initialLocalPositions = new List<Vector3>();
    private List<Quaternion> initialLocalRotations = new List<Quaternion>();
    private List<Transform> jointChain = new List<Transform>();
    private List<float> boneLengths = new List<float>();
    private List<Vector3> angles = new List<Vector3>();  // (localEulerAngles)
    private float totalChainLength = 0f;
    private float[] weights;
    private Vector3 previousRelativeTargetPosition;

    void Start()
    {
        InitializeChain();
        StoreInitialPose();
        DebugChainData();
        CalculateWeights();
    }

    void Update()
    {   
        Vector3 currentRelativeTargetPosition = GetRelativeTargetPosition();
        if (target != null && currentRelativeTargetPosition != previousRelativeTargetPosition)
        {
            if (posePreservation > 0) resetChainToInitialPose();
            if (startAlignedToTarget) AlighnChainTo(target);
            float endEffectorToTargetDist = Vector3.Distance(jointChain[jointChain.Count - 1].position, target.position);
            if (endEffectorToTargetDist > tolerance)
            {
                for (int i = 0; i < iterationsPerFrame; i++)
                {
                    SolveFabrikIkBackwards(1 - posePreservation);
                    SolveFabricIkForwards(1 - posePreservation);
                }
            }
        }
        previousRelativeTargetPosition = currentRelativeTargetPosition;
    }
    Vector3 GetRelativeTargetPosition()
    {
        // target position in root parent space
        return jointChain[0].parent.InverseTransformPoint(target.position);
    }
    void SolveFabricIkForwards(float weightRelaxation = 0f)
    {
        // Debug.Log("====== FORWARDS FABRIK SOLVER START ======");
        // calculate translation from end effector to target
        Vector3 endEffectorPos = jointChain[jointChain.Count - 1].position;
        Vector3 translationToTarget = target.position - endEffectorPos;
        jointChain[0].position += translationToTarget; // chain end is moved to target
        for (int i = 0; i < jointChain.Count - 1 - debug; i++)
        {
            jointChain[i].localPosition = initialLocalPositions[i]; // preserve initial local position
            // Calculate rotation to align current bone direction with next bone position
            float calculatedWeight = Mathf.Lerp(weights[i], 1f, weightRelaxation);
            Vector3 weightedPosition = Vector3.Lerp(
                GetBoneEndPosition(i), 
                jointChain[i + 1].position, 
                calculatedWeight);
            Quaternion jointRotation = GetRotationToAlignBAtoBC(
                GetBoneEndPosition(i),
                jointChain[i].position,
                weightedPosition);
            // Apply rotation to current joint
            jointChain[i].rotation = jointRotation * jointChain[i].rotation;
        }
        // Debug.Log("====== FORWARDS FABRIK SOLVER END ======");
    }
    void SolveFabrikIkBackwards(float weightRelaxation = 0f)
    {
        jointChain[jointChain.Count - 1].position = target.position;
        // Debug.Log("====== BACKWARDS FABRIK SOLVER START ======");
        for (int i = jointChain.Count - 2; i >= debug; i--)
        {
            // Debug.Log($"FABRIK Backwards - Adjusting Joint {i}");
            float calculatedWeight = Mathf.Lerp(weights[i], 1f, weightRelaxation);
            Vector3 weightedPosition = Vector3.Lerp(
                GetBoneEndPosition(i), 
                jointChain[i+1].transform.position, 
                calculatedWeight);
            quaternion jointRotation = GetRotationToAlignBAtoBC(
                GetBoneEndPosition(i),
                jointChain[i].position,
                weightedPosition);
            Vector3 jointEndPosition = jointChain[i].position + jointChain[i].forward * boneLengths[i];
            Quaternion chainRotation = GetRotationToAlignBAtoBC(
                jointEndPosition,
                jointChain[0].position,
                jointChain[i + 1].position);
            jointChain[0].rotation = chainRotation * jointChain[0].rotation;
            jointChain[i].rotation = jointRotation * jointChain[i].rotation;
            jointChain[i + 1].rotation = Quaternion.Inverse(jointRotation) * jointChain[i + 1].rotation;
            jointChain[i + 1].localPosition = initialLocalPositions[i + 1];
            Vector3 translation = target.position - jointChain[jointChain.Count - 1].position;
            jointChain[i].position += translation;
            // AlighnChainTo(target);
        }
        jointChain[0].localPosition = initialLocalPositions[0];
        AlighnChainTo(target);
        // Debug.Log("====== BACKWARDS FABRIK SOLVER END ======");
    }
    void PositionJointTo(int jointIndex, Transform target)
    {
        Vector3 translation = measureTranslation(jointIndex, target.position);
        jointChain[jointIndex].position += translation;
    }
    Vector3 measureTranslation(int jointIndex, Vector3 targetPosition)
    {
        Vector3 jointEndPosition = GetBoneEndPosition(jointIndex);
        return targetPosition - jointEndPosition;
    }
    Vector3 GetBoneEndPosition(int jointIndex)
    {
        return jointChain[jointIndex].position + jointChain[jointIndex].forward * boneLengths[jointIndex];
    }
    void AlighnChainTo(Transform target)
    {
       Quaternion chainRotation = GetRotationToAlignBAtoBC(
        jointChain[jointChain.Count - 1].position,
        jointChain[0].position,
        target.position);

        // Apply rotation to the chain root
        jointChain[0].rotation = chainRotation * jointChain[0].rotation;
    }

    Quaternion GetRotationToAlignBAtoBC(Vector3 A, Vector3 B, Vector3 C)
    {
        Vector3 dirBA = (A - B).normalized;
        Vector3 dirBC = (C - B).normalized;
        return Quaternion.FromToRotation(dirBA, dirBC);
    }

    void StoreInitialPose()
    {
        initialLocalPositions.Clear();
        initialLocalRotations.Clear();
        
        foreach (Transform joint in jointChain)
        {
            initialLocalPositions.Add(joint.localPosition); // Stores VALUE
            initialLocalRotations.Add(joint.localRotation); // Stores VALUE
            // Debug.Log($"Stored initial local position {joint.localPosition:F3} and rotation {joint.localRotation.eulerAngles:F1} for joint {joint.name}");
        }
    }
    
    void resetChainToInitialPose()
    {
        for (int i = 0; i < jointChain.Count; i++)
        {
            jointChain[i].localPosition = initialLocalPositions[i];
            jointChain[i].localRotation = initialLocalRotations[i];
        }
    }
    void CalculateWeights()
    {
        // Debug.Log("Calculating weights for FABRIK solver...");
        // float distSum = 0f;
        // // Weights are based on cumulative distance from the end effector
        // weights = new float[jointChain.Count - 1];
        // for (int i = jointChain.Count - 2; i >= 0; i--)
        // {
        //     distSum += boneLengths[i];
        //     weights[i] = distSum / totalChainLength;
        // }

        // foreach (float w in weights)
        // {
            // Debug.Log($"Distance Weight: {w:F3}");
        // }
        float[] reachabilities = new float[jointChain.Count - 2];
        for (int i = 0; i < jointChain.Count - 2; i++)
        {
            if (i == 0) 
            {
                reachabilities[i] = boneLengths[i]; // Root joint has full reachability
                continue;
            } 
            // sum of bone lenghts before joint i
            float sumBoneLengthsBeforeI = 0f;
            for (int j = 0; j < i; j++)
            {
                sumBoneLengthsBeforeI += boneLengths[j];
            }

            // sum of bone lenghts after joint i
            float sumBoneLengthsAfterI = 0f;
            for (int j = i; j < boneLengths.Count; j++)
            {
                sumBoneLengthsAfterI += boneLengths[j];
            }

            // max distance joint i can reach
            float jointReach = Mathf.Min(sumBoneLengthsBeforeI, sumBoneLengthsAfterI) * 2;
            reachabilities[i] = jointReach;
        }
        weights = new float[jointChain.Count - 1];
        for (int i = 0; i < weights.Length - 1; i++){
            weights[i] = reachabilities[i] / totalChainLength;
            // Debug.Log($"Joint {i} Reachability: {reachabilities[i]:F3}, Weight: {weights[i]:F3}");
        }
    }
    void InitializeChain()
    {
        // Clear previous data
        jointChain.Clear();
        boneLengths.Clear();
        angles.Clear();
        totalChainLength = 0f;

        // Start from current transform and follow children
        Transform current = transform;
        
        for (int i = 0; i < links; i++)
        {
            if (current == null)
            {
                Debug.LogWarning($"Chain broke at link {i}: null transform encountered");
                break;
            }

            // Add current joint to chain
            jointChain.Add(current);
            // Store initial local rotation as euler angles
            angles.Add(current.localEulerAngles);

            // Calculate bone length (distance to child, if exists)
            if (i < links - 1 && current.childCount > 0)
            {
                Transform child = current.GetChild(0);
                float length = Vector3.Distance(current.position, child.position);
                boneLengths.Add(length);
                totalChainLength += length;
                
                // Move to next joint
                current = child;
            }
            else
            {
                // Last joint in chain or no child
                if (i < links - 1)
                {
                    // If we expected more links but no child exists
                    boneLengths.Add(0f);
                    Debug.LogWarning($"Link {i} has no child, using bone length 0");
                }
                break;
            }
        }

        // If we collected fewer links than requested
        if (jointChain.Count < links)
        {
            Debug.LogWarning($"Requested {links} links but only found {jointChain.Count} in hierarchy");
        }
    }

    void DebugChainData()
    {
        Debug.Log("=== IK CHAIN INITIALIZATION ===");
        Debug.Log($"Root: {transform.name}");
        Debug.Log($"Target: {(target != null ? target.name : "None")}");
        Debug.Log($"Found {jointChain.Count} joints, {boneLengths.Count} bones");
        Debug.Log($"Total chain length: {totalChainLength:F3}");
        
        for (int i = 0; i < jointChain.Count; i++)
        {
            string jointInfo = $"Joint {i}: {jointChain[i].name}";
            jointInfo += $"\n  World Pos: {jointChain[i].position:F3}";
            jointInfo += $"\n  Local Euler: {angles[i]:F1}";
            
            if (i < boneLengths.Count)
            {
                jointInfo += $"\n  Bone Length: {boneLengths[i]:F3}";
                
                if (i < jointChain.Count - 1)
                {
                    Transform child = jointChain[i + 1];
                    float actualDistance = Vector3.Distance(jointChain[i].position, child.position);
                    float storedLength = boneLengths[i];
                    
                    if (Mathf.Abs(actualDistance - storedLength) > 0.001f)
                    {
                        Debug.LogWarning($"  WARNING: Mismatch! Actual distance: {actualDistance:F3}, Stored: {storedLength:F3}");
                    }
                }
            }
            else if (i == jointChain.Count - 1)
            {
                jointInfo += "\n  [END EFFECTOR - No bone length]";
            }
            
            Debug.Log(jointInfo);
        }
        
        Debug.Log("=== END INITIALIZATION ===");
    }

    void OnDrawGizmos()
    {
        if (!drawDebugGizmos) return;
        
        // Draw bone chain
        for (int i = 0; i < jointChain.Count; i++)
        {
            if (jointChain[i] == null) continue;
            
            // Draw joint sphere
            Gizmos.color = chainColor;
            float sphereSize = 0.05f * (1f - (i / (float)jointChain.Count) * 0.5f);
            Gizmos.DrawSphere(jointChain[i].position, sphereSize);
            
            // Draw bone to next joint
            if (i < jointChain.Count - 1 && jointChain[i + 1] != null)
            {
                Gizmos.color = boneLengthColor;
                Gizmos.DrawLine(jointChain[i].position, jointChain[i + 1].position);
                
                // Draw bone length indicator
                Vector3 midPoint = Vector3.Lerp(jointChain[i].position, jointChain[i + 1].position, 0.5f);
                Vector3 direction = (jointChain[i + 1].position - jointChain[i].position).normalized;
                float boneLength = Vector3.Distance(jointChain[i].position, jointChain[i + 1].position);
                
                // Create a small offset to avoid overlapping with bone line
                Vector3 offset = Vector3.Cross(direction, Vector3.up).normalized * 0.02f;
                UnityEditor.Handles.Label(midPoint + offset, $"{boneLength:F2}", 
                    new GUIStyle() { normal = new GUIStyleState() { textColor = boneLengthColor } });
            }
            
            // Draw coordinate axes for each joint
            if (jointChain[i] != null)
            {
                float axisLength = 0.1f;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(jointChain[i].position, jointChain[i].position + jointChain[i].right * axisLength);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(jointChain[i].position, jointChain[i].position + jointChain[i].up * axisLength);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(jointChain[i].position, jointChain[i].position + jointChain[i].forward * axisLength);
            }
        }
        
        // Draw target if assigned
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, 0.07f);
            
            // Line from end effector to target
            if (jointChain.Count > 0)
            {
                Transform endEffector = jointChain[jointChain.Count - 1];
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                Gizmos.DrawLine(endEffector.position, target.position);
                
                // Distance label
                float distance = Vector3.Distance(jointChain[0].transform.position, target.position);
                Vector3 labelPos = Vector3.Lerp(endEffector.position, target.position, 0.5f);
                UnityEditor.Handles.Label(labelPos + Vector3.up * 0.05f, 
                    $"Dist: {distance:F2}\nReachable: {(distance <= totalChainLength ? "YES" : "NO")}");
            }
        }
    }

    // Public API for debugging and testing
    public void Reinitialize()
    {
        InitializeChain();
        DebugChainData();
    }

    public string GetChainSummary()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"IK Chain Summary:");
        sb.AppendLine($"- Joints: {jointChain.Count}");
        sb.AppendLine($"- Total Length: {totalChainLength:F3}");
        sb.AppendLine($"- Target: {(target != null ? target.name : "None")}");
        
        for (int i = 0; i < Mathf.Min(3, jointChain.Count); i++)
        {
            sb.AppendLine($"  {i}: {jointChain[i].name}");
        }
        
        if (jointChain.Count > 3)
        {
            sb.AppendLine($"  ... ({jointChain.Count - 3} more)");
        }
        
        return sb.ToString();
    }

    // Helper to validate chain integrity
    public bool ValidateChain()
    {
        if (jointChain.Count == 0) return false;
        
        for (int i = 0; i < jointChain.Count; i++)
        {
            if (jointChain[i] == null)
            {
                Debug.LogError($"Joint {i} is null!");
                return false;
            }
        }
        
        for (int i = 0; i < boneLengths.Count; i++)
        {
            if (boneLengths[i] <= 0)
            {
                Debug.LogWarning($"Bone {i} has invalid length: {boneLengths[i]}");
            }
        }
        
        return true;
    }
}