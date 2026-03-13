using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LookAtCamera : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 initialScale;
    private Renderer rend;

    [Header("Distance Scaling")]
    public float referenceDistance = 10f; // distance at which scale = 1
    public float minScale = 0.5f;
    public float maxScale = 2f;

    void Start()
    {
        mainCamera = Camera.main;
        initialScale = transform.localScale;

        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // Force render on top
            Material mat = rend.material;
            mat.renderQueue = 4000; // Overlay queue
            mat.SetInt("_ZWrite", 0); // Don't write to depth
            mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always); // Always render
        }
    }

    void Update()
    {
        if (mainCamera == null) return;

        // 1️⃣ Face the camera
        Vector3 direction = mainCamera.transform.position - transform.position;
        direction.y = 0; // optional: only rotate around Y
        transform.rotation = Quaternion.LookRotation(direction);

        // 2️⃣ Scale by distance
        float distance = Vector3.Distance(transform.position, mainCamera.transform.position);
        float scaleFactor = Mathf.Clamp(distance / referenceDistance, minScale, maxScale);
        transform.localScale = initialScale * scaleFactor;
    }
}