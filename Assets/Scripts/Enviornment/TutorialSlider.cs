using System.Collections;
using UnityEngine;

public class TutorialMove : MonoBehaviour
{
    public GameManager.PostProcessingEffect postProcessingEffect;
    public float transitionTimeToMax = 0.5f;
    public float transitionTimeToDefault = 0.5f;
    public float transitionTimeToMin = 0.5f;

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
            yield return new WaitForSeconds(transitionTimeToDefault);
            // Default to Max
            GameManager.Instance.TransitionExternal(postProcessingEffect, GameManager.Setting.Max, 0.5f);
            yield return new WaitForSeconds(transitionTimeToMax);

            // Max to Default
            GameManager.Instance.TransitionExternal(postProcessingEffect, GameManager.Setting.Default, 0.5f);
            yield return new WaitForSeconds(transitionTimeToDefault);

            // Default to Min
            GameManager.Instance.TransitionExternal(postProcessingEffect, GameManager.Setting.Min, 0.5f);
            yield return new WaitForSeconds(transitionTimeToMin);
        }
    }
}
