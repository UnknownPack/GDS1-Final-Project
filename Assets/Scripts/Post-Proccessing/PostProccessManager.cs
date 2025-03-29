using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PostProccessManager : MonoBehaviour
{
    private Light2D light2D;
    [SerializeField] private Volume volume;
    [SerializeField] private UniversalRenderPipelineAsset urpAsset;
    private float brightnessValue;
    public float finalBrightness = 1.0f;
    float ScaleBrightness(float x, float y) => (1 + (x - 1) * y);

    
    void Start()
    {
        light2D = GetComponent<Light2D>();
    }

     
    void Update()
    {
        
    }

    public void ChangeAntiAlyasing () {
        //urpAsset.renderScale = antiAlyiasingSlider.value;
    }

    public void ChangeBrightness() {
        //float brightnessValue = brightnessSlider.value;
        if (brightnessValue > 1) {
            // brightnessValue = 1 + (brightnessValue - 1)/;
            brightnessValue = ScaleBrightness(brightnessValue, finalBrightness);
        }
        light2D.intensity = brightnessValue;
    }

    public void AdjustValue(GameManager.PostProcessingEffect effect, float value)
    {
        if(effect == GameManager.PostProcessingEffect.Brightness)   
            light2D.intensity = value;
        if (effect == GameManager.PostProcessingEffect.AntiAliasing)
            urpAsset.renderScale = value;
        if (effect == GameManager.PostProcessingEffect.MotionBlur)
        {
            /*TODO: IMmplemnt value change here*/
        }
    }
}
