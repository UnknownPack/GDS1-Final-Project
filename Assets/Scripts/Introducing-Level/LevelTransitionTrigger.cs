using System;
using UnityEngine;

public class LevelTransitionTrigger : MonoBehaviour
{
    public GameObject PixelPrefab ;
    private bool triggered = false;

    private void Start()
    {
        Instantiate(PixelPrefab, transform.position, Quaternion.identity);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            GameManager.Instance.TransitionToNextScene();
        }
    }
}
