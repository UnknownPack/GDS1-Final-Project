using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BossEventTrigger : MonoBehaviour
{
    [Header("Player Controller")]
    public PlayerController playerController;
    public Rigidbody2D playerRb;

    [Header("Camera Follow Script")]
    public CameraFollow cameraFollow;

    [Header("Target")]
    public Transform cameraTarget;

    [Header("HoldTime")]
    public float holdTime = 10f;

    [Header("Duration")]
    public float panDuration = 1f;

    [Header("Delay")]
    public float triggerDelay = 1f;

    private Vector3 camStartPos;
    private bool triggered = false;

    private void Awake()
    {
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;

        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(triggerDelay);

        camStartPos = playerController.transform.position
                    + cameraFollow.offset;

        cameraFollow.enabled = false;
        playerController.enabled = false;
        playerRb.linearVelocity = Vector2.zero;
        playerRb.constraints = RigidbodyConstraints2D.FreezeAll;

        StartCoroutine(PlayBossSequence());
    }

    private IEnumerator PlayBossSequence()
    {
        Vector3 camObservePos = new Vector3(
            cameraTarget.position.x,
            cameraTarget.position.y,
            camStartPos.z
        );

        float t = 0f;
        while (t < panDuration)
        {
            t += Time.deltaTime;
            float frac = t / panDuration;
            Camera.main.transform.position =
                Vector3.Lerp(camStartPos, camObservePos, frac);
            yield return null;
        }

        yield return new WaitForSeconds(holdTime);

        t = 0f;
        Vector3 returnStart = Camera.main.transform.position;
        Vector3 returnEnd = camStartPos;
        while (t < panDuration)
        {
            t += Time.deltaTime;
            float frac = t / panDuration;
            Camera.main.transform.position =
                Vector3.Lerp(returnStart, returnEnd, frac);
            yield return null;
        }

        playerRb.constraints = RigidbodyConstraints2D.None;
        playerController.enabled = true;
        cameraFollow.enabled = true;
        Destroy(gameObject);
    }
}
