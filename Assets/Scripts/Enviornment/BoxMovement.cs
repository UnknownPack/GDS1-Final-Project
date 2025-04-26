using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float travelTime = 1; // Speed at which the box moves
    public float waitTime = 2f;
    public List<Vector2> targetPositions; // List of target positions (coordinates) to move between
    private int currentTargetIndex = 0; // Index to track the current target position
    public GameObject detectorPair;
    private Detector detector;
    private Animator animator;
    void Start()
    { 
        transform.position = targetPositions[1];
        detector = detectorPair.GetComponent<Detector>();
        animator = detector.GetComponent<Animator>();
    }

    public IEnumerator movePlatform()
    {
        if (targetPositions.Count != 2)
        {
            Debug.LogError("Target positions not properly set");
            yield break;
        }
        
        animator.SetBool("IsDown", false);
        transform.position = targetPositions[1]; 
        Vector2 startPos = transform.position;

        float elapsedTime = 0f, duration = travelTime;

        // Move from 0 -> 1
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector2.Lerp(startPos, targetPositions[0], elapsedTime / duration);
            yield return null;
        }
        transform.position = targetPositions[0];

        yield return new WaitForSeconds(waitTime);

        elapsedTime = 0f;
        startPos = transform.position;

        // Move from 1 -> 0
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector2.Lerp(startPos, targetPositions[1], elapsedTime / duration);
            yield return null;
        }
        transform.position = targetPositions[1];

        if (detector == null)
        {
            Debug.LogError("No detector attached");
        }
        else
        {
            detector.isSet = true; 
            animator.SetBool("IsDown", true);
        }  
    }


}
