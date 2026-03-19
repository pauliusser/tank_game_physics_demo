using UnityEngine;

public class TankInstanceSetUp : MonoBehaviour
{
    public GameObject tankInstance;
    public LayerMask tankLayerMask;
    public string tankTag;
    public int maxHealth = 100;
    public float firePowerMultiplier = 1f;
    public Material tankMaterial;
    private TankRefs refs;
    private GameObject[] meshColliders;
    private GameObject[] coloredPars;
    void Start()
    {
        SetUpTank();
    }
    public void SetUpTank()
    {
        if (tankInstance == null) return;
        
        refs = tankInstance.GetComponent<TankRefs>();
        
        if (refs == null)
        {
            Debug.LogError($"TankInstanceSetUp: TankRefs component not found on {tankInstance.name}!");
            return;
        }
        tankInstance.tag = tankTag;
        tankInstance.GetComponent<TankStats>().maxHealth = maxHealth;
        refs = tankInstance.GetComponent<TankRefs>();
        refs.turret.GetComponent<TankTurret>().damageMultiplier = firePowerMultiplier;
        tankInstance.GetComponent<TankDeathHandler>().tankTag = tankTag;
        meshColliders = refs.meshColliders;
        coloredPars = refs.coloredPars;
        SetLayer();
        SetMaterial();
        if (!string.IsNullOrEmpty(tankTag) && refs.body != null)
        {
            refs.body.tag = tankTag;
        }
    }
    private void SetLayer()
    {
        if (meshColliders == null || meshColliders.Length == 0)
        {
            Debug.LogWarning("TankInstanceSetUp: No mesh colliders found to set layer!");
            return;
        }
        
        // Get the first layer from the mask (simpler approach)
        int layerIndex = (int)Mathf.Log(tankLayerMask.value, 2);
        
        // Validate that it's a valid layer index
        if (layerIndex < 0 || layerIndex >= 32)
        {
            Debug.LogError($"TankInstanceSetUp: Invalid layer index {layerIndex} from mask {tankLayerMask.value}");
            return;
        }
        
        foreach (GameObject colliderObj in meshColliders)
        {
            if (colliderObj != null)
            {
                colliderObj.layer = layerIndex;
            }
        }
        
        Debug.Log($"Set {meshColliders.Length} mesh colliders to layer: {LayerMask.LayerToName(layerIndex)}");
    }
    private void SetMaterial()
    {
        if (coloredPars == null || coloredPars.Length == 0)
        {
            Debug.LogWarning("TankInstanceSetUp: No colored parts found to set material!");
            return;
        }
        
        if (tankMaterial == null)
        {
            Debug.LogError("TankInstanceSetUp: tankMaterial is not assigned!");
            return;
        }
        
        int partsSet = 0;
        
        foreach (GameObject part in coloredPars)
        {
            if (part != null)
            {
                Renderer renderer = part.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = tankMaterial;
                    partsSet++;
                }
                else
                {
                    Debug.LogWarning($"TankInstanceSetUp: {part.name} has no Renderer component!");
                }
            }
        }
        
        Debug.Log($"Set material on {partsSet} of {coloredPars.Length} colored parts");
    }
}
