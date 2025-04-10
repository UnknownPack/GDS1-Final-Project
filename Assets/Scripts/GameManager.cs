using System.Collections;
using System.Collections.Generic;
using SimplePieMenu;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    
    [Header("Post Processing Sliders")]
    [Tooltip("This list stores the min, max and current values a slider for a post-porcessing effect can have")]
    [SerializeField]private List<HotBarPair> postProcessingSliderValues = new List<HotBarPair>();

    private int currentLevel = 0;
    [SerializeField] private List<HotBarPair> postProcessingSliderValues = new List<HotBarPair>();
    
    [Header("Slider Resource System")]
    [Tooltip("Maximum deviation budget from default values across all sliders")]
    [SerializeField] private float maxTotalDeviationBudget = 1.0f;
    [SerializeField] private float currentDeviationUsed = 0f;
    [Tooltip("Whether to reset deviation budget when loading a new scene")]
    [SerializeField] private bool resetBudgetOnSceneLoad = true;
    
    private Dictionary<PostProcessingEffect, float> CurrentPostProcessingEffectValues;
    private Dictionary<PostProcessingEffect, float> DefaultPostProcessingEffectValues;
    private HotBarPair[] CurrentHotBar = new HotBarPair[2];
    private Slider[] slider = new Slider[2];
    private int selectedSliderIndex = 0;

    // Resource UI elements
    private ProgressBar resourceBar;
    private Label resourceLabel;

    private PostProccessManager postProccessManager; 
    UIDocument quickAccessDocument;

    [Header("Tutorial UI (Only for Introducing-Level)")]
    public GameObject sliderTutorialText;
    private Coroutine sliderTutorialCoroutine;

    private EventCallback<ChangeEvent<float>>[] sliderCallbacks = new EventCallback<ChangeEvent<float>>[2];


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
         currentLevel = SceneManager.GetActiveScene().buildIndex;
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

         InitializeUIElements();
         
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
        InitializeSystem();
        postProccessManager = GetComponent<PostProccessManager>();
        InitializeUIElements();
        ResetDeviationBudget();
    }

    private void Update() {
        HandleHotbarInput();
    }

    
    private void InitializeSystem() {
        CurrentPostProcessingEffectValues = new Dictionary<PostProcessingEffect, float>();
        DefaultPostProcessingEffectValues = new Dictionary<PostProcessingEffect, float>();

        foreach (HotBarPair pair in postProcessingSliderValues) {
            CurrentPostProcessingEffectValues.Add(pair.type, pair.data.DefaultValue);
            DefaultPostProcessingEffectValues.Add(pair.type, pair.data.DefaultValue);
        }
    }

    private void InitializeUIElements() {
        quickAccessDocument = GetComponent<UIDocument>();
        
        slider[0] = quickAccessDocument.rootVisualElement.Q<Slider>("hotkeyOne");
        slider[1] = quickAccessDocument.rootVisualElement.Q<Slider>("hotkeyTwo");

        resourceBar = quickAccessDocument.rootVisualElement.Q<ProgressBar>("deviationBudgetBar");
        resourceLabel = quickAccessDocument.rootVisualElement.Q<Label>("deviationBudgetLabel");

            if (slider[0] == null || slider[1] == null || resourceBar == null || resourceLabel == null) {
        Debug.LogError("Failed to find required UI elements!");
        return;
    }

        for (int i = 0; i < 2; i++) {
            SetupSlider(i, postProcessingSliderValues[i]);
        }
        resourceBar.lowValue = 0;
        resourceBar.highValue = maxTotalDeviationBudget;
    }

    private void SetupSlider(int index, HotBarPair pair) {
        CurrentHotBar[index] = pair;
        slider[index].label = pair.type.ToString();
        slider[index].value = pair.data.DefaultValue;
        slider[index].lowValue = pair.data.MinValue;
        slider[index].highValue = pair.data.MaxValue;
        
        // Store the effect type locally to capture the correct value
        PostProcessingEffect effectType = pair.type;
        
        // Clean up existing callback first
        if (sliderCallbacks[index] != null) {
            slider[index].UnregisterValueChangedCallback(sliderCallbacks[index]);
        }
        
        // Create new callback with captured effectType
        sliderCallbacks[index] = evt => OnSliderChanged(evt, effectType);
        slider[index].RegisterValueChangedCallback(sliderCallbacks[index]);
    }

    private void HandleHotbarInput() {
        HandleSelectionInput();
        HandleAdjustmentInput();
    }

    private void HandleSelectionInput() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlider(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlider(1);
    }

    private void HandleAdjustmentInput() {
        if (CurrentHotBar.Length == 0 || selectedSliderIndex < 0 || selectedSliderIndex >= CurrentHotBar.Length) 
            return;

        HotBarPair currentPair = CurrentHotBar[selectedSliderIndex];
        PostProcessingEffect currentEffect = currentPair.type;
        float currentValue = CurrentPostProcessingEffectValues[currentEffect];
        float min = currentPair.data.MinValue;
        float max = currentPair.data.MaxValue;
        float def = currentPair.data.DefaultValue;
        float epsilon = 0.0001f; // Small value for float comparisons

        // Gradual adjustments
        if (Input.GetKey(KeyCode.W)) {
            float newValue = Mathf.Min(currentValue + (max - min) * 0.01f, max);
            UpdateSliderAndEffects(currentEffect, newValue);
        }
        if (Input.GetKey(KeyCode.S)) {
            float newValue = Mathf.Max(currentValue - (max - min) * 0.01f, min);
            UpdateSliderAndEffects(currentEffect, newValue);
        }

        // Q - moves down through states
        if (Input.GetKeyDown(KeyCode.Q)) {
            float newValue;
            if (Mathf.Abs(currentValue - max) < epsilon) {
                newValue = def; // Max → Default
            }
            else if (Mathf.Abs(currentValue - def) < epsilon && def > min + epsilon) {
                newValue = min; // Default → Min (only if default > min)
            }
            else if (currentValue > def + epsilon) {
                newValue = def; // Above default → Default
            }
            else {
                newValue = min; // Everything else → Min
            }
            UpdateSliderAndEffects(currentEffect, newValue);
        }

        // E - moves up through states
        if (Input.GetKeyDown(KeyCode.E)) {
            float newValue;
            if (Mathf.Abs(currentValue - min) < epsilon && def > min + epsilon) {
                newValue = def; // Min → Default (only if default > min)
            }
            else if (Mathf.Abs(currentValue - def) < epsilon) {
                newValue = max; // Default → Max
            }
            else if (currentValue < def - epsilon) {
                newValue = def; // Below default → Default
            }
            else {
                newValue = max; // Everything else → Max
            }
            UpdateSliderAndEffects(currentEffect, newValue);
        }
    }

    private void UpdateSliderAndEffects(PostProcessingEffect effect, float newValue) {
    // Clamp the value first
    HotBarPair pair = GetPairForEffect(effect);
    newValue = Mathf.Clamp(newValue, pair.data.MinValue, pair.data.MaxValue);

    // Try to adjust (checks deviation budget)
    if (TryAdjustSlider(effect, newValue)) {
        // Update UI slider
        int sliderIndex = GetSliderIndexForEffect(effect);
        if (sliderIndex >= 0 && sliderIndex < slider.Length) {
            slider[sliderIndex].value = newValue;
        }
    }
}

    private HotBarPair GetPairForEffect(PostProcessingEffect effect) {
        foreach (var pair in CurrentHotBar) {
            if (pair.type == effect) {
                return pair;
            }
        }
        return default;
    }

    private int GetSliderIndexForEffect(PostProcessingEffect effect) {
        for (int i = 0; i < CurrentHotBar.Length; i++) {
            if (CurrentHotBar[i].type == effect) {
                return i;
            }
        }
        return -1;
    }

    private void AdjustSliderValue(float newValue) {
        HotBarPair pair = CurrentHotBar[selectedSliderIndex];
        newValue = Mathf.Clamp(newValue, pair.data.MinValue, pair.data.MaxValue);
        
        if (TryAdjustSlider(pair.type, newValue)) {
            UpdateSliderVisuals();
        }
    }

    private void SetSliderToMin() {
        HotBarPair pair = CurrentHotBar[selectedSliderIndex];
        AdjustSliderValue(pair.data.MinValue);
    }

    private void SetSliderToMax() {
        HotBarPair pair = CurrentHotBar[selectedSliderIndex];
        AdjustSliderValue(pair.data.MaxValue);
    }

    private void SelectSlider(int index) {
        selectedSliderIndex = Mathf.Clamp(index, 0, CurrentHotBar.Length - 1);
        UpdateSliderVisuals();
    }

    private void UpdateSliderVisuals() {
        for (int i = 0; i < slider.Length; i++) {
            slider[i].style.backgroundColor = new StyleColor(
                i == selectedSliderIndex ? Color.green : Color.clear
            );
        }
    }

    private bool TryAdjustSlider(PostProcessingEffect effect, float newValue) {
        float defaultValue = DefaultPostProcessingEffectValues[effect];
        float currentValue = CurrentPostProcessingEffectValues[effect];
        float sliderRange = GetSliderRange(effect);

        float currentDeviation = Mathf.Abs(currentValue - defaultValue) / sliderRange;
        float newDeviation = Mathf.Abs(newValue - defaultValue) / sliderRange;
        float deviationDelta = newDeviation - currentDeviation;

        if (deviationDelta > 0 && currentDeviationUsed + deviationDelta > maxTotalDeviationBudget) {
            Debug.Log("Deviation budget exceeded");
            return false;
        }

        currentDeviationUsed += deviationDelta;
        CurrentPostProcessingEffectValues[effect] = newValue;
        UpdateResourceUI();
        return true;
    }

    private float GetSliderRange(PostProcessingEffect effect) {
        foreach (HotBarPair pair in postProcessingSliderValues) {
            if (pair.type == effect) {
                return pair.data.MaxValue - pair.data.MinValue;
            }
        }
        return 1f;
    }

    public void SetSelectedSlider(int index) {
        SelectSlider(index);
    }

    private void UpdateResourceUI() {
        resourceBar.value = maxTotalDeviationBudget - currentDeviationUsed;
        resourceLabel.text = $"Resources: {(maxTotalDeviationBudget - currentDeviationUsed) * 100:F2}/{maxTotalDeviationBudget * 100:F2}";
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
            CurrentPostProcessingEffectValues.Add(hotBarPair.type, hotBarPair.data.DefaultValue);
        }  
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

    private void RecalculateCurrentDeviation() {
        currentDeviationUsed = 0f;
        // Only consider effects in the current hotbar
        foreach (HotBarPair pair in CurrentHotBar) {
            PostProcessingEffect effect = pair.type;
            float defaultValue = DefaultPostProcessingEffectValues[effect];
            float currentValue = CurrentPostProcessingEffectValues[effect];
            float sliderRange = GetSliderRange(effect);
            float normalizedDeviation = Mathf.Abs(currentValue - defaultValue) / sliderRange;
            currentDeviationUsed += normalizedDeviation;
        }
        UpdateResourceUI();
    }
    
    #endregion

    #region Listener Methods

    private void OnSliderChanged(ChangeEvent<float> evt, PostProcessingEffect effect)
{
    if (TryAdjustSlider(effect, evt.newValue))
    {
        switch (effect)
        {
            case PostProcessingEffect.Brightness:
                PostProccessManager.Instance.ChangeBrightness(evt.newValue);
                break;
            case PostProcessingEffect.AntiAliasing:
                PostProccessManager.Instance.ChangeAntiAlyasing(evt.newValue);
                break;
            case PostProcessingEffect.MotionBlur:
                PostProccessManager.Instance.ChangeMotionBlur(evt.newValue);
                break;
            case PostProcessingEffect.FilmGrain:
                PostProccessManager.Instance.ChangeFilmGrain(evt.newValue);
                break;
            case PostProcessingEffect.ColorCorrection:
                PostProccessManager.Instance.ChangeColorCorrection(evt.newValue);
                break;
            case PostProcessingEffect.ChromaticAberration:
                PostProccessManager.Instance.ChangeChromaticAberration(evt.newValue);
                break;
            case PostProcessingEffect.Bloom:
                PostProccessManager.Instance.ChangeBloom(evt.newValue);
                break;
        }

        ResetSliderTutorialTimer();
    }
    else
    {
        // Revert the slider to previous value if not enough budget
        slider[0].SetValueWithoutNotify(CurrentPostProcessingEffectValues[effect]);
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
    
    public void RestartLevel() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public int GetCurrentLevel() { return currentLevel; }

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
        for (int i = 0; i < 7; i++)
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

    public void ReplaceSlider(PostProcessingEffect newEffect)
    {
        int slotIndex = selectedSliderIndex;
        // Remove this line as it overrides the passed slotIndex with selectedSliderIndex
        // slotIndex = selectedSliderIndex;
        
        if (slotIndex < 0 || slotIndex >= CurrentHotBar.Length)
        {
            Debug.LogError($"Invalid slot index: {slotIndex}");
            return;
        }

        // Get the current effect before replacement
        PostProcessingEffect oldEffect = CurrentHotBar[slotIndex].type;

        UpdateSliderAndEffects(oldEffect, GetDefaultValue(oldEffect));
        
        // If replacing with same effect, do nothing
        if (oldEffect == newEffect) return;

        // Find the new effect's data
        HotBarPair newPair = postProcessingSliderValues.Find(pair => pair.type == newEffect);
        if (newPair.Equals(default(HotBarPair)))
        {
            Debug.LogError($"Effect {newEffect} not found in postProcessingSliderValues");
            return;
        }

        // Calculate deviation adjustment:
        // 1. Remove deviation from old effect
        float oldDefault = DefaultPostProcessingEffectValues[oldEffect];
        float oldValue = CurrentPostProcessingEffectValues[oldEffect];
        float oldDeviation = Mathf.Abs(oldValue - oldDefault) / 
                            (CurrentHotBar[slotIndex].data.MaxValue - CurrentHotBar[slotIndex].data.MinValue);
        currentDeviationUsed -= oldDeviation;

        // 2. Reset old effect to default in dictionary
        CurrentPostProcessingEffectValues[oldEffect] = oldDefault;

        // 3. Set new effect to its default value
        float newDefault = newPair.data.DefaultValue;
        CurrentPostProcessingEffectValues[newEffect] = newDefault;
        
        // 4. Calculate new effect's deviation (will be 0 since we're setting to default)
        float newDeviation = 0f;
        currentDeviationUsed += newDeviation;
        // Unregister old callback
        if (sliderCallbacks[slotIndex] != null)
        {
            slider[slotIndex].UnregisterValueChangedCallback(sliderCallbacks[slotIndex]);
        }

        // Update the hotbar slot
        CurrentHotBar[slotIndex] = newPair;

        // Update UI slider
        slider[slotIndex].label = newEffect.ToString();
        slider[slotIndex].lowValue = newPair.data.MinValue;
        slider[slotIndex].highValue = newPair.data.MaxValue;
        slider[slotIndex].value = newDefault;

        // Register new callback
        sliderCallbacks[slotIndex] = evt => OnSliderChanged(evt, newEffect);
        slider[slotIndex].RegisterValueChangedCallback(sliderCallbacks[slotIndex]);

        // Update the UI (no need for full recalculate since we've manually adjusted deviation)
        UpdateResourceUI();
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
        FilmGrain,
        ColorCorrection,
        ChromaticAberration,
        Bloom
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
