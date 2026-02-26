using UnityEngine;

public class TwoInterestsCenter : MonoBehaviour
{
    public Transform interestA;
    public Transform interestB;
    public float weight = 0.5f;
    public bool lagBehind = true;
    public float catchUpSpeed = 0.01f;
    public float deadZoneSize = 0.1f;
    private Vector3 prevPos;
    void Start()
    {
        prevPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 newPos = Vector3.Lerp(interestA.position, interestB.position, weight);
        // Vector3 newPos = (interestA.position + interestB.position) / 2;
        Vector3 translation =  newPos - prevPos;
        if(translation.magnitude > deadZoneSize && lagBehind)
        {
            transform.position += translation * catchUpSpeed;
        }
        prevPos = transform.position;
    }
}
