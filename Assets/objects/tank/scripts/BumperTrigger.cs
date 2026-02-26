using UnityEngine;

public class BumperTrigger : MonoBehaviour
{
    public bool isTouchingWall = false;
    public LayerMask wallLayerMask; // Assign in Inspector: which layers count as "walls"
    
    void OnTriggerEnter(Collider other)
    {
        // Check if the collider is on a wall layer
        if (((1 << other.gameObject.layer) & wallLayerMask) != 0)
        {
            isTouchingWall = true;
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        // Keep isTouchingWall true while touching
        if (((1 << other.gameObject.layer) & wallLayerMask) != 0)
        {
            isTouchingWall = true;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // Check if the collider is on a wall layer
        if (((1 << other.gameObject.layer) & wallLayerMask) != 0)
        {
            isTouchingWall = false;
        }
    }
}
