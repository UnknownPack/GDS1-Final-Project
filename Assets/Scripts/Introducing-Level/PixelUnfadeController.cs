using UnityEngine;
using UnityEngine.UI;

public class PixelUnfadeController : MonoBehaviour
{
    public RawImage fadeImage;
    public float duration = 2f;

    private Material fadeMaterial;
    private float progress = 1f;

    void Start()
    {
        fadeMaterial = Instantiate(fadeImage.material);
        fadeImage.material = fadeMaterial;
        fadeMaterial.SetFloat("_Progress", 1f);

        StartCoroutine(Unfade());
    }

    System.Collections.IEnumerator Unfade()
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            progress = Mathf.Clamp01(1f - t / duration);
            fadeMaterial.SetFloat("_Progress", progress);
            yield return null;
        }

        fadeMaterial.SetFloat("_Progress", 0f);
    }
}
