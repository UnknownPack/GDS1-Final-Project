using System.Collections;
using UnityEngine;

public class TutorialMove : MonoBehaviour
{
    public GameManager.PostProcessingEffect postProcessingEffect;
    public float MaxTime = 0.5f;
    public float DefaultTime = 0.5f;
    public float MinTime = 0.5f;

    private void Start()
    {
        StartCoroutine(TransitionCycle());
    }

    private IEnumerator TransitionCycle()
    {
        while (true)
        {
            // Min to Default
            GameManager.Instance.TransitionExternal(postProcessingEffect, GameManager.Setting.Default, 0.5f);
            yield return new WaitForSeconds(DefaultTime);
            // Default to Max
            GameManager.Instance.TransitionExternal(postProcessingEffect, GameManager.Setting.Max, 0.5f);
            yield return new WaitForSeconds(MaxTime);

            // Max to Default
            GameManager.Instance.TransitionExternal(postProcessingEffect, GameManager.Setting.Default, 0.5f);
            yield return new WaitForSeconds(DefaultTime);

            // Default to Min
            GameManager.Instance.TransitionExternal(postProcessingEffect, GameManager.Setting.Min, 0.5f);
            yield return new WaitForSeconds(MinTime);
        }
    }
}
