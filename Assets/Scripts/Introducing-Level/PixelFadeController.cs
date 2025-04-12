using UnityEngine;
using UnityEngine.UI;

public class PixelFadeController : MonoBehaviour
{
    public RawImage fadeImage;
    public float duration = 2f;
    private Material fadeMaterial;
    private float progress = 0f;

    void Start()
    {
        fadeMaterial = Instantiate(fadeImage.material); 
        fadeImage.material = fadeMaterial;
        fadeMaterial.SetFloat("_Progress", 0f);
    }

    public void StartFade()
    {
        StartCoroutine(FadeIn());
    }

    System.Collections.IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            progress = Mathf.Clamp01(t / duration);
            fadeMaterial.SetFloat("_Progress", progress);
            yield return null;
        }

        fadeMaterial.SetFloat("_Progress", 1f);
    }
}
