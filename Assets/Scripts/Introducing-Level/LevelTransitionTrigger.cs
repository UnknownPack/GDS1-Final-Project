using UnityEngine;

public class LevelTransitionTrigger : MonoBehaviour
{
    public string nextSceneName; // 下一个场景的名字，比如 "Level2"
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            FindObjectOfType<PixelTransitionController>().FadeToScene(nextSceneName);
        }
    }
}
