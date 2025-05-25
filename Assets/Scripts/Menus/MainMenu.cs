using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    private UIDocument uiDocument;
    private AudioSource audioSource;
    private Button playButton, levelSelect, exitButton;
    private VisualElement levelSelector;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        uiDocument = GetComponent<UIDocument>();
        playButton = uiDocument.rootVisualElement.Q<Button>("start");
        exitButton = uiDocument.rootVisualElement.Q<Button>("exit");
        //levelSelect = uiDocument.rootVisualElement.Q<Button>("LevelSelect");
        levelSelector = uiDocument.rootVisualElement.Q<VisualElement>("LevelSelects");
        levelSelector.style.display = DisplayStyle.None;
        /*
        levelSelect.clicked += () =>
        {
            levelSelector.style.display = DisplayStyle.Flex;
        };
        */
        playButton.clicked += () =>
        {
            SceneManager.LoadScene("Plot");
            audioSource.Play();
        };
        exitButton.clicked += () =>
        {
            Application.Quit();
        };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            levelSelector.style.display = DisplayStyle.None;    
        }
    }

}
