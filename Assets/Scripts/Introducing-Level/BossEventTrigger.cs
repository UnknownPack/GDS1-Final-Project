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

    [Header("Camera Target")]
    public Transform cameraTarget;

    [Header("Boss Settings")]
    public Transform bossTarget;
    public float bossMoveSpeed = 2f;

    [Header("Timing")]
    public float holdTime = 10f;
    public float panDuration = 1f;
    public float triggerDelay = 1f;

    private Vector3 camStartPos;
    private bool triggered = false;
    private GameObject boss;

    private void Awake()
    {
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        
        // Find the boss GameObject
        boss = GameObject.FindWithTag("Boss");
        if (boss == null)
        {
            Debug.LogWarning("BossEventTrigger: No GameObject with 'Boss' tag found!");
        }
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

        // Start boss movement and camera return simultaneously
        StartCoroutine(MoveBossToTarget());

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
        
        // Wait a bit before destroying to let boss movement complete
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

    private IEnumerator MoveBossToTarget()
    {
        if (boss == null || bossTarget == null)
        {
            Debug.LogWarning("BossEventTrigger: Boss or Boss Target is null, cannot move boss!");
            yield break;
        }

        Vector3 targetPosition = bossTarget.position;
        float stoppingDistance = 0.1f; // How close to consider "reached"

        // Move until we reach the target position
        while (Vector3.Distance(boss.transform.position, targetPosition) > stoppingDistance)
        {
            // Move towards target at constant speed
            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position, 
                targetPosition, 
                bossMoveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // Ensure boss reaches exact target position
        boss.transform.position = targetPosition;
        Debug.Log("Boss has reached target position!");
    }
}
