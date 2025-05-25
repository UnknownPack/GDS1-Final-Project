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
            Time.timeScale = 1f;
            currentlyPaused = false;
            uiDocument.rootVisualElement.style.display = DisplayStyle.None; 
            GameManager.Instance.RestartLevel(); 
        }; 
        
        mainMenu = uiDocument.rootVisualElement.Q<Button>("Menu");
        mainMenu.clicked += () => 
        {  
            GameManager.Instance.TransitionToDifferentScene("MainMenu"); 
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
        Time.timeScale = currentlyPaused ? 0f : 1f;
        uiDocument.rootVisualElement.style.display = (currentlyPaused) ? DisplayStyle.Flex: DisplayStyle.None; 
    } 
    
    private void OnDestroy()
    {
        if (pauseAction != null)
            pauseAction.performed -= HandlePause;
    }
}
