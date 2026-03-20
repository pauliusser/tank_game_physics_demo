using UnityEngine;

public class TankDamageHandler : MonoBehaviour, IDamagable
{
    public TankStats stats;
    public TankVehicleFSM stateMachine;
    public int Health
    {
        get => stats.Health;
        set => stats.Health = value;
    }

    public void Damage(Damage.Request d)
    {
        if (stats.Health <= 0) return;

        if (d.type == "kinetic" || d.type == "explosive")
        {
            if (stats.shield > 0)
            {
                stats.shield -= d.damage;
                if (stats.shield < 0)
                {
                    stats.Health += stats.shield;
                    stats.shield = 0;
                    if (gameObject.tag == "Player")
                    {
                        GameEvents.OnHealthUpdate.Invoke((float)stats.Health / stats.maxHealth);
                        GameEvents.OnShieldUpdate.Invoke(0f);
                    }
                }
                else if (gameObject.tag == "Player") GameEvents.OnShieldUpdate.Invoke((float)stats.shield / stats.maxShield);
            }
            else
            {
                stats.Health -= d.damage;
                if (gameObject.tag == "Player") GameEvents.OnHealthUpdate.Invoke((float)stats.Health / stats.maxHealth);
            }
        } else if (d.type == "death volume")
        {
            stats.Health = 0;
            stats.shield = 0;
            if (gameObject.tag == "Player")
            {
                GameEvents.OnHealthUpdate.Invoke(0f);
                GameEvents.OnShieldUpdate.Invoke(0f);
            }
        }

        if (stats.Health <= 0)
        {
            stateMachine.ChangeState(new DeathState());
            if (gameObject.tag == "Player") GameEvents.OnHealthUpdate.Invoke(0f);
        }
    }
}
