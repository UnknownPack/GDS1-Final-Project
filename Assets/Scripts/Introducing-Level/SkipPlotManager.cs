using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipPlotManager : MonoBehaviour
{
    [Header("Skip Scene")]
    public string nextSceneName = "SliderIntroScene";

    public void OnSkipButtonClicked()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
