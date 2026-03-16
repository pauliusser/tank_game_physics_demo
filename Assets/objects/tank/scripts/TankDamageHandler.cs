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
                    GameEvents.OnHealthUpdate.Invoke((float)stats.Health / stats.maxHealth);
                }
                GameEvents.OnShieldUpdate.Invoke((float)stats.shield / stats.maxShield);
            }
            else
            {
                stats.Health -= d.damage;
                GameEvents.OnHealthUpdate.Invoke((float)stats.Health / stats.maxHealth);
            }

        }
            

        if (stats.Health <= 0)
        {
            stateMachine.ChangeState(new DeathState());
            GameEvents.OnHealthUpdate.Invoke(0f);
        }
            
    }
}
