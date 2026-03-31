// using UnityEngine;

// public class TrackLinkControl : MonoBehaviour
// {
//     // private bool isRight = true;
//     private Transform waypointA;
//     private Transform waypointB;
//     private Transform waypointC;
//     private Transform waypointD;
//     public float radius = 0.13f;
//     private float trackProgress = 0f;
//     public float pathOffset = 0f;
//     private float initialAngle = 0f;
//     private Vector3 wA;
//     private Vector3 wB;
//     private Vector3 wC;
//     private Vector3 wD;
//     private float[] sectionLengths;
//     private float[] lengthsFromStart;
//     private float[] anglesDelta;
//     private float[] angles;
//     private Vector3[] waypoints;
//     public Track tr;
//     public TrackContact TrackContact;
//     public bool isTouchingGround;
//     public Vector3 contactFront;
//     public Vector3 contactPosition;
//     private Vector3 finalRotation = Vector3.zero;
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         waypointA = tr.waypointA;
//         waypointB = tr.waypointB;
//         waypointC = tr.waypointC;
//         waypointD = tr.waypointD;
//         trackProgress = tr.trackProgress;
//         initialAngle = tr.initialAngle;        
//         wA = waypointA.localPosition;
//         wB = waypointB.localPosition;
//         wC = waypointC.localPosition;
//         wD = waypointD.localPosition;

//         waypoints = new Vector3[]{wA, wB, wC, wD};
//         // Debug.Log($"wA:{wA} wB:{wB} wC:{wC} wD:{wD}");
//         float dAB = (wB - wA).magnitude;
//         float dBC = (wC - wB).magnitude;
//         float dCD = (wD - wC).magnitude;
//         float dDA = (wA - wD).magnitude;
//         // Debug.Log($"dAB:{dAB} dBC:{dBC} dCD:{dCD} dDA:{dDA}");
//         // Debug.Log($"linear path length:{dAB + dBC + dCD + dDA}");
//         float aA = Mathf.PI - Vector3.Angle(wD - wA, wB - wA) * Mathf.Deg2Rad;
//         float aB = Mathf.PI - Vector3.Angle(wA - wB, wC - wB) * Mathf.Deg2Rad;
//         float aC = Mathf.PI - Vector3.Angle(wB - wC, wD - wC) * Mathf.Deg2Rad;
//         float aD = Mathf.PI - Vector3.Angle(wC - wD, wA - wD) * Mathf.Deg2Rad;
//         // Debug.Log($"angles A:{aA} B:{aB} C:{aC} D:{aD}");
//         // Debug.Log($"angles check = 2 * PI?:{aA + aB + aC + aD}");
//         float arcA = aA * radius;
//         float arcB = aB * radius;
//         float arcC = aC * radius;
//         float arcD = aD * radius;
//         // Debug.Log($"arc lengths A:{arcA} B:{arcB} C:{arcC} D:{arcD}");
//         // Debug.Log($"arc length sum:{arcA + arcB + arcC + arcD}");
//         float totalLen = arcA + arcB + arcC + arcD + dAB + dBC + dCD + dDA;
//         // Debug.Log($"total outside path length: {totalLen}");
//         sectionLengths = new float[]{
//         arcA / totalLen, dAB / totalLen,
//         arcB / totalLen, dBC / totalLen,
//         arcC / totalLen, dCD / totalLen,
//         arcD / totalLen, dDA / totalLen};

//         int sIndex = 0;
//         foreach (var len in sectionLengths)
//         {
//             // Debug.Log($"section {sIndex} length = {len}");
//             sIndex ++;
//         }

//         lengthsFromStart = new float[sectionLengths.Length];
//         float lSum = 0f;
//         for (int i = 0; i < sectionLengths.Length; i++)
//         {
//             lSum += sectionLengths[i];
//             lengthsFromStart[i] = lSum;
//             // Debug.Log($"section {i} length = {sectionLengths[i]}");
//         }
//         anglesDelta = new float[]{0, aA, 0, aB, 0, aC, 0, aD, 0};
//         // Debug.Log($"is it 1?:{sectionLengths.Sum()}");

//         angles = new float[anglesDelta.Length];
//         float aSum = 0f;
//         for (int i = 0; i < anglesDelta.Length; i++)
//         {
//             aSum += anglesDelta[i];
//             angles[i] = aSum;
//             // Debug.Log($"angle {i} = {aSum}");
//         }
//     }
//     int sectionIndex(float val)
// {
//     val %= 1f; // Ensure within [0,1)
//     for (int i = 0; i < lengthsFromStart.Length; i++)
//     {
//         if (val < lengthsFromStart[i]) // Use < instead of <= for first element
//         {
//             return i;
//         }
//     }
//     return 0; // Shouldn't happen if val is in [0,1)
// }

// float sectionPercent(int index, float trackProgress)
// {
//     float sectionLength = sectionLengths[index];
//     float sectionStart = (index == 0) ? 0f : lengthsFromStart[index - 1];
//     float lengthOnSection = trackProgress - sectionStart;
//     return Mathf.Clamp01(lengthOnSection / sectionLength);
// }

