using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DynamicTriangleController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float turnSpeed = 180f;
    [SerializeField] private float _maxSpeed = 10f;

    [Header("Triangle Animation Settings")]
    [SerializeField] float baseTriangleSize = 1f;
    [SerializeField] float sizeMultiplierOnSpeed = 0.5f;
    [SerializeField] float pulsationSpeed = 3f;
    [SerializeField] Color baseColor = Color.white;
    [SerializeField] Color speedColor = Color.red;

    [Header("Direction Indicator")]
    [SerializeField] bool showDirectionIndicator = true;
    [SerializeField] float indicatorLength = 1f;
    [SerializeField] Color indicatorColor = Color.green;

    [Header("Trail Settings")]
    [SerializeField] int trailLength = 20;
    [SerializeField] float trailFadeTime = 1f;

    private LineRenderer directionIndicator;

    public Vector2 velocity { get; private set; }
    public float currentSpeed { get; private set; }
    public float MaxSpeed => _maxSpeed;
    private SpriteRenderer spriteRenderer;
    private List<TrailPoint> trailPoints = new List<TrailPoint>();
    
    private struct TrailPoint
    {
        public Vector3 position;
        public float timestamp;
        public float intensity;
    }

    void Start()
    {
        SetupDynamicTriangle();
    }

    void Update()
    {
        HandleInput();
        UpdateMovement();
        UpdateTriangleVisuals();
        UpdateTrail();
        UpdateDirectionIndicator();
        DrawTriangle();
    }

    void SetupDynamicTriangle()
    {
        // Create sprite renderer component if it doesn't exist
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Setup direction indicator
        SetupDirectionIndicator();

        // Create initial triangle texture
        CreateTriangleTexture();
    }

    void HandleInput()
    {
        float moveInput = 0f;
        float steerInput = 0f;

        if (Keyboard.current.upArrowKey.isPressed)
            moveInput = 1f;
        else if (Keyboard.current.downArrowKey.isPressed)
            moveInput = -1f;

        if (Keyboard.current.leftArrowKey.isPressed)
            steerInput = 1f;
        else if (Keyboard.current.rightArrowKey.isPressed)
            steerInput = -1f;

        // Get the forward direction based on current rotation (transform.up points forward in 2D)
        Vector2 forwardDirection = transform.up;
        
        // Apply movement in the direction the object is facing
        velocity += forwardDirection * moveInput * moveSpeed * Time.deltaTime;
        
        // Apply Z-axis rotation for 2D game
        transform.Rotate(0, 0, steerInput * turnSpeed * Time.deltaTime);

        // Apply drag
        velocity *= 0.95f;
        velocity = Vector2.ClampMagnitude(velocity, _maxSpeed);
    }

    void UpdateMovement()
    {
        currentSpeed = velocity.magnitude;
        // Move in world space - keep it simple for 2D
        transform.Translate(velocity * Time.deltaTime, Space.World);
    }

    void UpdateTriangleVisuals()
    {
        // Update color based on speed
        float speedRatio = currentSpeed / _maxSpeed;
        Color currentColor = Color.Lerp(baseColor, speedColor, speedRatio);
        
        // Add pulsation effect
        float pulse = Mathf.Sin(Time.time * pulsationSpeed * (1 + speedRatio)) * 0.2f + 1f;
        currentColor.a = pulse * 0.8f + 0.2f;
        
        spriteRenderer.color = currentColor;

        // Update size based on speed
        float sizeMultiplier = 1f + (speedRatio * sizeMultiplierOnSpeed);
        transform.localScale = Vector3.one * baseTriangleSize * sizeMultiplier * pulse;
    }

    void UpdateTrail()
    {
        // Add current position to trail
        if (currentSpeed > 0.1f)
        {
                    Debug.Log("Updating Trail");
            trailPoints.Add(new TrailPoint
            {
                position = transform.position,
                timestamp = Time.time,
                intensity = currentSpeed / _maxSpeed
            });
        }

        // Remove old trail points
        trailPoints.RemoveAll(point => Time.time - point.timestamp > trailFadeTime);
        
        // Limit trail length
        while (trailPoints.Count > trailLength)
        {
            trailPoints.RemoveAt(0);
        }
    }

    void CreateTriangleTexture()
    {
        int textureSize = 64;
        Texture2D triangleTexture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        // Create triangle shape
        Vector2 center = new Vector2(textureSize * 0.5f, textureSize * 0.5f);
        float radius = textureSize * 0.4f;

        // Triangle vertices (pointing up)
        Vector2[] vertices = new Vector2[3]
        {
            center + new Vector2(0, radius),                    // Top
            center + new Vector2(-radius * 0.866f, -radius * 0.5f),  // Bottom left
            center + new Vector2(radius * 0.866f, -radius * 0.5f)    // Bottom right
        };

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                Vector2 point = new Vector2(x, y);
                
                if (IsPointInTriangle(point, vertices[0], vertices[1], vertices[2]))
                {
                    // Create gradient effect from center
                    float distFromCenter = Vector2.Distance(point, center) / radius;
                    float alpha = 1f - distFromCenter * 0.5f;
                    pixels[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    pixels[y * textureSize + x] = Color.clear;
                }
            }
        }

        triangleTexture.SetPixels(pixels);
        triangleTexture.Apply();

        // Create sprite from texture
        Sprite triangleSprite = Sprite.Create(triangleTexture, 
            new Rect(0, 0, textureSize, textureSize), 
            new Vector2(0.5f, 0.5f), 
            textureSize);
        
        spriteRenderer.sprite = triangleSprite;
    }

    bool IsPointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
    {
        float denominator = ((b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y));
        float alpha = ((b.y - c.y) * (point.x - c.x) + (c.x - b.x) * (point.y - c.y)) / denominator;
        float beta = ((c.y - a.y) * (point.x - c.x) + (a.x - c.x) * (point.y - c.y)) / denominator;
        float gamma = 1 - alpha - beta;

        return alpha >= 0 && beta >= 0 && gamma >= 0;
    }

    void DrawTriangle()
    {
        // Update triangle shape dynamically based on movement
        if (currentSpeed > 0.5f)
        {
            CreateDynamicTriangleTexture();
        }
    }

    void CreateDynamicTriangleTexture()
    {
        int textureSize = 64;
        Texture2D triangleTexture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        Vector2 center = new Vector2(textureSize * 0.5f, textureSize * 0.5f);
        float radius = textureSize * 0.4f;

        // Modify triangle shape based on speed and direction
        float speedRatio = currentSpeed / _maxSpeed;
        float elongation = 1f + speedRatio * 0.5f; // Stretch triangle when moving fast
        
        // Triangle vertices with dynamic shape
        Vector2[] vertices = new Vector2[3]
        {
            center + new Vector2(0, radius * elongation),                           // Top (elongated)
            center + new Vector2(-radius * 0.866f, -radius * 0.5f),               // Bottom left
            center + new Vector2(radius * 0.866f, -radius * 0.5f)                 // Bottom right
        };

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                Vector2 point = new Vector2(x, y);
                
                if (IsPointInTriangle(point, vertices[0], vertices[1], vertices[2]))
                {
                    float distFromCenter = Vector2.Distance(point, center) / radius;
                    float alpha = 1f - distFromCenter * 0.3f;
                    
                    // Add speed-based color intensity
                    float intensity = 0.5f + speedRatio * 0.5f;
                    pixels[y * textureSize + x] = new Color(intensity, intensity, intensity, alpha);
                }
                else
                {
                    pixels[y * textureSize + x] = Color.clear;
                }
            }
        }

        triangleTexture.SetPixels(pixels);
        triangleTexture.Apply();

        Sprite dynamicSprite = Sprite.Create(triangleTexture, 
            new Rect(0, 0, textureSize, textureSize), 
            new Vector2(0.5f, 0.5f), 
            textureSize);
        
        spriteRenderer.sprite = dynamicSprite;
    }

    void SetupDirectionIndicator()
    {
        if (!showDirectionIndicator) return;

        // Create a child object for the direction indicator
        GameObject indicatorObj = new GameObject("DirectionIndicator");
        indicatorObj.transform.SetParent(transform);
        indicatorObj.transform.localPosition = Vector3.zero;

        directionIndicator = indicatorObj.AddComponent<LineRenderer>();
        directionIndicator.material = new Material(Shader.Find("Sprites/Default"));
        directionIndicator.material.color = indicatorColor;
        directionIndicator.startWidth = 0.05f;
        directionIndicator.endWidth = 0.02f;
        directionIndicator.positionCount = 2;
        directionIndicator.useWorldSpace = true;
        directionIndicator.sortingOrder = 1; // Draw on top
    }

    void UpdateDirectionIndicator()
    {
        if (!showDirectionIndicator || directionIndicator == null) return;

        // Only show when a movement key is pressed
        bool isMoving = Keyboard.current.upArrowKey.isPressed || 
                       Keyboard.current.downArrowKey.isPressed ||
                       Keyboard.current.leftArrowKey.isPressed || 
                       Keyboard.current.rightArrowKey.isPressed;

        directionIndicator.enabled = isMoving;

        if (isMoving)
        {
            // Calculate triangle tip position (front of triangle)
            Vector3 triangleTip = transform.position + (Vector3)transform.up * (baseTriangleSize * 0.5f);
            
            // Direction vector based on current facing direction
            Vector3 directionEnd = triangleTip + (Vector3)transform.up * indicatorLength;

            // Set line positions
            directionIndicator.SetPosition(0, triangleTip);
            directionIndicator.SetPosition(1, directionEnd);

            // Update color based on input
            if (Keyboard.current.upArrowKey.isPressed)
                directionIndicator.material.color = Color.green;  // Forward - green
            else if (Keyboard.current.downArrowKey.isPressed)
                directionIndicator.material.color = Color.red;    // Backward - red
            else
                directionIndicator.material.color = Color.yellow; // Turning only - yellow
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw trail in editor
        Gizmos.color = Color.yellow;
        for (int i = 0; i < trailPoints.Count - 1; i++)
        {
            float fade = (float)i / trailPoints.Count;
            Gizmos.color = Color.Lerp(Color.clear, Color.yellow, fade * trailPoints[i].intensity);
            if (i < trailPoints.Count - 1)
            {
                Gizmos.DrawLine(trailPoints[i].position, trailPoints[i + 1].position);
            }
        }

        // Draw velocity vector
        if (Application.isPlaying && currentSpeed > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, (Vector3)velocity.normalized * 2f);
        }
    }
}
