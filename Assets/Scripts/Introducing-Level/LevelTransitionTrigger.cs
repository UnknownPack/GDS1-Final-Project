using UnityEngine;

public class LevelTransitionTrigger : MonoBehaviour
{
    public string nextSceneName; // ��һ�����������֣����� "Level2"
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
