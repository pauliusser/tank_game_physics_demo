// using UnityEngine;

// public class MouseTarget : MonoBehaviour
// {
//     [Header("Optional Layer Filtering")]
//     public bool useLayerMask = false;       // enable/disable layer filtering
//     public LayerMask raycastLayer;      // layer to hit if filtering is enabled
//     public bool normalDebug = false;
//     public LayerMask enemyLayerMask;
//     void Update()
//     {
//         if (Input.mousePosition!= null)
//         {
//             Vector3 mousePos = Input.mousePosition;
//             Ray ray = Camera.main.ScreenPointToRay(mousePos);
//             RaycastHit hit;
//             bool hitSomething;
//             if (useLayerMask)
//             {
//                 hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayer);
//             }
//             else
//             {
//                 hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity);
//             }
//             if (hitSomething)
//             {
//                 if (((1 << hit.collider.gameObject.layer) & enemyLayerMask) != 0)
//                 {
//                     Vector3 camPos = Camera.main.transform.position;
//                     Vector3 coliderPos = hit.collider.transform.position;
//                     Vector3 direction = (hit.point - camPos).normalized;
//                     float distance = (camPos - coliderPos).magnitude;
//                     transform.position = camPos + direction * distance;
//                 }
//                 else transform.position = hit.point;
//                 if (normalDebug) Debug.DrawRay(hit.point, hit.normal, Color.green);
//             }
//         }
//     }
// }
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseTarget : MonoBehaviour
{
    [Header("Optional Layer Filtering")]
    public bool useLayerMask = false;
    public LayerMask raycastLayer;
    public bool normalDebug = false;
    public LayerMask enemyLayerMask;

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        bool hitSomething;

        if (useLayerMask)
            hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayer);
        else
            hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity);

        if (hitSomething)
        {
            if (((1 << hit.collider.gameObject.layer) & enemyLayerMask) != 0)
            {
                Vector3 camPos = Camera.main.transform.position;
                Vector3 colliderPos = hit.collider.transform.position;
                Vector3 direction = (hit.point - camPos).normalized;
                float distance = (camPos - colliderPos).magnitude;
                transform.position = camPos + direction * distance;
            }
            else
            {
                transform.position = hit.point;
            }

            if (normalDebug) Debug.DrawRay(hit.point, hit.normal, Color.green);
        }
    }
}