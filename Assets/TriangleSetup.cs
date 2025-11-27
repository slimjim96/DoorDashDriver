using UnityEngine;

public class TriangleSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] bool autoSetupOnStart = true;
    [SerializeField] bool addTrailRenderer = true;
    [SerializeField] bool addSimple2DEffects = true;

    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupTriangle();
        }
    }

    [ContextMenu("Setup Triangle")]
    public void SetupTriangle()
    {
        // Ensure we have the main controller
        DynamicTriangleController controller = GetComponent<DynamicTriangleController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<DynamicTriangleController>();
        }

        // Add trail renderer if requested
        if (addTrailRenderer)
        {
            DynamicTrailRenderer trailRenderer = GetComponent<DynamicTrailRenderer>();
            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<DynamicTrailRenderer>();
            }
        }

        // Add simple 2D effects if requested
        if (addSimple2DEffects)
        {
            TestVisibleEffects testEffects = GetComponent<TestVisibleEffects>();
            if (testEffects == null)
            {
                testEffects = gameObject.AddComponent<TestVisibleEffects>();
            }
            
            Simple2DEffects simple2DEffects = GetComponent<Simple2DEffects>();
            if (simple2DEffects == null)
            {
                simple2DEffects = gameObject.AddComponent<Simple2DEffects>();
            }
        }

        // Set initial position and setup
        transform.position = Vector3.zero;
        
        Debug.Log("Dynamic Triangle setup complete! Use arrow keys to move.");
    }

    [ContextMenu("Debug Components")]
    public void DebugComponents()
    {
        Debug.Log("=== Triangle Component Debug ===");
        
        DynamicTriangleController controller = GetComponent<DynamicTriangleController>();
        Debug.Log($"DynamicTriangleController: {(controller != null ? "✓ Found" : "✗ Missing")}");
        
        DynamicTrailRenderer trail = GetComponent<DynamicTrailRenderer>();
        Debug.Log($"DynamicTrailRenderer: {(trail != null ? "✓ Found" : "✗ Missing")}");
        
        Simple2DEffects simple2D = GetComponent<Simple2DEffects>();
        Debug.Log($"Simple2DEffects: {(simple2D != null ? "✓ Found" : "✗ Missing")}");
        
        if (controller != null)
        {
            Debug.Log($"Current Speed: {controller.currentSpeed}");
            Debug.Log($"Velocity: {controller.velocity}");
        }
        
        LineRenderer[] lineRenderers = GetComponentsInChildren<LineRenderer>();
        Debug.Log($"LineRenderers found: {lineRenderers.Length}");
    }

    [ContextMenu("Create Test Triangle")]
    public void CreateTestTriangle()
    {
        GameObject triangleObject = new GameObject("DynamicTriangle");
        triangleObject.transform.position = Vector3.zero;
        
        // Add all components
        triangleObject.AddComponent<DynamicTriangleController>();
        triangleObject.AddComponent<DynamicTrailRenderer>();
        triangleObject.AddComponent<Simple2DEffects>();
        triangleObject.AddComponent<TriangleSetup>();
        
        Debug.Log("Test triangle created!");
    }
}