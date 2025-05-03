using System;
using UnityEngine;

public class LevelTransitionTrigger : MonoBehaviour
{ 
    private bool triggered = false;
    public GameObject pixelTransitionprefab;
    private void Start()
    { 
        Instantiate(pixelTransitionprefab, gameObject.transform);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            Debug.Log("Triggered");
            triggered = true;
            GameManager.Instance.TransitionToNextScene();
        }
    }
}
