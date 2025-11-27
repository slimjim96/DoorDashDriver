using UnityEngine;

public class TestVisibleEffects : MonoBehaviour
{
    [Header("Test Effects")]
    [SerializeField] bool enableEffects = true;
    [SerializeField] Color testColor = Color.red;
    
    private DynamicTriangleController triangleController;
    private GameObject testDot;
    private float lastEffectTime;
    
    void Start()
    {
        triangleController = GetComponent<DynamicTriangleController>();
        CreateTestDot();
    }
    
    void Update()
    {
        if (!enableEffects || triangleController == null) return;
        
        // Simple test: create a visible dot every second when moving
        if (triangleController.currentSpeed > 0.1f && Time.time - lastEffectTime > 1f)
        {
            Debug.Log($"Creating test effect at speed: {triangleController.currentSpeed}");
            CreateVisibleDot();
            lastEffectTime = Time.time;
        }
        
        // Move the persistent test dot
        if (testDot != null && triangleController.currentSpeed > 0.1f)
        {
            Vector3 offset = -transform.up * 0.5f;
            testDot.transform.position = transform.position + offset;
            
            // Change color based on speed
            SpriteRenderer sr = testDot.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float intensity = triangleController.currentSpeed / triangleController.MaxSpeed;
                sr.color = Color.Lerp(Color.white, testColor, intensity);
            }
        }
    }
    
    void CreateTestDot()
    {
        testDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        testDot.transform.SetParent(transform);
        testDot.transform.localScale = Vector3.one * 0.2f;
        testDot.name = "TestDot";
        
        // Remove collider
        Collider col = testDot.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);
        
        // Set color
        Renderer renderer = testDot.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = testColor;
        }
    }
    
    void CreateVisibleDot()
    {
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.transform.position = transform.position + Random.insideUnitSphere * 0.5f;
        dot.transform.localScale = Vector3.one * 0.1f;
        dot.name = "TemporaryDot";
        
        // Remove collider
        Collider col = dot.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);
        
        // Set random color
        Renderer renderer = dot.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Random.ColorHSV();
        }
        
        // Destroy after 2 seconds
        Destroy(dot, 2f);
    }
}