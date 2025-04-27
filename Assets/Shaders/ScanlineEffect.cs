// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.Universal;

// [System.Serializable, VolumeComponentMenu("Custom/Scanlines")]
// public class ScanlinesEffect : VolumeComponent, IPostProcessComponent
// {
//     // Renamed to avoid conflict with MonoBehaviour.active
//     public BoolParameter isActive = new BoolParameter(false);
//     public ClampedFloatParameter intensity = new ClampedFloatParameter(0.5f, 0, 1);
//     public ClampedIntParameter lineCount = new ClampedIntParameter(10, 1, 100);
//     public ColorParameter lineColor = new ColorParameter(Color.black);
//     public ClampedFloatParameter lineWidth = new ClampedFloatParameter(0.5f, 0, 1);
//     public BoolParameter animate = new BoolParameter(false);
//     public FloatParameter scrollSpeed = new FloatParameter(1);

//     // Flicker & Jitter
//     public BoolParameter enableFlicker = new BoolParameter(false);
//     public ClampedFloatParameter flickerSpeed = new ClampedFloatParameter(60f, 0f, 120f);
//     public ClampedFloatParameter flickerAmount = new ClampedFloatParameter(0.1f, 0f, 0.5f);
    
//     public BoolParameter enableJitter = new BoolParameter(false);
//     public ClampedFloatParameter jitterSpeed = new ClampedFloatParameter(5f, 0f, 20f);
//     public ClampedFloatParameter jitterAmount = new ClampedFloatParameter(0.002f, 0f, 0.01f);

//     // Variable Line Thickness
//     public BoolParameter enablePulsing = new BoolParameter(false);
//     public ClampedFloatParameter pulseSpeed = new ClampedFloatParameter(3f, 0f, 10f);
//     public ClampedFloatParameter pulseAmount = new ClampedFloatParameter(0.2f, 0f, 0.5f);

//     // Ghosting
//     public BoolParameter enableGhosting = new BoolParameter(false);
//     public ClampedFloatParameter ghostingAmount = new ClampedFloatParameter(0.05f, 0f, 0.2f);

//     public bool IsActive() => isActive.value;
//     public bool IsTileCompatible() => true;
// }