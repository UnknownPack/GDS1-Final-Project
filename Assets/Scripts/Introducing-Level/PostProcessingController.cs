using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingController : MonoBehaviour
{
    public Volume volume;
    public bool playerControlEnabled = false;

    private ColorAdjustments colorAdjust;
    private FilmGrain grain;
    private MotionBlur motionBlur;

    private float flickerTimer;

    void Start()
    {
        volume.profile.TryGet(out colorAdjust);
        volume.profile.TryGet(out grain);
        volume.profile.TryGet(out motionBlur);
    }

    void Update()
    {
        if (!playerControlEnabled)
        {
            SimulateSystemInterference();
        }
        else
        {
        }
    }

    void SimulateSystemInterference()
    {
        flickerTimer += Time.deltaTime * 2f;
        colorAdjust.postExposure.value = Mathf.Sin(flickerTimer) * 2f;

        colorAdjust.colorFilter.value = new Color(0.4f, 1.0f, 1.0f);

        grain.active = true;
        grain.intensity.value = 1f;
        grain.response.value = 1f;

        motionBlur.active = true;
        motionBlur.intensity.value = 0.8f;
    }

    public void EnablePlayerControl()
    {
        playerControlEnabled = true;

        colorAdjust.postExposure.value = 0f;
        colorAdjust.colorFilter.value = Color.white;
        grain.intensity.value = 0.2f;
        motionBlur.intensity.value = 0f;

        grain.active = false;
        motionBlur.active = false;
    }
}
