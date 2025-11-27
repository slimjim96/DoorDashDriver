using UnityEngine;
using System.Collections.Generic;

public class Simple2DEffects : MonoBehaviour
{
    [Header("Runtime 2D Effects")]
    [SerializeField] bool enableSimpleEffects = true;
    [SerializeField] float effectIntensity = 1f;
    [SerializeField] Color speedLineColor = Color.cyan;
    [SerializeField] Color boostColor = Color.yellow;
    [SerializeField] int maxSpeedLines = 5;
    [SerializeField] float boostThreshold = 7f;
    
    private DynamicTriangleController triangleController;
    private List<SpeedLine> speedLines = new List<SpeedLine>();
    private List<BoostDot> boostDots = new List<BoostDot>();
    private float lastBoostTime;
    
    private struct SpeedLine
    {
        public Vector3 startPos;
        public Vector3 endPos;
        public float timestamp;
        public float alpha;
        public LineRenderer lineRenderer;
    }
    
    private struct BoostDot
    {
        public Vector3 position;
        public Vector3 velocity;
        public float timestamp;
        public float size;
        public GameObject dotObject;
        public SpriteRenderer renderer;
    }
    
    void Start()
    {
        triangleController = GetComponent<DynamicTriangleController>();
    }
    
    void Update()
    {
        if (!enableSimpleEffects || triangleController == null) return;
        
        UpdateSpeedLines();
        UpdateBoostDots();
        CheckForBoost();
    }
    
    void UpdateSpeedLines()
    {
        if (triangleController == null) return;
        
        float currentSpeed = triangleController.currentSpeed;
        
        // Create new speed lines when moving (simpler timing)
        if (currentSpeed > 0.5f && speedLines.Count < maxSpeedLines)
        {
            // Create a line every few frames when moving
            if (Time.frameCount % 10 == 0)
            {
                Debug.Log($"Creating speed line at speed: {currentSpeed}");
                CreateSpeedLine();
            }
        }
        
        // Update existing speed lines
        for (int i = speedLines.Count - 1; i >= 0; i--)
        {
            var line = speedLines[i];
            float age = Time.time - line.timestamp;
            
            if (age > 1f || line.lineRenderer == null)
            {
                // Remove old lines
                if (line.lineRenderer != null)
                {
                    DestroyImmediate(line.lineRenderer.gameObject);
                }
                speedLines.RemoveAt(i);
            }
            else
            {
                // Fade out the line
                float alpha = 1f - (age / 1f);
                Color currentColor = speedLineColor;
                currentColor.a = alpha * 0.7f;
                line.lineRenderer.material.color = currentColor;
            }
        }
    }
    
    void CreateSpeedLine()
    {
        if (speedLines.Count >= maxSpeedLines) return;
        
        // Create a small line behind the triangle
        Vector3 backDirection = -transform.up;
        Vector3 startPos = transform.position + backDirection * 0.3f;
        Vector3 endPos = startPos + backDirection * (0.2f + triangleController.currentSpeed * 0.1f);
        
        // Add some random offset
        Vector3 offset = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
        startPos += offset;
        endPos += offset;
        
        GameObject lineObj = new GameObject("SpeedLine");
        lineObj.transform.SetParent(transform);
        
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = speedLineColor;
        lr.startWidth = 0.03f;
        lr.endWidth = 0.01f;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.sortingOrder = -2;
        
        lr.SetPosition(0, startPos);
        lr.SetPosition(1, endPos);
        
        SpeedLine newLine = new SpeedLine
        {
            startPos = startPos,
            endPos = endPos,
            timestamp = Time.time,
            alpha = 1f,
            lineRenderer = lr
        };
        
        speedLines.Add(newLine);
    }
    
    void CheckForBoost()
    {
        if (triangleController == null) return;
        
        float currentSpeed = triangleController.currentSpeed;
        
        // Create boost effect when hitting high speed
        if (currentSpeed > boostThreshold && Time.time - lastBoostTime > 0.5f)
        {
            CreateBoostBurst();
            lastBoostTime = Time.time;
        }
    }
    
    void CreateBoostBurst()
    {
        // Create 3-5 boost dots that fly outward
        int dotCount = Random.Range(3, 6);
        
        for (int i = 0; i < dotCount; i++)
        {
            Vector3 randomDirection = Random.insideUnitCircle.normalized;
            Vector3 velocity = randomDirection * Random.Range(2f, 4f);
            
            GameObject dotObj = CreateDotObject();
            dotObj.transform.position = transform.position;
            
            BoostDot dot = new BoostDot
            {
                position = transform.position,
                velocity = velocity,
                timestamp = Time.time,
                size = Random.Range(0.05f, 0.15f),
                dotObject = dotObj,
                renderer = dotObj.GetComponent<SpriteRenderer>()
            };
            
            boostDots.Add(dot);
        }
    }
    
    GameObject CreateDotObject()
    {
        GameObject dotObj = new GameObject("BoostDot");
        dotObj.transform.SetParent(transform);
        
        SpriteRenderer sr = dotObj.AddComponent<SpriteRenderer>();
        
        // Create a simple circle texture
        Texture2D circleTexture = CreateCircleTexture(16);
        Sprite circleSprite = Sprite.Create(circleTexture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f));
        
        sr.sprite = circleSprite;
        sr.color = boostColor;
        sr.sortingOrder = 1;
        
        return dotObj;
    }
    
    Texture2D CreateCircleTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.4f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = distance < radius ? 1f - (distance / radius) : 0f;
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    void UpdateBoostDots()
    {
        for (int i = boostDots.Count - 1; i >= 0; i--)
        {
            var dot = boostDots[i];
            float age = Time.time - dot.timestamp;
            
            if (age > 0.8f || dot.dotObject == null)
            {
                // Remove old dots
                if (dot.dotObject != null)
                {
                    DestroyImmediate(dot.dotObject);
                }
                boostDots.RemoveAt(i);
            }
            else
            {
                // Update dot position and appearance
                dot.position += dot.velocity * Time.deltaTime;
                dot.dotObject.transform.position = dot.position;
                
                // Fade and shrink over time
                float fadeRatio = 1f - (age / 0.8f);
                Color color = boostColor;
                color.a = fadeRatio;
                dot.renderer.color = color;
                
                float scale = dot.size * fadeRatio;
                dot.dotObject.transform.localScale = Vector3.one * scale;
                
                // Update the struct in the list
                boostDots[i] = dot;
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up any remaining objects
        foreach (var line in speedLines)
        {
            if (line.lineRenderer != null)
            {
                DestroyImmediate(line.lineRenderer.gameObject);
            }
        }
        
        foreach (var dot in boostDots)
        {
            if (dot.dotObject != null)
            {
                DestroyImmediate(dot.dotObject);
            }
        }
    }
}