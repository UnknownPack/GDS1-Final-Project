using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TypingEffect : MonoBehaviour
{
    [TextArea]
    public string fullText = "Default typing text...";
    public float typingSpeed = 0.05f;
    public bool playOnStart = true;
    public bool IsTyping()
    {
        return isTyping;
    }


    private TextMeshProUGUI textComponent;
    private string currentTypedText = "";
    private Coroutine cursorCoroutine;
    private bool showCursor = true;
    private bool isTyping = false;

    [Header("Cursor Settings")]
    public string cursorChar = "|";
    public float cursorBlinkInterval = 0.5f;

    [Header("Audio")]
    public AudioClip typingSound;
    private AudioSource audioSource;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();

        if (typingSound)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = typingSound;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        if (playOnStart)
        {
            StartTyping();
        }
    }

    public void StartTyping()
    {
        if (isTyping) return;
        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        isTyping = true;
        currentTypedText = "";
        textComponent.text = "";

        if (audioSource && typingSound)
        {
            audioSource.Play();
        }

        if (cursorCoroutine == null)
            cursorCoroutine = StartCoroutine(BlinkCursor());

        foreach (char c in fullText)
        {
            currentTypedText += c;
            UpdateTextWithCursor();
            yield return new WaitForSeconds(typingSpeed);
        }

        if (audioSource && audioSource.isPlaying)
        {
            audioSource.Stop();
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
