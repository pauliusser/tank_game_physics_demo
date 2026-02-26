using UnityEngine;

public class TankVisualInterestTarget : MonoBehaviour
{
    public GameObject tank;
    // Update is called once per frame
    private Rigidbody tankRb;
    void Start()
    {
        tankRb = tank.GetComponent<Rigidbody>();
    }
void FixedUpdate()
{
    Vector3 localVelocity = tank.transform.InverseTransformDirection(tankRb.linearVelocity);
    float forwardSpeed = localVelocity.z;

    if (Mathf.Abs(forwardSpeed) < 0.05f)
        forwardSpeed = 0f;

    // Debug.Log(forwardSpeed);

    Vector3 tankPos = tank.transform.position;
    Vector3 tankForward = tank.transform.forward.normalized;

    transform.position = tankPos + tankForward * forwardSpeed;
}

}
