using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class TextTriggerManager : MonoBehaviour
{
    [System.Serializable]
    public class TriggerInfo
    {
        [Header("Trigger Collider")]
        public Collider2D triggerCollider;

        [Header("Display Message")]
        [TextArea] public string message = "Default message";

        [Header("Display Duration (secs)")]
        public float duration = 3f;
    }

    [Header("Text Prefab (World-Space Canvas + TextMeshProUGUI)")]
    public GameObject textPrefab;

    [Header("Triggers Configuration")]
    public List<TriggerInfo> triggers = new List<TriggerInfo>();

    private GameObject currentText;
    private Coroutine hideCoroutine;

    private void Start()
    {
        foreach (var info in triggers)
        {
            if (info.triggerCollider != null)
            {
                var proxy = info.triggerCollider.gameObject.AddComponent<ProxyBehaviour>();
                proxy.manager = this;
                proxy.info = info;
            }
        }
    }

    public void OnTriggerActivated(TriggerInfo info)
    {

        if (info.triggerCollider != null)
            info.triggerCollider.enabled = false;

        if (currentText != null)
            Destroy(currentText);

        Vector3 spawnPos = info.triggerCollider.transform.position;
        currentText = Instantiate(textPrefab, spawnPos, Quaternion.identity);

        var typing = currentText.GetComponentInChildren<TypingEffect>();
        if (typing != null)
        {
            typing.fullText = info.message;
            typing.StartTyping();
        }
        else
        {
            var tmp = currentText.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = info.message;
        }

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay(info.duration));
    }

    private IEnumerator HideAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (currentText != null)
            Destroy(currentText);
        currentText = null;
        hideCoroutine = null;
    }

    private class ProxyBehaviour : MonoBehaviour
    {
        [HideInInspector] public TextTriggerManager manager;
        [HideInInspector] public TriggerInfo info;
        private bool hasFired = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasFired) return;

            if (other.CompareTag("Player"))
            {
                hasFired = true;
                manager.OnTriggerActivated(info);

                Destroy(this);
            }
        }
    }
}
