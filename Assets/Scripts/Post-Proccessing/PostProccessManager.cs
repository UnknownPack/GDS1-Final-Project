using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PostProccessManager : MonoBehaviour
{

    public static PostProccessManager Instance { get; private set; }
    private Light2D light2D;
    private Volume volume;
    private MotionBlur motionBlur;
    [SerializeField] private UniversalRenderPipelineAsset urpAsset;
    private PlayerTrail playerTrail;
    public float finalBrightness = 1.0f;
    float ScaleBrightness(float x, float y) => (1 + (x - 1) * y);
    float ScaleAntiAliasing(float x) =>  (0.9f + (x * (1.5f - 0.9f)));
    float ScaleAntiAliasing2(float x) =>  (1f + (x * (100f - 1f)));

      private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }

    
    void Start()
    {
        playerTrail = GetComponent<PlayerTrail>();
        light2D = GetComponent<Light2D>();
        volume = GetComponent<Volume>();
        Debug.Log(volume);
        volume.profile.TryGet(out motionBlur);
    }

     
    void Update()
    {
        
    }

    public void ChangeAntiAlyasing (float value) {
        urpAsset.renderScale = ScaleAntiAliasing(value);
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Spikes");
        foreach (GameObject obj in objects)
        {
            SpikeCobntroiller spikeCobntroiller = obj.GetComponent<SpikeCobntroiller>();
            if (spikeCobntroiller != null)
            {
                spikeCobntroiller.UpdateVerticies(ScaleAntiAliasing2(value));
            }
        }
    }

    public void ChangeBrightness (float value) {
        //float brightnessValue = brightnessSlider.value;
        if (value > 1) {
            // brightnessValue = 1 + (brightnessValue - 1)/;
            value = ScaleBrightness(value, finalBrightness);
        }
        light2D.intensity = value;
    }

    public void ChangeMotionBlur (float value) {    
        motionBlur.intensity.value = value;
        playerTrail.trailLifetime = value;
    }

    // public void AdjustValue(GameManager.PostProcessingEffect effect, float value)
    // {
    //     if(effect == GameManager.PostProcessingEffect.Brightness)   
    //         light2D.intensity = value;
    //     if (effect == GameManager.PostProcessingEffect.AntiAliasing)
    //         urpAsset.renderScale = value;
    //     if (effect == GameManager.PostProcessingEffect.MotionBlur)
    //     {
    //         /*TODO: IMmplemnt value change here*/
    //     }
    // }
}
