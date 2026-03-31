using UnityEngine;

public class EmptyGizmoSphere : MonoBehaviour
{
    public Color gizmoColor = Color.white;
    public float size = 0.1f;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, size);
    }
}
