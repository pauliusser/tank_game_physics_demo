using UnityEngine;

public class GroundPenetrationDetector : MonoBehaviour
{
    public LayerMask groundLayers;
    public bool IsPenetrating { get; private set; }

    void OnTriggerStay(Collider other)
    {
        if ((1 << other.gameObject.layer & groundLayers) != 0)
            IsPenetrating = true;
    }

    void OnTriggerExit(Collider other)
    {
        if ((1 << other.gameObject.layer & groundLayers) != 0)
            IsPenetrating = false;
    }
}