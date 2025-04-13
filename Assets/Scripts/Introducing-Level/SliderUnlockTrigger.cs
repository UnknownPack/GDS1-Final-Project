using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SliderUnlockTrigger : MonoBehaviour
{
    [Header("References")]
    public GameObject sliderPrefab;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            gameObject.SetActive(false);
        };
        if (SceneManager.GetActiveScene().name != "SliderIntroScene") return;
        triggered = true;

        if (sliderPrefab != null)
        {
            sliderPrefab.SetActive(true);
            GameManager gm = GameManager.Instance;
            if (gm != null)
            {
                gm.EnableSliderManually(sliderPrefab, GameManager.SliderType.Brightness);
            }
        }
    }
}
