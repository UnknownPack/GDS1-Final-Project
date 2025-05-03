using UnityEngine;
using UnityEngine.UIElements;

public class DisableUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.SetUIDisplay(DisplayStyle.None);
    }

}
