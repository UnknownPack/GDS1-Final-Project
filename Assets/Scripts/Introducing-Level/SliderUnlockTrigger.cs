using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SliderUnlockTrigger : MonoBehaviour
{
    [Header("References")]
    public GameObject sliderPrefab;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    { 
        if (triggered || SceneManager.GetActiveScene().name != "SliderIntroScene") return; 
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.SetUIDisplay(DisplayStyle.Flex); 
        }; 
        triggered = true;
    }
}
