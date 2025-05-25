using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TypingSequenceController : MonoBehaviour
{
    [Header("Typing Effect")]
    public TypingEffect typingEffect;

    [Header("Dialogue Lines")]
    [TextArea(3, 10)]
    public string[] dialogueLines;

    [Header("Timing")]
    [Tooltip("Delay before the first line appears")]
    public float startDelay = 1f;
    [Tooltip("Delay between each completed line")]
    public float delayBetweenLines = 2f;

    [Header("Scene Transition")]
    [Tooltip("Check to disable the automatic scene load at the end")]
    public bool disableSceneTransition = false;

    private int currentLineIndex = 0;

    void Start()
    {
        StartCoroutine(PlayDialogue());
    }

    private IEnumerator PlayDialogue()
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        while (currentLineIndex < dialogueLines.Length)
        {
            typingEffect.fullText = dialogueLines[currentLineIndex];
            typingEffect.StartTyping();

            yield return new WaitUntil(() => !typingEffect.IsTyping());

            if (delayBetweenLines > 0f)
                yield return new WaitForSeconds(delayBetweenLines);

            currentLineIndex++;
        }

        if (!disableSceneTransition)
        {
            SceneManager.LoadScene("SliderIntroScene");
        }
    }
}
