using UnityEngine;

public class SceneSettings : MonoBehaviour
{
    public float gravityMultiplyer = 10f;
    void Start()
    {
        Physics.gravity = new Vector3(0, -9.81f * gravityMultiplyer, 0); // -98.1
    }
}
