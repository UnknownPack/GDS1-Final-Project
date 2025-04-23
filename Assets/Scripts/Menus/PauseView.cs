using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseView : MonoBehaviour
{
    UIDocument uiDocument;
    private UnityEngine.InputSystem.PlayerInput playerInput;
    private InputAction pauseAction; 
    private bool currentlyPaused = false;
    private Button resume, reset, settings, mainMenu;
    void Start()
    { 
        uiDocument = GetComponent<UIDocument>();
        resume = uiDocument.rootVisualElement.Q<Button>("Resume");
        resume.clicked += () => 
        {
            Debug.Log("Resume button clicked");
            currentlyPaused = false;
            Time.timeScale = 1f;
            uiDocument.rootVisualElement.style.display = DisplayStyle.None; 
        };

        reset = uiDocument.rootVisualElement.Q<Button>("Restart");
        reset.clicked += () => {
            Debug.Log("Reset button clicked"); 
            GameManager.Instance.RestartLevel(); 
        };
    
        settings = uiDocument.rootVisualElement.Q<Button>("Settings");
        settings.clicked += () => 
        {
            Debug.Log("Settings button clicked"); 
            //TODO: DISSCUSS AND IMPLEMENT SETTINGS WINDOW
        };
        
        mainMenu = uiDocument.rootVisualElement.Q<Button>("Menu");
        mainMenu.clicked += () => 
        {  
            SceneManager.LoadScene("MainMenu");
        }; 
        
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;   
        playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        pauseAction = playerInput.actions["Pause"];
        pauseAction.Enable();
        pauseAction.performed += HandlePause;
    }
    
    private void HandlePause(InputAction.CallbackContext context)
    { 
        currentlyPaused = !currentlyPaused;   
        Time.timeScale = currentlyPaused ? 1f : 0f;
        uiDocument.rootVisualElement.style.display = (currentlyPaused) ? DisplayStyle.Flex: DisplayStyle.None; 
    } 
    
    private void OnDestroy()
    {
        if (pauseAction != null)
            pauseAction.performed -= HandlePause;
    }
}
