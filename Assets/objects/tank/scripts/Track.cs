// using UnityEngine;

// public class Track : MonoBehaviour
// {
//     public Transform waypointA;
//     public Transform waypointB;
//     public Transform waypointC;
//     public Transform waypointD;
//     public GameObject trackLinkPrefab;
//     public float trackProgress;
//     public bool isRight = true;
//     public float initialAngle = 180f;
//     public int linksCount = 45;
//     public TrackLinkControl[] tlc;
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void OnEnable()
//     {
//         tlc = new TrackLinkControl[linksCount];
//         for (int i = 0; i < linksCount; i++)
//         {
//             GameObject link = Instantiate(trackLinkPrefab, transform);
//             TrackLinkControl controller = link.GetComponent<TrackLinkControl>();
//             controller.tr = GetComponent<Track>();
//             controller.pathOffset = (float)i / linksCount * 100;
//             tlc[i] = controller;
//         }
//     }
// }


using UnityEngine;

public class Track : MonoBehaviour
{
    public Transform waypointA;
    public Transform waypointB;
    public Transform waypointC;
    public Transform waypointD;
    public GameObject trackLinkPrefab;
    public float trackProgress;
    public bool isRight = true;
    public float initialAngle = 180f;
    public int linksCount = 45;
    public TrackLinkControl[] tlc;
    public float totalLen = 0f;
    // Calculated data that will be shared with TrackLinkControl instances
    public Vector3[] waypoints { get; private set; }
    public float[] sectionLengths { get; private set; }
    public float[] lengthsFromStart { get; private set; }
    public float[] angles { get; private set; }
    public float radius { get; private set; } = 0.13f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        // Calculate all the track data once
        CalculateTrackData();
        
        // Initialize track links
        tlc = new TrackLinkControl[linksCount];
        for (int i = 0; i < linksCount; i++)
        {
            GameObject link = Instantiate(trackLinkPrefab, transform);
            TrackLinkControl controller = link.GetComponent<TrackLinkControl>();
            controller.tr = this; // Pass reference to this track
            controller.pathOffset = (float)i / linksCount * 100;
            tlc[i] = controller;
        }
    }
    
    private void CalculateTrackData()
    {
        // Convert waypoint positions to local space
        Vector3 wA = waypointA.localPosition;
        Vector3 wB = waypointB.localPosition;
        Vector3 wC = waypointC.localPosition;
        Vector3 wD = waypointD.localPosition;
        
        waypoints = new Vector3[]{wA, wB, wC, wD};
        
        // Debug.Log($"wA:{wA} wB:{wB} wC:{wC} wD:{wD}");
        
        // Calculate distances between waypoints
        float dAB = (wB - wA).magnitude;
        float dBC = (wC - wB).magnitude;
        float dCD = (wD - wC).magnitude;
        float dDA = (wA - wD).magnitude;
        
        // Debug.Log($"dAB:{dAB} dBC:{dBC} dCD:{dCD} dDA:{dDA}");
        // Debug.Log($"linear path length:{dAB + dBC + dCD + dDA}");
        
        // Calculate corner angles
        float aA = Mathf.PI - Vector3.Angle(wD - wA, wB - wA) * Mathf.Deg2Rad;
        float aB = Mathf.PI - Vector3.Angle(wA - wB, wC - wB) * Mathf.Deg2Rad;
        float aC = Mathf.PI - Vector3.Angle(wB - wC, wD - wC) * Mathf.Deg2Rad;
        float aD = Mathf.PI - Vector3.Angle(wC - wD, wA - wD) * Mathf.Deg2Rad;
        
        // Debug.Log($"angles A:{aA} B:{aB} C:{aC} D:{aD}");
        // Debug.Log($"angles check = 2 * PI?:{aA + aB + aC + aD}");
        
        // Calculate arc lengths
        float arcA = aA * radius;
        float arcB = aB * radius;
        float arcC = aC * radius;
        float arcD = aD * radius;
        
        // Debug.Log($"arc lengths A:{arcA} B:{arcB} C:{arcC} D:{arcD}");
        // Debug.Log($"arc length sum:{arcA + arcB + arcC + arcD}");
        
        // Calculate total path length
        totalLen = arcA + arcB + arcC + arcD + dAB + dBC + dCD + dDA;
        // Debug.Log($"total outside path length: {totalLen}");
        
        // Calculate section lengths (normalized)
        sectionLengths = new float[]{
            arcA / totalLen, dAB / totalLen,
            arcB / totalLen, dBC / totalLen,
            arcC / totalLen, dCD / totalLen,
            arcD / totalLen, dDA / totalLen};
        
        // Debug logging for section lengths
        // int sIndex = 0;
        // foreach (var len in sectionLengths)
        // {
        //     Debug.Log($"section {sIndex} length = {len}");
        //     sIndex++;
        // }
        
        // Calculate cumulative lengths from start
        lengthsFromStart = new float[sectionLengths.Length];
        float lSum = 0f;
        for (int i = 0; i < sectionLengths.Length; i++)
        {
            lSum += sectionLengths[i];
            lengthsFromStart[i] = lSum;
            // Debug.Log($"section {i} length = {sectionLengths[i]}");
        }
        
        // Calculate angle deltas and cumulative angles
        float[] anglesDelta = new float[]{0, aA, 0, aB, 0, aC, 0, aD, 0};
        // Debug.Log($"is it 1?:{sectionLengths.Sum()}");
        
        angles = new float[anglesDelta.Length];
        float aSum = 0f;
        for (int i = 0; i < anglesDelta.Length; i++)
        {
            aSum += anglesDelta[i];
            angles[i] = aSum;
            // Debug.Log($"angle {i} = {aSum}");
        }
    }
}