using UnityEngine;
using UnityEngine.InputSystem;

public class MouseCameraInput : MonoBehaviour
{
	public Vector3 posOut = Vector3.zero;
	private Vector3 inertionDelta = Vector3.zero;
	public float inertion = 0.8f;
	private int longEdge;
	public float sensitivity = 150f;

	public bool isZoomEnabled = false;
	public bool isTiltEnabled = false;


	void Awake()
	{        
		longEdge = Mathf.Max(Screen.width,Screen.height);
	}

	void updateCamera(Vector3 newCamPos)
	{
		if (TurntTableCameraControlsManager.Instance != null)
		{
			TurntTableCameraControlsManager.Instance.SendCameraInput(newCamPos);
		}
	}

	void Update()
	{
		if (Mouse.current == null) return;

		// Right mouse drag
		if (Mouse.current.rightButton.isPressed){
			Vector2 delta = Mouse.current.delta.ReadValue();
			posOut.x += delta.x / longEdge * sensitivity;
			posOut.y += isTiltEnabled ? delta.y / longEdge * sensitivity : 0f;
			updateCamera(posOut);
			inertionDelta.x = delta.x;
			inertionDelta.y = delta.y;
		} else if (
		Mathf.Abs(inertionDelta.x) + 
		Mathf.Abs(inertionDelta.y) + 
		Mathf.Abs(inertionDelta.z) > 0.01f)	{
			inertionDelta *= inertion;
			posOut.x += inertionDelta.x;
			posOut.y += isTiltEnabled ? inertionDelta.y : 0f;
			updateCamera(posOut);
		}            
		

		// Mouse wheel zoom
		float scroll = Mouse.current.scroll.ReadValue().y;
		if (scroll != 0 && isZoomEnabled)
		{
			posOut.z += scroll * 0.1f;
			updateCamera(posOut);
		}
	}
}