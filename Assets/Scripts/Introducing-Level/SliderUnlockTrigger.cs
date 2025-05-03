using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SliderUnlockTrigger : MonoBehaviour
{  
    private bool triggered = false;

    private void Start()
    {
        GameManager.Instance.SetUiDispaly(DisplayStyle.None);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || SceneManager.GetActiveScene().name != "SliderIntroScene") return;
        if (other.CompareTag("Player"))
        { 
            triggered = true;  
            GameManager.Instance.SetUiDispaly(DisplayStyle.Flex);
        }
    }
}
