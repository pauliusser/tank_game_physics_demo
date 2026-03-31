using UnityEngine;

public class TrackContact : MonoBehaviour
{
    public bool isTouchingGround = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isTouchingGround = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isTouchingGround = false;
        }
    }
}

