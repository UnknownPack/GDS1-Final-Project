using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PixelTransitionController : MonoBehaviour
{
    public RawImage fadeImage;
    public float transitionDuration = 2f;
    public bool playUnfadeOnStart = true; 

    private Material fadeMaterial;
    private bool isTransitioning = false;

    void Awake()
    {
        fadeMaterial = Instantiate(fadeImage.material);
        fadeImage.material = fadeMaterial;
    }

    void Start()
    {
        if (playUnfadeOnStart)
        {
            fadeMaterial.SetFloat("_Progress", 1f);
            StartCoroutine(Unfade());
        }
        else
        {
            fadeMaterial.SetFloat("_Progress", 0f);
        }
    }

    public void FadeToScene(string sceneName)
    {
        if (!isTransitioning)
            StartCoroutine(FadeAndSwitchScene(sceneName));
    }

    private IEnumerator Unfade()
    {
        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(1f - t / transitionDuration);
            fadeMaterial.SetFloat("_Progress", progress);
            yield return null;
        }
        fadeMaterial.SetFloat("_Progress", 0f);
    }

    private IEnumerator FadeAndSwitchScene(string sceneName)
    {
        isTransitioning = true;
        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / transitionDuration);
            fadeMaterial.SetFloat("_Progress", progress);
            yield return null;
        }

        fadeMaterial.SetFloat("_Progress", 1f);
        yield return new WaitForSeconds(0.2f); 
        SceneManager.LoadScene(sceneName);
    }
}
