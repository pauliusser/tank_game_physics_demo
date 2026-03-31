using UnityEngine;

public class tankWheel : MonoBehaviour
{
    public float teethCount = 12f;
    public Track track;
    public float offsetAngle = 0f;
    private float trackLength;
    private int trackLinkCount;
    private float trackProgress;
    private Vector3 wheelAngle;
    private float rotationRatio;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        wheelAngle = transform.localEulerAngles;
        trackLength = track.totalLen;
        trackLinkCount = track.linksCount;
        rotationRatio = trackLinkCount / teethCount;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        trackProgress = track.trackProgress;
        wheelAngle.z = trackProgress / 100 * 360 * rotationRatio + offsetAngle;
        transform.localEulerAngles = wheelAngle;
    }
}
