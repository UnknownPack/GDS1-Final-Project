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
        Debug.Log(other.name + " " + other.tag);
        if (triggered || SceneManager.GetActiveScene().name != "SliderIntroScene") return;
        if (other.CompareTag("Player"))
        {  
            GameManager.Instance.SetUiDispaly(DisplayStyle.Flex);
            triggered = true;   
        }
    }
}
