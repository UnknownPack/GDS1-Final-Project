using System;
using UnityEngine;

public class LevelTransitionTrigger : MonoBehaviour
{ 
    private bool triggered = false; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            GameManager.Instance.TransitionToNextScene();
        }
    }
}
