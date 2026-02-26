using System;
using UnityEngine;

public class TurntTableCameraControlsManager : MonoBehaviour
{
    public static TurntTableCameraControlsManager Instance;
    public event Action<Vector3> OnMouseTurntableInput;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SendCameraInput(Vector3 camPos) 
    {
        OnMouseTurntableInput?.Invoke(camPos);
        // DebugSubscribers();
    }
    public void SubCameraHandler(Action<Vector3> observer) => OnMouseTurntableInput += observer;
    public void UnsubCameraHandler(Action<Vector3> observer) => OnMouseTurntableInput -= observer;
    public void DebugSubscribers()
    {
        if (OnMouseTurntableInput != null)
        {
            Debug.Log("There are subscribers!");
            Debug.Log($"Subscriber count: {OnMouseTurntableInput.GetInvocationList().Length}");
        }
        else
        {
            Debug.Log("No subscribers");
        }
    }
}
