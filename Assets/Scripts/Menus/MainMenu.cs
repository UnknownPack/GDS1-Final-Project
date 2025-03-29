using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button playButton, exitButton;
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        playButton = uiDocument.rootVisualElement.Q<Button>("start");
        exitButton = uiDocument.rootVisualElement.Q<Button>("exit");
        playButton.clicked += () => SceneManager.LoadScene("Level 1");
        exitButton.clicked += () => Application.Quit();
    }

}
