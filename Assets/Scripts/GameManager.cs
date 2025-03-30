using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    
    [Header("Post Processing Sliders")]
    [Tooltip("This list stores the min, max and current values a slider for a post-porcessing effect can have")]
    [SerializeField] private List<HotBarPair> postProcessingSliderValues = new List<HotBarPair>();
    
    [Header("Slider Resource System")]
    [Tooltip("Maximum deviation budget from default values across all sliders")]
    [SerializeField] private float maxTotalDeviationBudget = 1.0f;
    [SerializeField] private float currentDeviationUsed = 0f;
    [Tooltip("Whether to reset deviation budget when loading a new scene")]
    [SerializeField] private bool resetBudgetOnSceneLoad = true;
    
    private Dictionary<PostProcessingEffect, float> CurrentPostProcessingEffectValues;
    private Dictionary<PostProcessingEffect, float> DefaultPostProcessingEffectValues;
    private HotBarPair[] CurrentHotBar = new HotBarPair[3];
    private Slider[] slider = new Slider[3];

    // Resource UI elements
    private ProgressBar resourceBar;
    private Label resourceLabel;

    private PostProccessManager postProccessManager; 
    UIDocument quickAccessDocument;

    [Header("Tutorial UI (Only for Introducing-Level)")]
    public GameObject sliderTutorialText;
    private Coroutine sliderTutorialCoroutine;

    #region Persistant Game Manager Instancing 
    public static GameManager Instance {
         get {
             if (instance == null) {
                 GameObject gameManagerobject = new GameObject("GameManager");
                 instance = gameManagerobject.AddComponent<GameManager>();
                 DontDestroyOnLoad(instance.gameObject);
             }
             return instance;
         }
     }
     private void Awake() {
         if (instance != null && instance != this) {
             Destroy(gameObject);
             return;
         }
         instance = this;
         DontDestroyOnLoad(gameObject);
         SceneManager.sceneLoaded += OnSceneLoaded;
     }
     
     private void OnDestroy()
     {
         // Always unsubscribe to avoid memory leaks
         SceneManager.sceneLoaded -= OnSceneLoaded;
     }
     
     private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
     {
         Debug.Log("Scene loaded: " + scene.name);
         InitalizeDefaultSliderValues();
         
         if (resetBudgetOnSceneLoad)
         {
             ResetDeviationBudget();
         }
         else
         {
             // Recalculate current deviation based on saved values and new defaults
             RecalculateCurrentDeviation();
         }
         
         if (scene.name == "Introducing-Level")
         {
             if (sliderTutorialText != null)
                 sliderTutorialText.SetActive(true); 
         }
         else
         {
             if (sliderTutorialText != null)
                 sliderTutorialText.SetActive(false);
         }
     }
     
     #endregion
 
    void Start()
    { 
        InitalizeDefaultSliderValues();
        postProccessManager = GetComponent<PostProccessManager>();
        
        #region UI Initialization
        quickAccessDocument = GetComponent<UIDocument>();
        slider[0] = quickAccessDocument.rootVisualElement.Q<Slider>("hotkeyOne");
        slider[1] = quickAccessDocument.rootVisualElement.Q<Slider>("hotkeyTwo");
        slider[2] = quickAccessDocument.rootVisualElement.Q<Slider>("hotkeyThree");
        
        // Get resource UI elements (you'll need to add these to your UI)
        resourceBar = quickAccessDocument.rootVisualElement.Q<ProgressBar>("deviationBudgetBar");
        resourceLabel = quickAccessDocument.rootVisualElement.Q<Label>("deviationBudgetLabel");
        #endregion

        // Initialize deviation budget
        ResetDeviationBudget();

        for (int i = 0; i < 3; i++)
        {
            SetSlider(i, slider[i], postProcessingSliderValues[i]);
        }

        slider[0].RegisterValueChangedCallback(OnBrightnessChanged);
        slider[1].RegisterValueChangedCallback(OnAntiAliasingChanged);
        slider[2].RegisterValueChangedCallback(OnMotionBlurChanged);
        
        UpdateResourceUI();
    }

    void SetSlider(int index, Slider slider, HotBarPair hotBarPair)
    {
        CurrentHotBar[index] = hotBarPair; 
        slider.label = hotBarPair.type.ToString();
        slider.value = CurrentPostProcessingEffectValues[hotBarPair.type];
        slider.lowValue = hotBarPair.data.MinValue;
        slider.highValue = hotBarPair.data.MaxValue;
    }

    // Initialize default values at the start of each scene
    void InitalizeDefaultSliderValues()
    {
        CurrentPostProcessingEffectValues = new Dictionary<PostProcessingEffect, float>();
        DefaultPostProcessingEffectValues = new Dictionary<PostProcessingEffect, float>();
        
        foreach (HotBarPair hotBarPair in postProcessingSliderValues)
        {
            float defaultValue = hotBarPair.data.DefaultValue;
            CurrentPostProcessingEffectValues.Add(hotBarPair.type, defaultValue);
            DefaultPostProcessingEffectValues.Add(hotBarPair.type, defaultValue);
        }
    }

    #region Resource System Methods
    
    private void ResetDeviationBudget()
    {
        currentDeviationUsed = 0f;
        UpdateResourceUI();
    }

    private void RecalculateCurrentDeviation()
    {
        currentDeviationUsed = 0f;
        foreach (KeyValuePair<PostProcessingEffect, float> kvp in CurrentPostProcessingEffectValues)
        {
            float defaultValue = DefaultPostProcessingEffectValues[kvp.Key];
            float sliderRange = GetSliderRange(kvp.Key);
            float normalizedDeviation = Mathf.Abs(kvp.Value - defaultValue) / sliderRange;
            currentDeviationUsed += normalizedDeviation;
        }
        
        UpdateResourceUI();
    }
    
    private float GetSliderRange(PostProcessingEffect effect)
    {
        foreach (HotBarPair hotBarPair in postProcessingSliderValues)
        {
            if (hotBarPair.type == effect)
            {
                return hotBarPair.data.MaxValue - hotBarPair.data.MinValue;
            }
        }
        
        return 1.0f; // Default range if not found
    }
    
    private bool TryAdjustSlider(PostProcessingEffect effect, float newValue)
    {
        float defaultValue = DefaultPostProcessingEffectValues[effect];
        float currentValue = CurrentPostProcessingEffectValues[effect];
        float sliderRange = GetSliderRange(effect);
        
        // Calculate the normalized deviation from default (as percentage of total range)
        float currentNormalizedDeviation = Mathf.Abs(currentValue - defaultValue) / sliderRange;
        float newNormalizedDeviation = Mathf.Abs(newValue - defaultValue) / sliderRange;
        
        // Calculate the change in deviation
        float deviationChange = newNormalizedDeviation - currentNormalizedDeviation;
        
        // Special case: if returning closer to default, allow it
        if (deviationChange < 0)
        {
            // Update the current deviation used
            currentDeviationUsed += deviationChange; // This will be negative, reducing the total
            
            // Apply the new value
            CurrentPostProcessingEffectValues[effect] = newValue;
            UpdateResourceUI();
            return true;
        }
        
        // Check if there's enough budget for the adjustment
        if (currentDeviationUsed + deviationChange > maxTotalDeviationBudget)
        {
            Debug.Log($"Not enough deviation budget! Need {deviationChange}, have {maxTotalDeviationBudget - currentDeviationUsed}");
            return false;
        }
        
        // Update the current deviation used
        currentDeviationUsed += deviationChange;
        
        // Apply the new value
        CurrentPostProcessingEffectValues[effect] = newValue;
        UpdateResourceUI();
        
        return true;
    }
    
    private void UpdateResourceUI()
    {
        if (resourceBar != null)
        {
            resourceBar.value = maxTotalDeviationBudget - currentDeviationUsed;
            resourceBar.highValue = maxTotalDeviationBudget;
        }
        
        if (resourceLabel != null)
        {
            resourceLabel.text = $"Adjustment Budget: {(maxTotalDeviationBudget - currentDeviationUsed).ToString("F2")}/{maxTotalDeviationBudget.ToString("F2")}";
        }
        
        // Disable all sliders if budget is fully depleted (except for returning to default)
        bool hasRemainingBudget = currentDeviationUsed < maxTotalDeviationBudget;
        for (int i = 0; i < 3; i++)
        {
            PostProcessingEffect effect = CurrentHotBar[i].type;
            float currentValue = CurrentPostProcessingEffectValues[effect];
            float defaultValue = DefaultPostProcessingEffectValues[effect];
            
            // Enable sliders if there's budget OR if they're not at default (so they can return)
            bool canMove = hasRemainingBudget || !Mathf.Approximately(currentValue, defaultValue);
            slider[i].SetEnabled(canMove);
        }
    }
    
    #endregion

    #region Listener Methods

    private void OnBrightnessChanged(ChangeEvent<float> evt) {
        if (TryAdjustSlider(PostProcessingEffect.Brightness, evt.newValue))
        {
            PostProccessManager.Instance.ChangeBrightness(evt.newValue);
            ResetSliderTutorialTimer();
        }
        else
        {
            // Revert the slider to previous value if not enough budget
            slider[0].SetValueWithoutNotify(CurrentPostProcessingEffectValues[PostProcessingEffect.Brightness]);
        }
    }

    private void OnAntiAliasingChanged(ChangeEvent<float> evt) {
        if (TryAdjustSlider(PostProcessingEffect.AntiAliasing, evt.newValue))
        {
            PostProccessManager.Instance.ChangeAntiAlyasing(evt.newValue);
            ResetSliderTutorialTimer();
        }
        else
        {
            // Revert the slider to previous value if not enough budget
            slider[1].SetValueWithoutNotify(CurrentPostProcessingEffectValues[PostProcessingEffect.AntiAliasing]);
        }
    }

    private void OnMotionBlurChanged(ChangeEvent<float> evt) {
        if (TryAdjustSlider(PostProcessingEffect.MotionBlur, evt.newValue))
        {
            PostProccessManager.Instance.ChangeMotionBlur(evt.newValue);
            ResetSliderTutorialTimer();
        }
        else
        {
            // Revert the slider to previous value if not enough budget
            slider[2].SetValueWithoutNotify(CurrentPostProcessingEffectValues[PostProcessingEffect.MotionBlur]);
        }
    }

    #endregion

    #region Slider Tutorial Timer
    private void ResetSliderTutorialTimer()
    {
        if (SceneManager.GetActiveScene().name != "Introducing-Level")
            return;

        if (sliderTutorialText != null)
        {
            if (sliderTutorialCoroutine != null)
            {
                StopCoroutine(sliderTutorialCoroutine);
            }
            sliderTutorialCoroutine = StartCoroutine(HideSliderTutorialAfterDelay(3f));
        }
    }
    private IEnumerator HideSliderTutorialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(1f);
        if (sliderTutorialText != null)
            sliderTutorialText.SetActive(false);
        sliderTutorialCoroutine = null;
    }
    #endregion

    #region Public Methods

    public float GetPostProcessingValue(PostProcessingEffect postProcessingEffect)
    {
        foreach (HotBarPair hotBarPair in postProcessingSliderValues)
        {
            if(hotBarPair.type == postProcessingEffect)
                return CurrentPostProcessingEffectValues[postProcessingEffect] / hotBarPair.data.MaxValue;
        } 
        Debug.LogError("No Post Processing Effect found: " + postProcessingEffect);
        return 0;
    }
    
    public float GetRemainingDeviationBudget()
    {
        return maxTotalDeviationBudget - currentDeviationUsed;
    }
    
    public float GetDefaultValue(PostProcessingEffect effect)
    {
        return DefaultPostProcessingEffectValues[effect];
    }
    
    public void ResetAllSlidersToDefault()
    {
        for (int i = 0; i < 3; i++)
        {
            PostProcessingEffect effect = CurrentHotBar[i].type;
            float defaultValue = DefaultPostProcessingEffectValues[effect];
            
            // Set value directly to avoid callback loop
            CurrentPostProcessingEffectValues[effect] = defaultValue;
            slider[i].SetValueWithoutNotify(defaultValue);
            
            // Apply the effect
            switch (effect)
            {
                case PostProcessingEffect.Brightness:
                    PostProccessManager.Instance.ChangeBrightness(defaultValue);
                    break;
                case PostProcessingEffect.AntiAliasing:
                    PostProccessManager.Instance.ChangeAntiAlyasing(defaultValue);
                    break;
                case PostProcessingEffect.MotionBlur:
                    PostProccessManager.Instance.ChangeMotionBlur(defaultValue);
                    break;
            }
        }
        
        // Reset the deviation budget
        ResetDeviationBudget();
    }
    #endregion
    
    
    #region Custom Structs
    [System.Serializable] private struct HotBarPair
    {
        public PostProcessingEffect type;
        public PostProcessingEffectData data;
    }
    
    public enum PostProcessingEffect
    {
        Brightness,
        AntiAliasing,
        MotionBlur,
    }

    [System.Serializable]public struct PostProcessingEffectData
    { 
        [SerializeField, Tooltip("minimum value the slider can change for the value.")]
        private float minValue;
        [SerializeField, Tooltip("maximum value the slider can change for the value.")]
        private float maxValue;
        [SerializeField, Tooltip("Default value, the slider and value is set to at the start of a new scene")]
        private float defaultValue;
        
        public float MinValue => minValue;
        public float MaxValue => maxValue;
        public float DefaultValue => defaultValue;
    }
    #endregion

    public void RestartLevel() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}