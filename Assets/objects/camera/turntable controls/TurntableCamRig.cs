using UnityEngine;

public class TurntableCamRig : MonoBehaviour
{
    private Transform pivot;
    Vector3 rot = Vector3.zero;
    Vector3 pos = Vector3.zero; 
    public Vector3 camRigStartPos = Vector3.zero;
    public Vector3 prevInput = Vector3.zero;
    public Vector3 inputDelta = Vector3.zero;
    public float zFarLim = -30f;
	public float zNearLim = -1.3f;
    public float yLoLim = 0f;
    public float yHiLim = 90f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        pivot = transform.parent;
        rot = pivot.localEulerAngles;
        pos = transform.localPosition;
        camRigStartPos.x = pivot.localEulerAngles.y;
        camRigStartPos.y = -pivot.localEulerAngles.x;
        camRigStartPos.z = transform.localPosition.z;
        // Debug.Log("turntable camera rig is awake");
    }

    void Start()
    {
        // Subscribe to the event
        if (TurntTableCameraControlsManager.Instance != null)
        {
            TurntTableCameraControlsManager.Instance.SubCameraHandler(positionUpdateHandler);
        }
        else
        {
            Debug.Log("TurntTableCameraControlsManager instance not found");
        }
        positionUpdateHandler(Vector3.zero);
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        if (TurntTableCameraControlsManager.Instance != null)
            TurntTableCameraControlsManager.Instance.UnsubCameraHandler(positionUpdateHandler);
    }

    void positionUpdateHandler(Vector3 input)
    {
        inputDelta = input - prevInput;
        
        if (rot.x - inputDelta.y > yHiLim) rot.x = yHiLim;
        else if (rot.x - inputDelta.y < yLoLim) rot.x = yLoLim;
        else rot.x -= inputDelta.y;
        
        rot.y += inputDelta.x;
        rot.y = (rot.y + 360f) % 360f;

        if (pos.z + inputDelta.z < zFarLim ) pos.z = zFarLim;
        else if (pos.z + inputDelta.z > zNearLim) pos.z = zNearLim;
        else pos.z += inputDelta.z;



        pivot.localEulerAngles = rot;
        transform.localPosition = pos;
        prevInput = input;
    }
}
