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

    
    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCollider2D;
    private float currentAlpha;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        switch (startingState)
        {
            case LightBlockState.DefaultEnabled:
                currentAlpha = 1f;
                boxCollider2D.enabled = true;
                break;
            case LightBlockState.DefaultDisabled:
                currentAlpha = 0f;
                boxCollider2D.enabled = false;
                break;
        }
 
        Color colour = spriteRenderer.color;
        spriteRenderer.color = new Color(colour.r, colour.g, colour.b, currentAlpha);
    }

    void Update()
    { 

    }

    public void ChangeBrightness(float brightness) {
        if (spriteRenderer == null || boxCollider2D == null) {
            return;
        }
        brightness = brightness / 2;
        if (activationState == Activation.FullLight) 
            currentAlpha = 1f - brightness;  
        else 
            currentAlpha = brightness;  
        
        Color colour = spriteRenderer.color;
        spriteRenderer.color = new Color(colour.r, colour.g, colour.b, currentAlpha); 
        if (Mathf.Approximately(currentAlpha, 1f)) 
            boxCollider2D.enabled = true; 
        else if (Mathf.Approximately(currentAlpha, 0f)) 
            boxCollider2D.enabled = false; 
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

