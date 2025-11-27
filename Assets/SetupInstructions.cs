using UnityEngine;

[System.Serializable]
public class SetupInstructions : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(10, 15)]
    public string instructions = @"DYNAMIC TRIANGLE SETUP INSTRUCTIONS - CLEAN 2D VERSION:

1. ADD COMPONENTS:
   - Add 'TriangleSetup' script to your game object
   - Check 'Auto Setup On Start' in the inspector
   - OR right-click this component → 'Setup Triangle'

2. VERIFY COMPONENTS (Right-click TriangleSetup → 'Debug Components'):
   - DynamicTriangleController ✓ (main triangle with dynamic shape/color)
   - DynamicTrailRenderer ✓ (smooth 2D trail following triangle)
   - Simple2DEffects ✓ (lightweight visual effects for 2D)

3. PLAY THE SCENE:
   - Use Arrow Keys to move
   - Triangle changes color/size based on speed
   - Cyan trail appears when moving  
   - Green direction indicator shows when keys pressed
   - Simple speed-based visual effects

4. CLEAN 2D APPROACH:
   - Removed complex particle systems (too heavy for 2D)
   - Focused on trail + triangle morphing + simple effects
   - Better performance for 2D side-facing gameplay
   - Cleaner visual style appropriate for 2D

5. CUSTOMIZATION:
   - Adjust triangle speeds, colors, sizes in DynamicTriangleController
   - Modify trail color/width in DynamicTrailRenderer  
   - Turn on/off effects in Simple2DEffects
   - Toggle direction indicator on/off";

    [ContextMenu("Force Setup All Components")]
    void ForceSetup()
    {
        TriangleSetup setup = GetComponent<TriangleSetup>();
        if (setup == null)
        {
            setup = gameObject.AddComponent<TriangleSetup>();
        }
        setup.SetupTriangle();
        
        Debug.Log("Forced setup complete! Check console for component status.");
        setup.DebugComponents();
    }
}