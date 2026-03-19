using UnityEngine;

public class TestBox : MonoBehaviour, IDamagable
{
    public int score = 10;
    public int maxHealth = 100;
    public int Health { get; set; }
    private Renderer rend;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        rend = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
        Health = maxHealth;
        UpdateTint();
    }
    public void Damage(Damage.Request d)
    {
        Debug.Log($"current health {Health}");
        if (d.type == "kinetic") Health -= d.damage;
        if (d.type == "explosive") Health -= d.damage;
        if (d.type == "death volume") Health = 0;
        // Debug.Log($"damage: {d.damage} health: {Health} type: {d.type} source: {d.source}");
        if (Health <= 0) 
        {
            Destroy(gameObject);
            if(d.source.tag == "Player")
            {
                PlayerEvents.OnPlayerScored.Invoke(score);
            }
        }
        UpdateTint();
    }
    void UpdateTint()
    {
        rend.GetPropertyBlock(propBlock);
        float healthPercent = (float) Health / maxHealth;
        // Debug.Log($"healthPercent: {healthPercent}");
        float hue = Mathf.Lerp(0f, 90f, healthPercent);
        // Debug.Log($"hue: {hue}");
        Color tintColor = Color.HSVToRGB(hue / 360f, 1f, 1f);
        propBlock.SetColor("_BaseColor", tintColor);
        rend.SetPropertyBlock(propBlock);
    }
}
