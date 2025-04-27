using UnityEngine;

public class LevelTransitionTrigger : MonoBehaviour
{
    public string nextSceneName; 
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            FindFirstObjectByType<PixelTransitionController>().FadeToScene(nextSceneName);
        }
    }
}
