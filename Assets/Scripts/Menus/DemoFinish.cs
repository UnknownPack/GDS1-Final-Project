using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class DemoFinish : MonoBehaviour
{
    private UIDocument uiDocument; 
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        uiDocument.rootVisualElement.Q<Button>("menu").clicked += () => SceneManager.LoadScene("MainMenu");
        uiDocument.rootVisualElement.Q<Button>("exit").clicked += () => Application.Quit(); 
    }
}
