using UnityEngine;
using System.Collections;

public class MovementTutorialTrigger : MonoBehaviour
{
    public enum TutorialStage
    {
        Movement,
        BrightnessYellow,
        BrightnessBlue
    }

    public TutorialStage stage;

    public GameObject movementTutorialText;
    public GameObject sliderTutorialText;
    public GameObject brightnessYellowText;
    public GameObject brightnessBlueText;

    private bool hasTriggered = false;

    private void Start()
    {
        if (stage == TutorialStage.Movement)
        {
            if (movementTutorialText != null)
                movementTutorialText.SetActive(true);

            if (sliderTutorialText != null)
                sliderTutorialText.SetActive(false);
        }

        if (stage == TutorialStage.BrightnessYellow && brightnessYellowText != null)
            brightnessYellowText.SetActive(false);

        if (stage == TutorialStage.BrightnessBlue && brightnessBlueText != null)
            brightnessBlueText.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;

            switch (stage)
            {
                case TutorialStage.Movement:
                    StartCoroutine(SwitchToSliderTutorial());
                    break;

                case TutorialStage.BrightnessYellow:
                    ShowBrightnessYellowInstruction();
                    break;

                case TutorialStage.BrightnessBlue:
                    ShowBrightnessBlueInstruction();
                    break;
            }
        }
    }

    private IEnumerator SwitchToSliderTutorial()
    {
        yield return new WaitForSeconds(1f);

        if (movementTutorialText != null)
            movementTutorialText.SetActive(false);

        if (sliderTutorialText != null)
            sliderTutorialText.SetActive(true);
    }

    private void ShowBrightnessYellowInstruction()
    {
        if (sliderTutorialText != null)
            sliderTutorialText.SetActive(false);

        if (brightnessYellowText != null)
            brightnessYellowText.SetActive(true);
    }

    private void ShowBrightnessBlueInstruction()
    {
        if (brightnessYellowText != null)
            brightnessYellowText.SetActive(false);

        if (brightnessBlueText != null)
            brightnessBlueText.SetActive(true);
    }
}
