using System.Collections;
using UnityEngine;

public class TypingSequenceController : MonoBehaviour
{
    public TypingEffect typingEffect;
    [TextArea(3, 10)]
    public string[] dialogueLines;
    public float delayBetweenLines = 2f;

    private int currentLineIndex = 0;

    void Start()
    {
        StartCoroutine(PlayDialogue());
    }

    IEnumerator PlayDialogue()
    {
        while (currentLineIndex < dialogueLines.Length)
        {
            typingEffect.fullText = dialogueLines[currentLineIndex];
            typingEffect.StartTyping();

            yield return new WaitUntil(() => !typingEffect.IsTyping());
            yield return new WaitForSeconds(delayBetweenLines);

            currentLineIndex++;
        }

        TriggerPixelFade();
    }

    void TriggerPixelFade()
    {
        PixelFadeController pixelFade = FindFirstObjectByType<PixelFadeController>();
        if (pixelFade != null)
        {
            Debug.Log("PixelFade Triggered from TypingSequenceController");
            pixelFade.StartFade();
        }
        else
        {
            Debug.LogWarning("PixelFadeController not found in scene.");
        }
    }
}
