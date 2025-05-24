using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        
        SceneManager.LoadScene("SliderIntroScene");
    }
}
