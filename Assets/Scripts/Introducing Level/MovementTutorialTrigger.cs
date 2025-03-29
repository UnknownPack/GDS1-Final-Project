using UnityEngine;

public class MovementTutorialTrigger : MonoBehaviour
{
    public GameObject movementTutorialText;
    public GameObject sliderTutorialText;

    private bool hasTriggered = false;

    void Start()
    {
        if (sliderTutorialText != null)
        {
            sliderTutorialText.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            if (movementTutorialText != null)
            {
                movementTutorialText.SetActive(false);
            }
            if (sliderTutorialText != null)
            {
                sliderTutorialText.SetActive(true);
            }
        }
    }
}
