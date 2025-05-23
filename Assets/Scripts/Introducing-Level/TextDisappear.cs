using System.Collections;
using UnityEngine;
using TMPro;

public class TextDisappear : MonoBehaviour
{
    [Header("How Long")]
    public float disappearAfterSeconds = 3f;

    private void Start()
    {
        StartCoroutine(HideTextAfterDelay());
    }

    IEnumerator HideTextAfterDelay()
    {
        yield return new WaitForSeconds(disappearAfterSeconds);
        gameObject.SetActive(false); 
    }
}
