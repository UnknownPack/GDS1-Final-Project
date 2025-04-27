using UnityEngine;

public class SkipPlotManager : MonoBehaviour
{
    public string targetSceneName = "SliderIntroScene";

    public void SkipPlot()
    {
        FindFirstObjectByType<PixelTransitionController>().FadeToScene(targetSceneName);
    }
}
