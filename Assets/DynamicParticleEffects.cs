using UnityEngine;

public class DynamicParticleEffects : MonoBehaviour
{
    [Header("Speed Particles")]
    [SerializeField] GameObject particlePrefab;
    [SerializeField] int maxParticles = 20;
    [SerializeField] float particleLifetime = 1f;
    [SerializeField] float speedThreshold = 2f;
    [SerializeField] Color[] particleColors = { Color.cyan, Color.yellow, Color.red };

    [Header("Boost Effects")]
    [SerializeField] float boostThreshold = 5f;
    [SerializeField] int boostParticleCount = 5;

    private ParticleSystem speedParticles;
    private ParticleSystem boostParticles;
    private DynamicTriangleController triangleController;

    void Start()
    {
        triangleController = GetComponent<DynamicTriangleController>();
        SetupParticleSystems();
    }

    void Update()
    {
        UpdateSpeedParticles();
        UpdateBoostParticles();
    }

    void SetupParticleSystems()
    {
        // Speed particles (continuous trail)
        GameObject speedParticleGO = new GameObject("SpeedParticles");
        speedParticleGO.transform.SetParent(transform);
        speedParticleGO.transform.localPosition = Vector3.zero;
        
        speedParticles = speedParticleGO.AddComponent<ParticleSystem>();
        var speedMain = speedParticles.main;
        speedMain.startLifetime = particleLifetime;
        speedMain.startSpeed = 2f;
        speedMain.startSize = 0.2f; // Make particles larger and more visible
        speedMain.startColor = particleColors[0];
        speedMain.maxParticles = maxParticles;
        speedMain.simulationSpace = ParticleSystemSimulationSpace.World;

        var speedEmission = speedParticles.emission;
        speedEmission.rateOverTime = 10f; // Add continuous emission for visibility

        // Enable the renderer and set material
        var speedRenderer = speedParticles.GetComponent<ParticleSystemRenderer>();
        speedRenderer.material = new Material(Shader.Find("Sprites/Default"));
        speedRenderer.material.color = Color.white;

        var speedVelocity = speedParticles.velocityOverLifetime;
        speedVelocity.enabled = true;
        speedVelocity.space = ParticleSystemSimulationSpace.Local;

        var speedColorOverLifetime = speedParticles.colorOverLifetime;
        speedColorOverLifetime.enabled = true;
        Gradient speedGradient = new Gradient();
        speedGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.blue, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        speedColorOverLifetime.color = speedGradient;

        // Boost particles (burst effect)
        GameObject boostParticleGO = new GameObject("BoostParticles");
        boostParticleGO.transform.SetParent(transform);
        boostParticleGO.transform.localPosition = Vector3.zero;
        
        boostParticles = boostParticleGO.AddComponent<ParticleSystem>();
        var boostMain = boostParticles.main;
        boostMain.startLifetime = 0.5f;
        boostMain.startSpeed = 5f;
        boostMain.startSize = 0.2f;
        boostMain.startColor = Color.red;
        boostMain.maxParticles = boostParticleCount * 2;
        boostMain.simulationSpace = ParticleSystemSimulationSpace.World;

        var boostEmission = boostParticles.emission;
        boostEmission.rateOverTime = 0f;

        var boostShape = boostParticles.shape;
        boostShape.enabled = true;
        boostShape.shapeType = ParticleSystemShapeType.Circle;
        boostShape.radius = 0.5f;
    }

    void UpdateSpeedParticles()
    {
        if (triangleController == null) return;

        float currentSpeed = triangleController.currentSpeed;
        
        // Always emit some particles when moving, make threshold lower
        if (currentSpeed > 0.1f)
        {
            // Emit speed particles based on current speed
            int particlesToEmit = Mathf.RoundToInt(currentSpeed * 2f); // More particles
            particlesToEmit = Mathf.Min(particlesToEmit, 5);

            for (int i = 0; i < particlesToEmit; i++)
            {
                EmitSpeedParticle();
            }
        }
    }

    void UpdateBoostParticles()
    {
        if (triangleController == null) return;

        float currentSpeed = triangleController.currentSpeed;
        
        // Check for boost threshold crossing
        if (currentSpeed > boostThreshold && !IsInvoking("EmitBoostBurst"))
        {
            EmitBoostBurst();
        }
    }

    void EmitSpeedParticle()
    {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        
        // Position behind the triangle (2D appropriate scale)
        Vector3 emitPosition = transform.position - (Vector3)transform.up * 0.3f;
        Vector2 randomOffset = Random.insideUnitCircle * 0.1f;
        emitPosition += new Vector3(randomOffset.x, randomOffset.y, 0);
        emitParams.position = emitPosition;
        
        // Velocity opposite to movement direction (2D appropriate scale)
        Vector2 velocity2D = -triangleController.velocity.normalized * Random.Range(1f, 3f);
        velocity2D += Random.insideUnitCircle * 0.5f;
        emitParams.velocity = velocity2D;
        
        // Random color from palette
        Color particleColor = particleColors[Random.Range(0, particleColors.Length)];
        emitParams.startColor = particleColor;
        
        // Size based on speed (2D appropriate scale)
        float size = 0.05f + (triangleController.currentSpeed / triangleController.MaxSpeed) * 0.15f;
        emitParams.startSize = size;
        
        speedParticles.Emit(emitParams, 1);
    }

    void EmitBoostBurst()
    {
        // Create a burst effect
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.position = transform.position;
        
        for (int i = 0; i < boostParticleCount; i++)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            emitParams.velocity = randomDirection * Random.Range(3f, 7f);
            emitParams.startColor = Color.Lerp(Color.yellow, Color.red, Random.value);
            emitParams.startSize = Random.Range(0.1f, 0.3f);
            
            boostParticles.Emit(emitParams, 1);
        }
        
        // Prevent multiple bursts too quickly
        Invoke("ResetBoostCooldown", 0.5f);
    }

    void ResetBoostCooldown()
    {
        // This method exists just to create a cooldown period
    }

    // Public method to trigger custom particle effects
    public void TriggerSpeedBurst(int count = 10)
    {
        for (int i = 0; i < count; i++)
        {
            EmitSpeedParticle();
        }
    }
}