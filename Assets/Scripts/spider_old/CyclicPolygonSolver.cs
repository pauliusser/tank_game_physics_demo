using UnityEngine;

public class CyclicPolygonSolver : MonoBehaviour
{
    [Header("Input Edge Lengths")]
    public float[] edgeLengths = { 1f, 2f, 3f, 5.5f };
    
    [Header("Settings")]
    public float epsilon = 0.0001f;
    public int maxIterations = 100;
    
    [Header("Debug Output")]
    public bool showDebugInfo = true;
    
    private const float TWO_PI = Mathf.PI * 2f;
    
    void Start()
    {
        TestSolver();
    }
    
    [ContextMenu("Run Solver")]
    public void TestSolver()
    {
        float[] interiorAngles = ComputeInteriorAngles(edgeLengths);
        
        if (interiorAngles != null && showDebugInfo)
        {
            Debug.Log("=== Cyclic Polygon Solver Results ===");
            Debug.Log($"Number of edges: {edgeLengths.Length}");
            Debug.Log("Edge lengths: " + string.Join(", ", edgeLengths));
            
            // Display interior angles with correct vertex mapping
            Debug.Log("Interior angles at each vertex (degrees):");
            float sum = 0f;
            for (int i = 0; i < interiorAngles.Length; i++)
            {
                sum += interiorAngles[i];
                // Debug.Log($"  Vertex {i}: {interiorAngles[i]:F2}° (between edges { i - 1 < 0 ? interiorAngles.Length-1:i-1} and {i})");
            }
            Debug.Log($"Sum of interior angles: {sum:F2}° (Expected: {(edgeLengths.Length - 2) * 180}°)");
        }
    }
    
    public static float[] ComputeInteriorAngles(float[] lengths)
    {
        if (lengths == null || lengths.Length < 3)
        {
            Debug.LogError($"Need at least 3 edges, got {lengths?.Length ?? 0}");
            return null;
        }
        
        // For triangle (3 edges), we can calculate directly
        if (lengths.Length == 3)
        {
            return ComputeTriangleAngles(lengths);
        }
        
        CyclicPolygonSolver solver = new CyclicPolygonSolver();
        return solver.SolveCyclicPolygon(lengths);
    }
    
    private static float[] ComputeTriangleAngles(float[] lengths)
    {
        // For triangle, use Law of Cosines
        float a = lengths[0], b = lengths[1], c = lengths[2];
        
        float angleA = Mathf.Acos((b * b + c * c - a * a) / (2 * b * c)) * Mathf.Rad2Deg;
        float angleB = Mathf.Acos((a * a + c * c - b * b) / (2 * a * c)) * Mathf.Rad2Deg;
        float angleC = 180f - angleA - angleB;
        
        return new float[] { angleA, angleB, angleC };
    }
    
    private float[] SolveCyclicPolygon(float[] lengths)
    {
        // 1. Find the longest edge
        int longestIndex = FindLongestEdgeIndex(lengths);
        float longestLength = lengths[longestIndex];
        
        // 2. Binary search for the correct central angle of longest edge
        float minAngle = epsilon;
        float maxAngle = TWO_PI - epsilon;
        float targetAngle = 0f;
        
        for (int i = 0; i < maxIterations; i++)
        {
            targetAngle = (minAngle + maxAngle) * 0.5f;
            float angleGap = CalculateTotalAngleGap(lengths, longestIndex, targetAngle);
            
            if (Mathf.Abs(angleGap) < epsilon) break;
            
            if (angleGap < 0f) 
                minAngle = targetAngle;  // Sum too small, need larger angle
            else 
                maxAngle = targetAngle;  // Sum too large, need smaller angle
        }
        
        // 3. Calculate radius from the found angle
        float radius = CalculateRadiusFromAngle(longestLength, targetAngle);
        
        // 4. Calculate all central angles
        float[] centralAngles = CalculateAllCentralAngles(lengths, longestIndex, targetAngle, radius);
        
        // 5. Convert to interior angles (in degrees) - FIXED METHOD
        float[] interiorAngles = ConvertToInteriorAnglesCorrect(centralAngles, radius);
        
        // 6. Debug output
        if (showDebugInfo)
        {
            PrintDebugInfo(lengths, centralAngles, interiorAngles, radius, longestIndex);
        }
        
        return interiorAngles;
    }
    
