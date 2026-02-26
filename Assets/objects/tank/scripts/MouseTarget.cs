using UnityEngine;

public class MouseTarget : MonoBehaviour
{
    [Header("Optional Layer Filtering")]
    public bool useLayerMask = false;       // enable/disable layer filtering
    public LayerMask raycastLayer;      // layer to hit if filtering is enabled
    public bool normalDebug = false;
    void Update()
    {
        if (Input.mousePosition!= null)
        {
            Vector3 mousePos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;
            bool hitSomething;
            if (useLayerMask)
            {
                hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayer);
            }
            else
            {
                hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity);
            }
            if (hitSomething)
            {
                transform.position = hit.point;
                if (normalDebug) Debug.DrawRay(hit.point, hit.normal, Color.green);
            }
        }
    }
}
