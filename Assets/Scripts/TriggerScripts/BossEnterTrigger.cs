using System.Collections;
using UnityEngine;

public class BossEnterTrigger : MonoBehaviour
{
    [Header("Boss Movement Settings")]
    [SerializeField] private Transform targetPosition;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool hasTriggered = false;
    
    private GameObject boss;
    private bool isMoving = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find the boss GameObject by tag
        boss = GameObject.FindWithTag("Boss");
        
        if (boss == null)
        {
            Debug.LogWarning("BossEnterTrigger: No GameObject with 'Boss' tag found!");
        }
        
        if (targetPosition == null)
        {
            Debug.LogWarning("BossEnterTrigger: Target position not assigned!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player entered and we haven't triggered yet
        if (other.CompareTag("Player") && !hasTriggered && boss != null && targetPosition != null)
        {
            hasTriggered = true;
            StartCoroutine(MoveBossToTarget());
        }
    }

    private IEnumerator MoveBossToTarget()
    {
        if (isMoving) yield break; // Prevent multiple movements
        
        isMoving = true;
        Vector3 startPosition = boss.transform.position;
        Vector3 endPosition = targetPosition.position;
        
        float distance = Vector3.Distance(startPosition, endPosition);
        float totalTime = distance / moveSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / totalTime;
            
            // Use smooth interpolation
            boss.transform.position = Vector3.Lerp(startPosition, endPosition, progress);
            yield return null;
        }

        // Ensure boss reaches exact target position
        boss.transform.position = endPosition;
        isMoving = false;
        
        Debug.Log("Boss has reached target position!");
    }
}
