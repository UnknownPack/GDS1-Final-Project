using System;
using UnityEngine;

public class LightBlocks : MonoBehaviour
{
    [Header("light Block Settings")] 
    [SerializeField, Tooltip("Sets when the block will be activated")] 
    private Activation activationState;

    [SerializeField, Tooltip("Sets the starting state of the block ")]
    private LightBlockState startingState;
    
    [SerializeField] private float enableThreshold = 0.9f;
    [SerializeField] private float disableThreshold = 0.1f;
    private const float BRIGHTNESS_TOLERANCE = 0.025f;
    private const float MAX_BRIGHTNESS = 2.0f;

    
    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCollider2D;
    PolygonCollider2D polyCollider2D;
    private float currentAlpha;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        polyCollider2D = GetComponent<PolygonCollider2D>();

        switch (startingState)
        {
            case LightBlockState.DefaultEnabled:
                currentAlpha = 1f;
                if(boxCollider2D != null)
                    boxCollider2D.enabled = true;
                if(polyCollider2D != null)
                    polyCollider2D.enabled = true; 
                break;
            case LightBlockState.DefaultDisabled:
                currentAlpha = 0.3f;
                if(boxCollider2D != null)
                    boxCollider2D.enabled = false;
                if(polyCollider2D != null)
                    polyCollider2D.enabled = false;
                break;
        }
 
        Color colour = spriteRenderer.color;
        spriteRenderer.color = new Color(colour.r, colour.g, colour.b, currentAlpha);
    }

    void Update()
    { 

    }

    public void ChangeBrightness(float brightness) {
        if (spriteRenderer == null) {
            return;
        }

        float normalizedBrightness = brightness / MAX_BRIGHTNESS;
        Debug.Log($"Yellow Block - Raw Brightness: {brightness}, Normalized: {normalizedBrightness}, EnableThreshold: {enableThreshold}, DisableThreshold: {disableThreshold}");

        if (activationState == Activation.FullLight) {
            currentAlpha = 1.0f - normalizedBrightness;
            
            if (normalizedBrightness <= (disableThreshold + BRIGHTNESS_TOLERANCE)) {
                if(boxCollider2D != null)
                    boxCollider2D.enabled = true;
                if(polyCollider2D != null)
                    polyCollider2D.enabled = true;
                Debug.Log("Yellow Block Enabled");
            }
            else if (normalizedBrightness >= (enableThreshold - BRIGHTNESS_TOLERANCE)) {
                if(boxCollider2D != null)
                    boxCollider2D.enabled = false;
                if(polyCollider2D != null)
                    polyCollider2D.enabled = false;
                Debug.Log("Yellow Block Disabled");
            }
            else {
                // Return to starting state when in default brightness range
                bool shouldBeEnabled = (startingState == LightBlockState.DefaultEnabled);
                if(boxCollider2D != null)
                    boxCollider2D.enabled = shouldBeEnabled;
                if(polyCollider2D != null)
                    polyCollider2D.enabled = shouldBeEnabled;
                Debug.Log($"Yellow Block Default State ({(shouldBeEnabled ? "Enabled" : "Disabled")})");
            }
        } else {
            currentAlpha = normalizedBrightness;
            if (currentAlpha >= (enableThreshold - BRIGHTNESS_TOLERANCE)) {
                if(boxCollider2D != null)
                    boxCollider2D.enabled = true;
                if(polyCollider2D != null)
                    polyCollider2D.enabled = true;
                Debug.Log("Blue Block Enabled");
            }
            else if (currentAlpha <= (disableThreshold + BRIGHTNESS_TOLERANCE)) {
                if(boxCollider2D != null)
                    boxCollider2D.enabled = false;
                if(polyCollider2D != null)
                    polyCollider2D.enabled = false;
                Debug.Log("Blue Block Disabled");
            }
            else {
                // Return to starting state when in default brightness range
                bool shouldBeEnabled = (startingState == LightBlockState.DefaultEnabled);
                if(boxCollider2D != null)
                    boxCollider2D.enabled = shouldBeEnabled;
                if(polyCollider2D != null)
                    polyCollider2D.enabled = shouldBeEnabled;
                Debug.Log($"Blue Block Default State ({(shouldBeEnabled ? "Enabled" : "Disabled")})");
            }
        }
        
        Color colour = spriteRenderer.color;
        spriteRenderer.color = new Color(colour.r, colour.g, colour.b, currentAlpha); 
    }
    
    private enum LightBlockState{
        DefaultEnabled,
        DefaultDisabled,
    }

    private enum Activation
    {
        FullDark,
        FullLight,
    }
}

