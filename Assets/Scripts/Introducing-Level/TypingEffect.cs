using System.Collections;
using TMPro;
using UnityEngine;

public class TypingEffect : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI textComponent;     
    public AudioSource audioSource;           
    public AudioClip typingSound;             

    [Header("Typing Settings")]
    [TextArea]
    public string fullText = "THIS IS A TYPING EFFECT USING TEXTMESH PRO!";
    public float typingSpeed = 0.05f;

    [Header("Cursor Settings")]
    public string cursorChar = "|";            
    public float cursorBlinkInterval = 0.5f;

    private string currentTypedText = "";
    private bool isTyping = false;
    private bool showCursor = true;

    private Coroutine cursorCoroutine;

    void Start()
    {
        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        isTyping = true;
        currentTypedText = "";
        textComponent.text = "";

        if (cursorCoroutine == null)
            cursorCoroutine = StartCoroutine(BlinkCursor());

        foreach (char c in fullText)
        {
            currentTypedText += c;

            if (typingSound && audioSource && c != ' ')
                audioSource.PlayOneShot(typingSound);

            UpdateTextWithCursor();
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        UpdateTextWithCursor(); 
    }

    IEnumerator BlinkCursor()
    {
        while (true)
        {
            if (!isTyping)
            {
                textComponent.text = currentTypedText;
                yield break;
            }

            showCursor = !showCursor;
            UpdateTextWithCursor();
            yield return new WaitForSeconds(cursorBlinkInterval);
        }
    }

    void UpdateTextWithCursor()
    {
        textComponent.text = currentTypedText + (showCursor ? $"<color=#FFFFFF>{cursorChar}</color>" : "");
    }
}
