using UnityEngine;
using System.Collections;

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
            StartCoroutine(DelayedSwitch());
        }
    }

    private IEnumerator DelayedSwitch()
    {
        yield return new WaitForSeconds(3f); 
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
