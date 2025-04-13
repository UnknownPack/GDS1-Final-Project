using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseView : MonoBehaviour
{
    [SerializeField]VisualTreeAsset GameUI;
    [SerializeField]VisualTreeAsset PauseMenuUI; 
    UIDocument uiDocument;
    
    private UnityEngine.InputSystem.PlayerInput playerInput;
    private InputAction pauseAction;
    private GameObject pauseMenuObject;
    private bool currentlyPaused = false;
    void Start()
    { 
        #region UI Initalization
        uiDocument = GetComponent<UIDocument>();
        
        pauseMenuObject = new GameObject("PauseMenuView");
        DontDestroyOnLoad(pauseMenuObject);
        pauseMenuObject.AddComponent<UIDocument>();
        UIDocument pauseUI = pauseMenuObject.GetComponent<UIDocument>();
        pauseUI.panelSettings = uiDocument.panelSettings;
        pauseUI.visualTreeAsset = PauseMenuUI;
        pauseUI.sortingOrder = 100;
        pauseUI.rootVisualElement.pickingMode = PickingMode.Position; 
        pauseUI.rootVisualElement.Q<Button>("Resume").RegisterCallback<ClickEvent>(evt => 
        {
            Debug.Log("Resume button clicked");
            currentlyPaused = false;
            Time.timeScale = 1f;
            uiDocument.enabled = true;
            pauseMenuObject.SetActive(false);
        });
        
        pauseUI.rootVisualElement.Q<Button>("Reset").RegisterCallback<ClickEvent>(evt => {
            Debug.Log("Reset button clicked"); 
            currentlyPaused = false;
            Time.timeScale = 1f;
            uiDocument.enabled = true;
            pauseMenuObject.SetActive(false);
            GameManager.Instance.RestartLevel(); 
        });
    
        pauseUI.rootVisualElement.Q<Button>("Settings").RegisterCallback<ClickEvent>(evt => 
        {
            Debug.Log("Settings button clicked"); 
            //TODO: DISSCUSS AND IMPLEMENT SETTINGS WINDOW
        });
        pauseUI.rootVisualElement.Q<Button>("Menu").RegisterCallback<ClickEvent>(evt => 
        { 
            Debug.Log("Menu button clicked"); 
            currentlyPaused = false;
            Time.timeScale = 1f;
            uiDocument.enabled = true;
            pauseMenuObject.SetActive(false);
            SceneManager.LoadScene("MainMenu");
        }); 
        
        pauseMenuObject.SetActive(false);
        #endregion  
        playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        pauseAction = playerInput.actions["Pause"];
        pauseAction.Enable();
        pauseAction.performed += OnPause;
    }
    
    private void OnPause(InputAction.CallbackContext context)
    {
        Debug.Log("PauseButtonPressed");
        currentlyPaused = !currentlyPaused; 
        uiDocument.enabled = !currentlyPaused;
        Time.timeScale = (currentlyPaused) ? 0.0f : 1.0f;
        pauseMenuObject.SetActive(currentlyPaused);
    } 
    
    private void OnDestroy()
    {
        if (pauseAction != null)
            pauseAction.performed -= OnPause;
    }
}