    private float[] ConvertToInteriorAnglesCorrect(float[] centralAngles, float radius)
    {
        int n = centralAngles.Length;
        float[] interiorAngles = new float[n];
        
        // Build vertices array - vertex 0 is at angle 0
        Vector2[] vertices = new Vector2[n];
        float currentAngle = 0f;
        
        for (int i = 0; i < n; i++)
        {
            vertices[i] = new Vector2(
                Mathf.Cos(currentAngle) * radius,
                Mathf.Sin(currentAngle) * radius
            );
            currentAngle += centralAngles[i];
        }
        
        // Now calculate interior angle at each vertex
        // Vertex i is between edge (i-1) and edge i
        // Where edge j connects vertex j to vertex (j+1)%n
        for (int vertexIndex = 0; vertexIndex < n; vertexIndex++)
        {
            int prevVertex = (vertexIndex - 1 + n) % n;
            int nextVertex = (vertexIndex + 1) % n;
            
            Vector2 v1 = vertices[prevVertex] - vertices[vertexIndex];
            Vector2 v2 = vertices[nextVertex] - vertices[vertexIndex];
            
            // Normalize and calculate angle
            v1.Normalize();
            v2.Normalize();
            
            float dot = Vector2.Dot(v1, v2);
            dot = Mathf.Clamp(dot, -1f, 1f);
            float angleRad = Mathf.Acos(dot);
            
            interiorAngles[vertexIndex] = angleRad * Mathf.Rad2Deg;
        }
        
        return interiorAngles;
    }
    
    private int FindLongestEdgeIndex(float[] lengths)
    {
        int longestIndex = 0;
        float maxLength = lengths[0];
        
        for (int i = 1; i < lengths.Length; i++)
        {
            if (lengths[i] > maxLength)
            {
                maxLength = lengths[i];
                longestIndex = i;
            }
        }
        
        return longestIndex;
    }
    
    private float CalculateTotalAngleGap(float[] lengths, int longestIndex, float testAngle)
    {
        // Calculate radius for this test angle
        float radius = CalculateRadiusFromAngle(lengths[longestIndex], testAngle);
        
        // Sum all central angles
        float angleSum = testAngle; // Start with the longest edge's angle
        
        for (int i = 0; i < lengths.Length; i++)
        {
            if (i == longestIndex) continue;
            angleSum += CalculateCentralAngle(lengths[i], radius);
        }
        
        // Return difference from full circle
        return angleSum - TWO_PI;
    }
    
    private float CalculateRadiusFromAngle(float edgeLength, float centralAngle)
    {
        // From chord length formula: chord = 2R * sin(angle/2)
        // So: R = chord / (2 * sin(angle/2))
        
        float sinHalfAngle = Mathf.Sin(centralAngle * 0.5f);
        
        // Avoid division by zero
        if (sinHalfAngle < epsilon)
        {
            return edgeLength * 0.5f / epsilon;
        }
        
        return edgeLength * 0.5f / sinHalfAngle;
    }
    
    private float CalculateCentralAngle(float edgeLength, float radius)
    {
        // Law of cosines for isosceles triangle: cos(angle) = 1 - (edgeLength²)/(2R²)
        float cosAngle = 1f - (edgeLength * edgeLength) / (2f * radius * radius);
        
        // Clamp to valid range for acos
        cosAngle = Mathf.Clamp(cosAngle, -1f, 1f);
        
        return Mathf.Acos(cosAngle);
    }
    
    private float[] CalculateAllCentralAngles(float[] lengths, int longestIndex, float longestAngle, float radius)
    {
        float[] centralAngles = new float[lengths.Length];
        
        for (int i = 0; i < lengths.Length; i++)
        {
            if (i == longestIndex)
            {
                centralAngles[i] = longestAngle;
            }
            else
            {
                centralAngles[i] = CalculateCentralAngle(lengths[i], radius);
            }
        }
        
        return centralAngles;
    }
    
    private void PrintDebugInfo(float[] lengths, float[] centralAngles, float[] interiorAngles, float radius, int longestIndex)
    {
        Debug.Log("=== Cyclic Polygon Debug Info ===");
        Debug.Log($"Longest edge: index={longestIndex}, length={lengths[longestIndex]}");
        Debug.Log($"Circumradius: {radius}");
        
        Debug.Log("Central Angles (for edges between vertices):");
        float centralSum = 0f;
        for (int i = 0; i < centralAngles.Length; i++)
        {
            centralSum += centralAngles[i];
            int nextVertex = (i + 1) % centralAngles.Length;
            Debug.Log($"  Edge {i} (vertex {i}→{nextVertex}): {centralAngles[i]:F4} rad | {centralAngles[i] * Mathf.Rad2Deg:F2}°");
        }
        Debug.Log($"Sum of central angles: {centralSum:F6} rad (Expected: {TWO_PI:F6})");
        
        Debug.Log("Interior Angles (at vertices):");
        float interiorSum = 0f;
        for (int i = 0; i < interiorAngles.Length; i++)
        {
            interiorSum += interiorAngles[i];
            int prevEdge = (i - 1 + interiorAngles.Length) % interiorAngles.Length;
            Debug.Log($"  Vertex {i} (between edges {prevEdge} and {i}): {interiorAngles[i]:F2}°");
        }
        Debug.Log($"Sum of interior angles: {interiorSum:F2}° (Expected: {(lengths.Length - 2) * 180}°)");
        
        // Validate by reconstructing vertices
        ValidateReconstruction(lengths, centralAngles, radius);
    }
    