// float getRotation(int index, float percentOnSection)
// {
//     float lerp = Mathf.Lerp(
//         angles[index],
//         angles[index + 1],
//         percentOnSection);
//     // Debug.Log($"A: {angles[index]} B: {angles[index + 1]} T: {percentOnSection} lerp: {lerp}");
//     return lerp / (Mathf.PI * 2) * 360;
// }

// Vector3 getPosition(int index, float percentOnSection)
// {
//     if (index % 2 == 1) // on straight section
//     {
//         int wpIndex1 = (index - 1) / 2;
//         int wpIndex2 = (wpIndex1 + 1) % waypoints.Length;
//         return Vector3.Lerp(
//             waypoints[wpIndex1],
//             waypoints[wpIndex2],
//             percentOnSection);
//     }
//     else // on corner (arc section)
//     {
//         int cornerIndex = index / 2;
//         return waypoints[cornerIndex];
//     }
// }

//     // Update is called once per frame
//     void FixedUpdate()
//     {
//         float totalPercentage = (1 + (tr.trackProgress + pathOffset) / 100) % 1;
//         int index = sectionIndex(totalPercentage);
//         float percentage = sectionPercent(index, totalPercentage);
//         float rotation = getRotation(index, percentage) + initialAngle;
//         // Debug.Log($"rotation: {rotation}");
//         Vector3 position = getPosition(index, percentage);
//         transform.localPosition = position;
//         finalRotation.x = rotation;
//         transform.localEulerAngles = finalRotation;
//         // Debug.Log($"path position: {totalPercentage} index:{index}, section length: {sectionLengths[index]} percentage on section:{percentage}, rotation: {rotation}, position: {position}");
//         isTouchingGround = TrackContact.isTouchingGround;
//         contactFront = TrackContact.transform.forward;
//         contactPosition = TrackContact.transform.position;
//     }
// }


using UnityEngine;

public class TrackLinkControl : MonoBehaviour
{
    public float radius = 0.13f;
    private float trackProgress = 0f;
    public float pathOffset = 0f;
    private float initialAngle = 0f;
    public Track tr;
    public TrackContact TrackContact;
    public bool isTouchingGround;
    public Vector3 contactFront;
    public Vector3 contactPosition;
    public Vector3 contactDown;
    private Vector3 finalRotation = Vector3.zero;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get values from the Track class
        trackProgress = tr.trackProgress;
        initialAngle = tr.initialAngle;
        
        // All calculations are now done in the Track class
        // Just verify we have the data
        if (tr.waypoints == null || tr.sectionLengths == null)
        {
            Debug.LogError("Track data not initialized!");
        }
    }
    
    int SectionIndex(float val)
    {
        val %= 1f; // Ensure within [0,1)
        for (int i = 0; i < tr.lengthsFromStart.Length; i++)
        {
            if (val < tr.lengthsFromStart[i]) // Use < instead of <= for first element
            {
                return i;
            }
        }
        return 0; // Shouldn't happen if val is in [0,1)
    }

    float SectionPercent(int index, float trackProgress)
    {
        float sectionLength = tr.sectionLengths[index];
        float sectionStart = (index == 0) ? 0f : tr.lengthsFromStart[index - 1];
        float lengthOnSection = trackProgress - sectionStart;
        return Mathf.Clamp01(lengthOnSection / sectionLength);
    }

    float GetRotation(int index, float percentOnSection)
    {
        float lerp = Mathf.Lerp(
            tr.angles[index],
            tr.angles[index + 1],
            percentOnSection);
        // Debug.Log($"A: {tr.angles[index]} B: {tr.angles[index + 1]} T: {percentOnSection} lerp: {lerp}");
        return lerp / (Mathf.PI * 2) * 360;
    }

    Vector3 GetPosition(int index, float percentOnSection)
    {
        if (index % 2 == 1) // on straight section
        {
            int wpIndex1 = (index - 1) / 2;
            int wpIndex2 = (wpIndex1 + 1) % tr.waypoints.Length;
            return Vector3.Lerp(
                tr.waypoints[wpIndex1],
                tr.waypoints[wpIndex2],
                percentOnSection);
        }
        else // on corner (arc section)
        {
            int cornerIndex = index / 2;
            return tr.waypoints[cornerIndex];
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float totalPercentage = (tr.trackProgress + pathOffset) / 100 % 1;
        if (totalPercentage < 0) totalPercentage += 1;
        int index = SectionIndex(totalPercentage);
        float percentage = SectionPercent(index, totalPercentage);
        float rotation = GetRotation(index, percentage) + tr.initialAngle;
        // Debug.Log($"rotation: {rotation}");
        Vector3 position = GetPosition(index, percentage);
        transform.localPosition = position;
        finalRotation.x = rotation;
        transform.localEulerAngles = finalRotation;
        // Debug.Log($"path position: {totalPercentage} index:{index}, section length: {tr.sectionLengths[index]} percentage on section:{percentage}, rotation: {rotation}, position: {position}");
        isTouchingGround = TrackContact.isTouchingGround;
        contactFront = TrackContact.transform.forward;
        contactPosition = TrackContact.transform.position;
        contactDown = -TrackContact.transform.up;
    }
}