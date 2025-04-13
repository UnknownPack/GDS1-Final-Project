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
    private FilmGrain filmGrain;
    private ChannelMixer channelMixer;
    private ChromaticAberration chromaticAberration;
    private Bloom bloom;
    [SerializeField] private UniversalRenderPipelineAsset urpAsset;
    private PlayerTrail playerTrail;
    public float finalBrightness = 1.0f;
    private GameObject player;
    private PlayerController playerController;
    float ScaleBrightness(float x, float y) => (1 + (x - 1) * y);
    float ScaleAntiAliasing(float x) =>  (0.9f + (x * (1.5f - 0.9f)));
    float ScaleAntiAliasing2(float x) =>  (1f + (x * (100f - 1f)));
    
    private ColourCorrectionBlocks.ColorChannelMatrix currentColorChannelMatrix;

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
        volume.profile.TryGet(out motionBlur);
        volume.profile.TryGet(out filmGrain);
        volume.profile.TryGet(out channelMixer);
        volume.profile.TryGet(out chromaticAberration);
        volume.profile.TryGet(out bloom);
        
        Vector3 redResult   = new Vector3(
            channelMixer.redOutRedIn.value,
            channelMixer.redOutGreenIn.value,
            channelMixer.redOutBlueIn.value);

        Vector3 greenResult = new Vector3(
            channelMixer.greenOutRedIn.value,
            channelMixer.greenOutGreenIn.value,
            channelMixer.greenOutBlueIn.value);

        Vector3 blueResult  = new Vector3(
            channelMixer.blueOutRedIn.value,
            channelMixer.blueOutGreenIn.value,
            channelMixer.blueOutBlueIn.value);
        
        currentColorChannelMatrix = new ColourCorrectionBlocks.ColorChannelMatrix(redResult, greenResult, blueResult);
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

    public void ChangeFilmGrain(float value)
    {
        filmGrain.intensity.value = value;
        bool shouldEnableJump = value > 0.95f;
    
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("No GameObject with tag 'Player' found!");
                return;
            }
        }

        if (playerController == null)
        {
            playerController = player.GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerScript component not found on player!");
                return;
            }
        } 
        playerController.SetJumpStatus(shouldEnableJump);
    }
    public void ChangeColorCorrection(float value)
    {

        // Default channel weights (identity matrix)
        Vector3 redChannel   = new Vector3(100, 0, 0);
        Vector3 greenChannel = new Vector3(0, 100, 0);
        Vector3 blueChannel  = new Vector3(0, 0, 100);

        // Left shift (RGB → BRG)
        Vector3 redLeft   = new Vector3(0, 0, 100);
        Vector3 greenLeft = new Vector3(100, 0, 0);
        Vector3 blueLeft  = new Vector3(0, 100, 0);

        // Right shift (RGB → GBR)
        Vector3 redRight   = new Vector3(0, 100, 0);
        Vector3 greenRight = new Vector3(0, 0, 100);
        Vector3 blueRight  = new Vector3(100, 0, 0);

        // Interpolate based on slider value
        if (value < 0)
        {
            // Lerp between normal and left shift
            float t = Mathf.Abs(value);
            channelMixer.redOutRedIn.value   = Mathf.Lerp(redChannel.x, redLeft.x, t);
            channelMixer.redOutGreenIn.value = Mathf.Lerp(redChannel.y, redLeft.y, t);
            channelMixer.redOutBlueIn.value  = Mathf.Lerp(redChannel.z, redLeft.z, t);

            channelMixer.greenOutRedIn.value   = Mathf.Lerp(greenChannel.x, greenLeft.x, t);
            channelMixer.greenOutGreenIn.value = Mathf.Lerp(greenChannel.y, greenLeft.y, t);
            channelMixer.greenOutBlueIn.value  = Mathf.Lerp(greenChannel.z, greenLeft.z, t);

            channelMixer.blueOutRedIn.value   = Mathf.Lerp(blueChannel.x, blueLeft.x, t);
            channelMixer.blueOutGreenIn.value = Mathf.Lerp(blueChannel.y, blueLeft.y, t);
            channelMixer.blueOutBlueIn.value  = Mathf.Lerp(blueChannel.z, blueLeft.z, t);
        }
        else
        {
            // Lerp between normal and right shift
            float t = value;
            channelMixer.redOutRedIn.value   = Mathf.Lerp(redChannel.x, redRight.x, t);
            channelMixer.redOutGreenIn.value = Mathf.Lerp(redChannel.y, redRight.y, t);
            channelMixer.redOutBlueIn.value  = Mathf.Lerp(redChannel.z, redRight.z, t);

            channelMixer.greenOutRedIn.value   = Mathf.Lerp(greenChannel.x, greenRight.x, t);
            channelMixer.greenOutGreenIn.value = Mathf.Lerp(greenChannel.y, greenRight.y, t);
            channelMixer.greenOutBlueIn.value  = Mathf.Lerp(greenChannel.z, greenRight.z, t);

            channelMixer.blueOutRedIn.value   = Mathf.Lerp(blueChannel.x, blueRight.x, t);
            channelMixer.blueOutGreenIn.value = Mathf.Lerp(blueChannel.y, blueRight.y, t);
            channelMixer.blueOutBlueIn.value  = Mathf.Lerp(blueChannel.z, blueRight.z, t);
        }
        Vector3 redResult   = new Vector3(
            channelMixer.redOutRedIn.value,
            channelMixer.redOutGreenIn.value,
            channelMixer.redOutBlueIn.value);

        Vector3 greenResult = new Vector3(
            channelMixer.greenOutRedIn.value,
            channelMixer.greenOutGreenIn.value,
            channelMixer.greenOutBlueIn.value);

        Vector3 blueResult  = new Vector3(
            channelMixer.blueOutRedIn.value,
            channelMixer.blueOutGreenIn.value,
            channelMixer.blueOutBlueIn.value);
        
        currentColorChannelMatrix = new ColourCorrectionBlocks.ColorChannelMatrix(redResult, greenResult, blueResult);
    }
    public void ChangeChromaticAberration(float value)
    {
        chromaticAberration.intensity.value = value;
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("No GameObject with tag 'Player' found!");
                return;
            }
        }

        if (playerController == null)
        {
            playerController = player.GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerScript component not found on player!");
                return;
            }
        } 
        playerController.SetGravityStatus(value >= 0.75f); 
    }
    public void ChangeBloom(float value)
    {
        bloom.intensity.value = value;
        //add method to crash
    }
    
    public ColourCorrectionBlocks.ColorChannelMatrix GetCurrentColorChannelMatrix(){return currentColorChannelMatrix;}
    
    public Vector3 ApplyMatrix(Color baseColor, ColourCorrectionBlocks.ColorChannelMatrix matrix)
    {
        Vector3 input = new Vector3(baseColor.r, baseColor.g, baseColor.b);

        float r = Vector3.Dot(matrix.RedChannel, input) / 100f;
        float g = Vector3.Dot(matrix.GreenChannel, input) / 100f;
        float b = Vector3.Dot(matrix.BlueChannel, input) / 100f;


        return new Vector3(r, g, b); 
    }
}
