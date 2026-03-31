using UnityEngine;

public class DeathVolume : MonoBehaviour
{
    Damage.Request deadlyDamage;
    void Awake()
    {
        deadlyDamage.damage = 999999999;
        deadlyDamage.type = "death volume";
        deadlyDamage.source = gameObject;
    }
    // void OnCollisionEnter(Collision collision)
    // {
    //     Debug.LogWarning($"{collision.gameObject} has entered death volume");
    //     IDamagable iDamagable = collision.gameObject.GetComponent<IDamagable>();
    //     if (iDamagable != null)
    //     {
    //         iDamagable.Damage(deadlyDamage);
    //     }
    // }
    void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning($"{other.gameObject} has entered death volume");
        IDamagable iDamagable = other.gameObject.GetComponent<IDamagable>();
        if (iDamagable != null)
        {
            iDamagable.Damage(deadlyDamage);
        }
    }
}
