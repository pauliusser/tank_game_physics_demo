using UnityEngine;

public class TankVisualInterestTarget : MonoBehaviour
{
    private GameObject tankBody;
    // Update is called once per frame
    private Rigidbody tankRb;
    void OnEnable()
    {
        PlayerEvents.OnTankSpawn.Subscribe(SetTank);
    }
    void OnDisable()
    {
        PlayerEvents.OnTankSpawn.Unsubscribe(SetTank);
    }
    void SetTank(GameObject t)
    {
        tankBody = t.GetComponent<TankVehicleFSM>().body;
        tankRb = tankBody.GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        if (tankBody == null) return;

        Vector3 localVelocity = tankBody.transform.InverseTransformDirection(tankRb.linearVelocity);
        float forwardSpeed = localVelocity.z;

        if (Mathf.Abs(forwardSpeed) < 0.05f)
            forwardSpeed = 0f;

        // Debug.Log(forwardSpeed);

        Vector3 tankPos = tankBody.transform.position;
        Vector3 tankForward = tankBody.transform.forward.normalized;

        transform.position = tankPos + tankForward * forwardSpeed;
    }

}
