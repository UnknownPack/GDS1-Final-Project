using UnityEngine;
using System.Collections;

public class MovementTutorialTrigger : MonoBehaviour
{
    public GameObject movementTutorialText;    
    public GameObject sliderTutorialText;       

    private bool hasTriggered = false;        

    private void Start()
    {
        if (sliderTutorialText != null)
            sliderTutorialText.SetActive(false);

        if (movementTutorialText != null)
            movementTutorialText.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(SwitchTextAfterDelay(3f));
        }
    }

    private IEnumerator SwitchTextAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        if (movementTutorialText != null)
            movementTutorialText.SetActive(false);

        if (sliderTutorialText != null)
            sliderTutorialText.SetActive(true);
    }
}
