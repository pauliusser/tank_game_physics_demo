using UnityEngine;

public class TargetedCamera : MonoBehaviour
{
    public Transform cameraTarget;
    void Update()
    {
        if (cameraTarget != null)
        {
            transform.LookAt(cameraTarget);
        }
    }
}
