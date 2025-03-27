using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PostProccessManager : MonoBehaviour
{
    [SerializeField] private Light2D light2D;
    [SerializeField] private Slider brightnessSlider;
    private float brightnessValue;
    public float finalBrightness = 1.0f;
    float ScaleBrightness(float x, float y) => (1 + (x - 1) * y);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        light2D = light2D.GetComponent<Light2D>();
        brightnessSlider = brightnessSlider.GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeBrightness() {
        float brightnessValue = brightnessSlider.value;
        if (brightnessValue > 1) {
            // brightnessValue = 1 + (brightnessValue - 1)/;
            brightnessValue = ScaleBrightness(brightnessValue, finalBrightness);
        }
        light2D.intensity = brightnessValue;
    }
}
