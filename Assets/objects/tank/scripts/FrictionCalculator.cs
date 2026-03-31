using UnityEngine;

public class FrictionCalculator : MonoBehaviour
{
    // Default friction coefficients
    public const float DEFAULT_STATIC_FRICTION = 0.6f;
    public const float DEFAULT_DYNAMIC_FRICTION = 0.4f;
    public const float VELOCITY_THRESHOLD = 0.01f; // m/s
    
    /// <summary>
    /// Calculates the friction force for an object on a surface
    /// </summary>
    public Vector3 CalculateFrictionForce(
        float mass,
        Vector3 gravity,
        Vector3 velocity,
        Vector3 surfaceNormal,
        float staticFriction = -1f,
        float dynamicFriction = -1f)
    {
        // Use defaults if not provided
        float μ_static = staticFriction >= 0 ? staticFriction : DEFAULT_STATIC_FRICTION;
        float μ_dynamic = dynamicFriction >= 0 ? dynamicFriction : DEFAULT_DYNAMIC_FRICTION;
        
        // 1. Calculate normal force
        float normalForce = CalculateNormalForce(mass, gravity, surfaceNormal);
        
        // 2. Extract horizontal components
        Vector3 horizontalVelocity = GetHorizontalComponent(velocity, surfaceNormal);
        Vector3 gravityHorizontal = GetHorizontalComponent(gravity, surfaceNormal);
        
        // 3. Determine friction type and calculate force
        if (horizontalVelocity.magnitude < VELOCITY_THRESHOLD)
        {
            // Static friction case
            return CalculateStaticFrictionForce(mass, gravityHorizontal, normalForce, μ_static);
        }
        else
        {
            // Dynamic friction case
            return CalculateDynamicFrictionForce(horizontalVelocity, normalForce, μ_dynamic);
        }
    }
    
    /// <summary>
    /// Calculates if static friction would be overcome by an applied force
    /// </summary>
    public bool WouldOvercomeStaticFriction(
        float mass,
        Vector3 gravity,
        Vector3 appliedForce,
        Vector3 surfaceNormal,
        float staticFriction = -1f)
    {
        float μ_static = staticFriction >= 0 ? staticFriction : DEFAULT_STATIC_FRICTION;
        float normalForce = CalculateNormalForce(mass, gravity, surfaceNormal);
        
        Vector3 horizontalForce = GetHorizontalComponent(appliedForce, surfaceNormal);
        float maxStaticFriction = μ_static * normalForce;
        
        return horizontalForce.magnitude > maxStaticFriction;
    }
    
    /// <summary>
    /// Calculates maximum static friction force before movement starts
    /// </summary>
    public float GetMaxStaticFrictionForce(
        float mass,
        Vector3 gravity,
        Vector3 surfaceNormal,
        float staticFriction = -1f)
    {
        float μ_static = staticFriction >= 0 ? staticFriction : DEFAULT_STATIC_FRICTION;
        float normalForce = CalculateNormalForce(mass, gravity, surfaceNormal);
        
        return μ_static * normalForce;
    }
    
    // ========== PRIVATE HELPER METHODS ==========
    
    private float CalculateNormalForce(float mass, Vector3 gravity, Vector3 surfaceNormal)
    {
        if (gravity.magnitude < 0.001f) return mass * 9.81f;
        
        Vector3 gravityDirection = gravity.normalized;
        Vector3 surfaceUp = surfaceNormal.normalized;
        
        float cosAngle = Vector3.Dot(gravityDirection, surfaceUp);
        float normalForceMagnitude = mass * gravity.magnitude * Mathf.Abs(cosAngle);
        
        return Mathf.Max(normalForceMagnitude, 0.01f);
    }
    
    private Vector3 GetHorizontalComponent(Vector3 vector, Vector3 surfaceNormal)
    {
        Vector3 surfaceUp = surfaceNormal.normalized;
        Vector3 verticalComponent = Vector3.Project(vector, surfaceUp);
        
        return vector - verticalComponent;
    }
    
    private Vector3 CalculateStaticFrictionForce(
        float mass, 
        Vector3 gravityHorizontal, 
        float normalForce, 
        float μ_static)
    {
        Vector3 frictionForce = -mass * gravityHorizontal;
        float maxStaticFriction = μ_static * normalForce;
        
        if (frictionForce.magnitude > maxStaticFriction)
        {
            frictionForce = frictionForce.normalized * maxStaticFriction;
        }
        
        return frictionForce;
    }
    
    private Vector3 CalculateDynamicFrictionForce(
        Vector3 horizontalVelocity, 
        float normalForce, 
        float μ_dynamic)
    {
        if (horizontalVelocity.magnitude < 0.001f) return Vector3.zero;
        
        Vector3 frictionDirection = -horizontalVelocity.normalized;
        float frictionMagnitude = μ_dynamic * normalForce;
        
        float velocityFactor = Mathf.Clamp01(horizontalVelocity.magnitude / 0.5f);
        frictionMagnitude *= velocityFactor;
        
        return frictionDirection * frictionMagnitude;
    }
    
    /// <summary>
    /// Calculates stopping distance based on current velocity and friction
    /// </summary>
    public float CalculateStoppingDistance(
        float mass,
        Vector3 velocity,
        Vector3 gravity,
        Vector3 surfaceNormal,
        float dynamicFriction = -1f)
    {
        float μ_dynamic = dynamicFriction >= 0 ? dynamicFriction : DEFAULT_DYNAMIC_FRICTION;
        float normalForce = CalculateNormalForce(mass, gravity, surfaceNormal);
        
        Vector3 horizontalVelocity = GetHorizontalComponent(velocity, surfaceNormal);
        float speed = horizontalVelocity.magnitude;
        
        if (speed < VELOCITY_THRESHOLD) return 0f;
        
        float frictionForce = μ_dynamic * normalForce;
        float deceleration = frictionForce / mass;
        float stoppingDistance = (speed * speed) / (2 * deceleration);
        
        return Mathf.Max(stoppingDistance, 0f);
    }
}