using System.Collections.Generic;
using UnityEngine;

public class BoxMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
     public float moveSpeed = 5f; // Speed at which the box moves
    public List<Vector2> targetPositions; // List of target positions (coordinates) to move between
    private int currentTargetIndex = 0; // Index to track the current target position

    void Start()
    {
        if (targetPositions.Count > 0)
        {
            // Move to the first target position at the start
            transform.position = targetPositions[currentTargetIndex];
        }
    }

    void Update()
    {
        // Check if there are target positions in the list
        if (targetPositions.Count == 0) return;

        // Move the box towards the current target position
        Vector2 target = targetPositions[currentTargetIndex];
        Vector2 currentPosition = transform.position;

        // Move towards the target position
        transform.position = Vector2.MoveTowards(currentPosition, target, moveSpeed * Time.deltaTime);

        // Check if the box has reached the current target position
        if ((Vector2)transform.position == target)
        {
            // Move to the next target position, looping back to the start if necessary
            currentTargetIndex = (currentTargetIndex + 1) % targetPositions.Count;
        }
    }
}
