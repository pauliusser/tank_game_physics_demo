using UnityEngine;

public class FollowTarget : MonoBehaviour
{
public Transform folowTarget;
public float followCoeff = 0.5f;
public float catchUpDistance = 2f;
private Vector3 prevPos;

    void Start()
    {
        prevPos = folowTarget.position;
    }
    void Update()
    {
        Vector3 drift = folowTarget.position - prevPos;
        float driftDist = drift.magnitude;
        float catchUpMotivation = 1 / ((catchUpDistance - driftDist) / catchUpDistance);
        // Debug.Log(catchUpMotivation);
        Vector3 motionVector = drift.normalized * driftDist * followCoeff;

        transform.position += motionVector;
        prevPos = folowTarget.position;
    }
}