    private void ValidateReconstruction(float[] lengths, float[] centralAngles, float radius)
    {
        Debug.Log("=== Validation by Vertex Reconstruction ===");
        
        Vector2[] vertices = new Vector2[lengths.Length];
        float currentAngle = 0f;
        
        // Build vertices
        for (int i = 0; i < lengths.Length; i++)
        {
            vertices[i] = new Vector2(
                Mathf.Cos(currentAngle) * radius,
                Mathf.Sin(currentAngle) * radius
            );
            currentAngle += centralAngles[i];
        }
        
        // Check each edge length
        for (int i = 0; i < lengths.Length; i++)
        {
            int next = (i + 1) % lengths.Length;
            float reconstructedLength = Vector2.Distance(vertices[i], vertices[next]);
            float error = Mathf.Abs(reconstructedLength - lengths[i]);
            
            Debug.Log($"Edge {i} (vertex {i}→{next}): Target={lengths[i]:F4}, Calculated={reconstructedLength:F4}, Error={error:F6}");
        }
        
        // Check polygon closure
        float closingDistance = Vector2.Distance(vertices[lengths.Length - 1], vertices[0]);
        Debug.Log($"Polygon closure error: {closingDistance:F6} (should be ~0)");
    }
    
    // NEW: Visual debug helper in Scene view
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !showDebugInfo) return;
        
        float radius;
        float[] centralAngles = SolveForCentralAngles(edgeLengths, out radius);
        if (centralAngles == null) return;
        
        // Draw circumscribed circle
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        DrawCircleGizmo(Vector3.zero, radius, 32);
        
        // Draw vertices and edges
        Vector3[] vertices = new Vector3[edgeLengths.Length];
        float currentAngle = 0f;
        
        for (int i = 0; i < edgeLengths.Length; i++)
        {
            vertices[i] = new Vector3(
                Mathf.Cos(currentAngle) * radius,
                Mathf.Sin(currentAngle) * radius,
                0
            );
            currentAngle += centralAngles[i];
            
            // Draw vertex
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(vertices[i], 0.1f);
            
            // Draw vertex label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(vertices[i] + Vector3.up * 0.2f, $"V{i}");
            #endif
        }
        
        // Draw edges
        Gizmos.color = Color.blue;
        for (int i = 0; i < edgeLengths.Length; i++)
        {
            int next = (i + 1) % edgeLengths.Length;
            Gizmos.DrawLine(vertices[i], vertices[next]);
            
            // Draw edge label at midpoint
            Vector3 midpoint = (vertices[i] + vertices[next]) / 2f;
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(midpoint, $"E{i}: {edgeLengths[i]:F2}");
            #endif
        }
    }
    
    private void DrawCircleGizmo(Vector3 center, float radius, int segments)
    {
        float angleStep = TWO_PI / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep;
            Vector3 nextPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0
            );
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
    
    private float[] SolveForCentralAngles(float[] lengths, out float radius)
    {
        radius = 0f;
        
        if (lengths == null || lengths.Length < 3) return null;
        
        int longestIndex = FindLongestEdgeIndex(lengths);
        float longestLength = lengths[longestIndex];
        
        float minAngle = epsilon;
        float maxAngle = TWO_PI - epsilon;
        float targetAngle = 0f;
        
        for (int i = 0; i < maxIterations; i++)
        {
            targetAngle = (minAngle + maxAngle) * 0.5f;
            float angleGap = CalculateTotalAngleGap(lengths, longestIndex, targetAngle);
            
            if (Mathf.Abs(angleGap) < epsilon) break;
            
            if (angleGap < 0f) 
                minAngle = targetAngle;
            else 
                maxAngle = targetAngle;
        }
        
        radius = CalculateRadiusFromAngle(longestLength, targetAngle);
        return CalculateAllCentralAngles(lengths, longestIndex, targetAngle, radius);
    }
}