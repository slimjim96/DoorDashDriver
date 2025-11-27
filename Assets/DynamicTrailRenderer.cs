using System.Collections.Generic;
using UnityEngine;

public class DynamicTrailRenderer : MonoBehaviour
{
    [Header("2D Trail Settings")]
    [SerializeField] float trailWidth = 0.2f;
    [SerializeField] int maxTrailPoints = 30;
    [SerializeField] float pointDistance = 0.05f;
    [SerializeField] float trailLifetime = 1.5f;
    [SerializeField] Color startColor = Color.cyan;
    [SerializeField] Color endColor = new Color(0, 1, 1, 0);
    [SerializeField] bool use2DRendering = true;

    private List<TrailPoint> trailPoints = new List<TrailPoint>();
    private LineRenderer lineRenderer;
    private Vector3 lastPosition;
    private DynamicTriangleController triangleController;

    private struct TrailPoint
    {
        public Vector3 position;
        public float timestamp;
        public float speed;
    }

    void Start()
    {
        triangleController = GetComponent<DynamicTriangleController>();
        SetupTrailRenderer();
        lastPosition = transform.position;
    }

    void Update()
    {
        AddTrailPoint();
        UpdateTrail();
        RenderTrail();
    }

    void SetupTrailRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Setup for 2D side-facing game
        lineRenderer.material = CreateSimple2DMaterial();
        lineRenderer.startWidth = trailWidth;
        lineRenderer.endWidth = trailWidth * 0.1f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingOrder = -1; // Behind the triangle
        lineRenderer.positionCount = 0;
        
        // 2D specific settings
        lineRenderer.alignment = LineAlignment.TransformZ; // Align to camera facing
        lineRenderer.textureMode = LineTextureMode.Stretch;
        
        // Ensure visibility in 2D
        lineRenderer.enabled = true;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }

    Material CreateSimple2DMaterial()
    {
        // Create a simple unlit material for 2D
        Material mat = new Material(Shader.Find("Unlit/Color"));
        if (mat.shader.name == "Hidden/InternalErrorShader")
        {
            // Fallback to Sprites/Default if Unlit/Color not found
            mat = new Material(Shader.Find("Sprites/Default"));
        }
        
        mat.color = startColor;
        
        // 2D transparency settings
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000; // Transparent queue
        
        return mat;
    }

    void AddTrailPoint()
    {
        Vector3 currentPosition = transform.position;
        
        // Only add points when moving to avoid cluttering
        float distance = Vector3.Distance(currentPosition, lastPosition);
        if (distance > pointDistance)
        {
            float currentSpeed = triangleController != null ? triangleController.currentSpeed : 0f;
            
            // Only add trail points when actually moving
            if (currentSpeed > 0.1f)
            {
                TrailPoint newPoint = new TrailPoint
                {
                    position = currentPosition,
                    timestamp = Time.time,
                    speed = currentSpeed
                };

                trailPoints.Add(newPoint);
                lastPosition = currentPosition;
                
                // Debug for visibility
                if (trailPoints.Count <= 3)
                {
                    Debug.Log($"Trail point added: {trailPoints.Count} at {currentPosition}");
                }
            }
        }
    }

    void UpdateTrail()
    {
        // Remove old points
        for (int i = trailPoints.Count - 1; i >= 0; i--)
        {
            if (Time.time - trailPoints[i].timestamp > trailLifetime)
            {
                trailPoints.RemoveAt(i);
            }
        }

        // Limit trail length
        while (trailPoints.Count > maxTrailPoints)
        {
            trailPoints.RemoveAt(0);
        }
    }

    void RenderTrail()
    {
        if (trailPoints.Count < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        // Set all positions at once
        lineRenderer.positionCount = trailPoints.Count;
        Vector3[] positions = new Vector3[trailPoints.Count];
        
        for (int i = 0; i < trailPoints.Count; i++)
        {
            positions[i] = trailPoints[i].position;
            
            // For 2D, ensure Z position is consistent (flat)
            if (use2DRendering)
            {
                positions[i].z = transform.position.z;
            }
        }
        
        lineRenderer.SetPositions(positions);

        // Simple gradient from start to end
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];

        // Start color (newest/brightest)
        colorKeys[0].color = startColor;
        colorKeys[0].time = 0f;
        
        // End color (oldest/transparent)
        colorKeys[1].color = endColor;
        colorKeys[1].time = 1f;

        // Alpha fade
        alphaKeys[0].alpha = startColor.a;
        alphaKeys[0].time = 0f;
        alphaKeys[1].alpha = 0f;
        alphaKeys[1].time = 1f;

        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;

        // Width curve - taper from full to thin
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 1f);    // Full width at start
        widthCurve.AddKey(1f, 0.1f);  // Thin at end
        lineRenderer.widthCurve = widthCurve;
    }

    // Debug visualization in Scene view
    void OnDrawGizmosSelected()
    {
        if (trailPoints == null || trailPoints.Count < 2) return;
        
        Gizmos.color = Color.cyan;
        for (int i = 0; i < trailPoints.Count - 1; i++)
        {
            float fade = (float)i / trailPoints.Count;
            Gizmos.color = Color.Lerp(Color.clear, Color.cyan, fade);
            Gizmos.DrawLine(trailPoints[i].position, trailPoints[i + 1].position);
        }
    }
}