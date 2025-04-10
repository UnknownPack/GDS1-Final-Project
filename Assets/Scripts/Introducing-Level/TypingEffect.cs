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

    private TextMeshProUGUI textComponent;
    private string currentTypedText = "";
    private Coroutine cursorCoroutine;
    private bool showCursor = true;
    private bool isTyping = false;

    [Header("Cursor Settings")]
    public string cursorChar = "|";
    public float cursorBlinkInterval = 0.5f;

    [Header("Audio")]
    public AudioClip typingSound; // 预先指定的打字声音
    private AudioSource audioSource;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();

        // 自动添加 AudioSource 组件并设置基础参数
        if (typingSound)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = typingSound;
            audioSource.loop = true; // 设置为循环播放
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

        // 开始播放连续打字声音
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

        // 文字完全显示后，停止打字声音
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
