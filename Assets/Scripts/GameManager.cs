using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using SimplePieMenu;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    
    [Header("Post Processing Sliders")]
    [Tooltip("Stores min, max and current values for post-processing sliders")]
    [SerializeField] private List<HotBarPair> postProcessingSliderValues = new List<HotBarPair>();
    
    [Header("Resource System")]
    [Tooltip("Maximum total deviation budget from default values")]
    [SerializeField] private float maxTotalDeviationBudget = 1.0f;
    [SerializeField] private float currentDeviationUsed = 0f;
    [Tooltip("Reset budget when loading new scene")]
    [SerializeField] private bool resetBudgetOnSceneLoad = true;

    [Header("Tutorial UI (Introducing-Level Only)")]
    public GameObject sliderTutorialText;

    // UI Elements
    private UIDocument quickAccessDocument;
    private ProgressBar resourceBar;
    private Label resourceLabel;
    private Slider[] sliders = new Slider[2];
    
    // State management
    private int selectedSliderIndex = 0;
    private HotBarPair[] currentHotBar = new HotBarPair[2];
    private Coroutine sliderTutorialCoroutine;
    private EventCallback<ChangeEvent<float>>[] sliderCallbacks = new EventCallback<ChangeEvent<float>>[2];
    
    // Post-processing management
    private PostProccessManager postProcessManager;
    private Dictionary<PostProcessingEffect, float> currentEffectValues;
    private Dictionary<PostProcessingEffect, float> defaultEffectValues;
    private Coroutine transitionCoroutine;

    #region Singleton Pattern
    public static GameManager Instance {
        get {
            if (instance == null) {
                GameObject gmObject = new GameObject("GameManager");
                instance = gmObject.AddComponent<GameManager>();
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

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion

    #region Scene Management
    public enum SliderType { Brightness, MotionBlur, FilmGrain }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        Debug.Log($"Scene loaded: {scene.name}");
        InitializeSystem();
        UpdateUIReferences();
        
        if (resetBudgetOnSceneLoad) ResetDeviationBudget();
        else RecalculateCurrentDeviation();
        
        HandleTutorialUI(scene.name);
    }
    public void EnableSliderManually(GameObject sliderObject, SliderType sliderType)
    {
        if (SceneManager.GetActiveScene().name != "SliderIntroScene")
            return;

        sliderObject.SetActive(true);

    }
    #endregion

    #region Initialization
    void Start() {
        InitializeSystem();
        postProcessManager = GetComponent<PostProccessManager>();
        InitializeUIElements();
        ResetDeviationBudget();
    }

    private void InitializeSystem() {
        currentEffectValues = new Dictionary<PostProcessingEffect, float>();
        defaultEffectValues = new Dictionary<PostProcessingEffect, float>();

        foreach (HotBarPair pair in postProcessingSliderValues) {
            currentEffectValues.Add(pair.type, pair.data.DefaultValue);
            defaultEffectValues.Add(pair.type, pair.data.DefaultValue);
        }
    }

    private void InitializeUIElements() {
        quickAccessDocument = GetComponent<UIDocument>();
        GetUIReferences();
        ValidateUIElements();

        SetupSlider(0, postProcessingSliderValues[0]);
        SetupSlider(1, postProcessingSliderValues[1]);
        resourceBar.lowValue = 0;
        resourceBar.highValue = maxTotalDeviationBudget;
    }

    private void UpdateUIReferences() {
        quickAccessDocument = GetComponent<UIDocument>();
        GetUIReferences();
        ValidateUIElements();
        
        SetupSlider(0, currentHotBar[0]);
        SetupSlider(1, currentHotBar[1]);
        resourceBar.highValue = maxTotalDeviationBudget;
    }

    private void GetUIReferences() {
        sliders[0] = quickAccessDocument.rootVisualElement.Q<Slider>("hotkeyOne");
        sliders[1] = quickAccessDocument.rootVisualElement.Q<Slider>("hotkeyTwo");
        resourceBar = quickAccessDocument.rootVisualElement.Q<ProgressBar>("deviationBudgetBar");
        resourceLabel = quickAccessDocument.rootVisualElement.Q<Label>("deviationBudgetLabel");
    }

    private void ValidateUIElements() {
        if (sliders[0] == null || sliders[1] == null || resourceBar == null || resourceLabel == null) {
            Debug.LogError("Missing UI elements!");
            enabled = false;
        }
    }
    #endregion

    #region Slider Management
    private void SetupSlider(int index, HotBarPair pair) {
        currentHotBar[index] = pair;
        var slider = sliders[index];
        
        slider.label = pair.type.ToString();
        slider.value = pair.data.DefaultValue;
        slider.lowValue = pair.data.MinValue;
        slider.highValue = pair.data.MaxValue;

        // Clean up existing callback
        if (sliderCallbacks[index] != null) {
            slider.UnregisterValueChangedCallback(sliderCallbacks[index]);
        }

        // Create new callback
        PostProcessingEffect effect = pair.type;
        sliderCallbacks[index] = evt => OnSliderChanged(evt, effect);
        slider.RegisterValueChangedCallback(sliderCallbacks[index]);
    }

    private void OnSliderChanged(ChangeEvent<float> evt, PostProcessingEffect effect) {
        if (TryAdjustSlider(effect, evt.newValue)) {
            ApplyPostProcessingEffect(effect, evt.newValue);
            ResetSliderTutorialTimer();
        }
        else {
            sliders[0].SetValueWithoutNotify(currentEffectValues[effect]);
        }
    }

    private void ApplyPostProcessingEffect(PostProcessingEffect effect, float value) {
        switch (effect) {
            case PostProcessingEffect.Brightness:
                postProcessManager.ChangeBrightness(value);
                break;
            case PostProcessingEffect.AntiAliasing:
                postProcessManager.ChangeAntiAlyasing(value);
                break;
            case PostProcessingEffect.MotionBlur:
                postProcessManager.ChangeMotionBlur(value);
                break;
            case PostProcessingEffect.FilmGrain:
                postProcessManager.ChangeFilmGrain(value);
                break;
            case PostProcessingEffect.ColorCorrection:
                postProcessManager.ChangeColorCorrection(value);
                break;
            case PostProcessingEffect.ChromaticAberration:
                postProcessManager.ChangeChromaticAberration(value);
                break;
            case PostProcessingEffect.Bloom:
                postProcessManager.ChangeBloom(value);
                break;
            case PostProcessingEffect.Empty:
                break;
        }
    }
    #endregion

    #region Input Handling
    private void Update() => HandleHotbarInput();

    private void HandleHotbarInput() {
        HandleSelectionInput();
        HandleAdjustmentInput();
    }

    private void HandleSelectionInput() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlider(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlider(1);
    }

    private void HandleAdjustmentInput() {
        if (currentHotBar.Length == 0 || selectedSliderIndex < 0 || selectedSliderIndex >= currentHotBar.Length) 
            return;

        HotBarPair pair = currentHotBar[selectedSliderIndex];
        PostProcessingEffect effect = pair.type;
        float currentValue = currentEffectValues[effect];
        float min = pair.data.MinValue;
        float max = pair.data.MaxValue;

        if (Input.GetKey(KeyCode.W)) AdjustValue(effect, currentValue + (max - min) * 0.01f);
        if (Input.GetKey(KeyCode.S)) AdjustValue(effect, currentValue - (max - min) * 0.01f);
        if (Input.GetKeyDown(KeyCode.Q)) CycleSliderState(effect, false);
        if (Input.GetKeyDown(KeyCode.E)) CycleSliderState(effect, true);
    }

    private void AdjustValue(PostProcessingEffect effect, float newValue) {
        UpdateSliderAndEffects(effect, newValue);
    }

    private void CycleSliderState(PostProcessingEffect effect, bool increase) {
        const float epsilon = 0.0001f;
        float currentValue = currentEffectValues[effect];
        float defaultValue = defaultEffectValues[effect];
        float min = GetPairForEffect(effect).data.MinValue;
        float max = GetPairForEffect(effect).data.MaxValue;

        float newValue = increase ? 
            CycleUp(currentValue, defaultValue, min, max, epsilon) :
            CycleDown(currentValue, defaultValue, min, max, epsilon);

        UpdateSliderAndEffects(effect, newValue);
    }

    private float CycleUp(float current, float def, float min, float max, float epsilon) {
        if (Mathf.Abs(current - min) < epsilon && def > min + epsilon) return def;
        if (Mathf.Abs(current - def) < epsilon) return max;
        if (current < def - epsilon) return def;
        return max;
    }

    private float CycleDown(float current, float def, float min, float max, float epsilon) {
        if (Mathf.Abs(current - max) < epsilon) return def;
        if (Mathf.Abs(current - def) < epsilon && def > min + epsilon) return min;
        if (current > def + epsilon) return def;
        return min;
    }
    #endregion

    #region Resource System
    private bool TryAdjustSlider(PostProcessingEffect effect, float newValue) {
        HotBarPair pair = GetPairForEffect(effect);
        newValue = Mathf.Clamp(newValue, pair.data.MinValue, pair.data.MaxValue);

        float currentValue = currentEffectValues[effect];
        float sliderRange = pair.data.MaxValue - pair.data.MinValue;
        float currentDeviation = Mathf.Abs(currentValue - pair.data.DefaultValue) / sliderRange;
        float newDeviation = Mathf.Abs(newValue - pair.data.DefaultValue) / sliderRange;
        float deviationDelta = newDeviation - currentDeviation;

        if (deviationDelta > 0 && currentDeviationUsed + deviationDelta > maxTotalDeviationBudget) {
            Debug.Log("Deviation budget exceeded");
            return false;
        }

        currentDeviationUsed += deviationDelta;
        currentEffectValues[effect] = newValue;
        UpdateResourceUI();
        return true;
    }

    private void UpdateResourceUI() {
        resourceBar.value = maxTotalDeviationBudget - currentDeviationUsed;
        resourceLabel.text = $"Resources: {(maxTotalDeviationBudget - currentDeviationUsed) * 100:F2}/" +
                            $"{maxTotalDeviationBudget * 100:F2}";
    }

    private void ResetDeviationBudget() {
        currentDeviationUsed = 0f;
        UpdateResourceUI();
    }

    private void RecalculateCurrentDeviation() {
        currentDeviationUsed = 0f;
        foreach (HotBarPair pair in currentHotBar) {
            PostProcessingEffect effect = pair.type;
            float currentValue = currentEffectValues[effect];
            float defaultValue = defaultEffectValues[effect];
            float sliderRange = pair.data.MaxValue - pair.data.MinValue;
            currentDeviationUsed += Mathf.Abs(currentValue - defaultValue) / sliderRange;
        }
        UpdateResourceUI();
    }
    #endregion

    #region Public Methods
    public void RestartLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    
    public float GetPostProcessingValue(PostProcessingEffect effect) {
        foreach (HotBarPair pair in currentHotBar) {
            if (pair.type == effect)
                return currentEffectValues[effect] / pair.data.MaxValue;
        }
        Debug.LogError($"Effect not found: {effect}");
        return 0;
    }

    public float GetRemainingDeviationBudget() => maxTotalDeviationBudget - currentDeviationUsed;
    public float GetDefaultValue(PostProcessingEffect effect) => defaultEffectValues[effect];

    public void ReplaceSlider(PostProcessingEffect newEffect, int slotIndex = -1)
    {
        // Default to currently selected slot
        if (slotIndex < 0) slotIndex = selectedSliderIndex;
        
        // Validate slot index
        if (slotIndex < 0 || slotIndex >= currentHotBar.Length)
        {
            Debug.LogError($"Invalid slot index: {slotIndex}");
            return;
        }

        // Get current effect in slot
        PostProcessingEffect oldEffect = currentHotBar[slotIndex].type;
        
        // Don't replace with same effect
        if (oldEffect == newEffect) return;

        // Find new effect's data
        HotBarPair newPair = postProcessingSliderValues.Find(p => p.type == newEffect);
        if (newPair.Equals(default(HotBarPair)))
        {
            Debug.LogError($"Effect {newEffect} not found in configuration!");
            return;
        }

        // Reset old effect to default and clear its deviation
        float oldDefault = defaultEffectValues[oldEffect];
        currentEffectValues[oldEffect] = oldDefault;
        currentDeviationUsed -= CalculateNormalizedDeviation(oldEffect);

        // Initialize new effect with its default value
        currentEffectValues[newEffect] = newPair.data.DefaultValue;
        defaultEffectValues[newEffect] = newPair.data.DefaultValue;

        // Update hotbar slot
        currentHotBar[slotIndex] = newPair;

        // Update UI slider
        SetupSlider(slotIndex, newPair);

        // Force update resource display
        RecalculateCurrentDeviation();
    }

    public void TransitionExternal(PostProcessingEffect effect, Setting setting)
    {
        // Find the effect's configuration data from all available effects
        HotBarPair newPair = postProcessingSliderValues.Find(p => p.type == effect);
        if (newPair.Equals(default(HotBarPair)))
        {
            Debug.LogError($"Effect {effect} not found in configuration!");
            return;
        }

        // Initialize or retrieve values
        if (!currentEffectValues.ContainsKey(effect))
        {
            currentEffectValues[effect] = newPair.data.DefaultValue;
        }
        
        if (!defaultEffectValues.ContainsKey(effect))
        {
            defaultEffectValues[effect] = newPair.data.DefaultValue;
        }

        float currentValue = currentEffectValues[effect];
        float targetValue;
        
        // Determine target value based on setting
        switch (setting)
        {
            case Setting.Min:
                targetValue = newPair.data.MinValue;
                break;
            case Setting.Default:
                targetValue = newPair.data.DefaultValue;
                break;
            case Setting.Max:
                targetValue = newPair.data.MaxValue;
                break;
            default:
                targetValue = newPair.data.DefaultValue;
                break;
        }

        if (Mathf.Approximately(currentValue, targetValue))
            return;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionEffectValue(effect, currentValue, targetValue, 0.5f, newPair));
    }
    #endregion

    #region Helper Methods
    private HotBarPair GetPairForEffect(PostProcessingEffect effect) {
        foreach (var pair in currentHotBar) {
            if (pair.type == effect) return pair;
        }
        return default;
    }
    private float GetTargetValue(PostProcessingEffect effect, Setting setting, HotBarPair pair)
    {
        float defaultValue = defaultEffectValues[effect];
        float min = pair.data.MinValue;
        float max = pair.data.MaxValue;

        return setting switch
        {
            Setting.Min => min,
            Setting.Default => defaultValue,
            Setting.Max => max,
            _ => defaultValue
        };
    }
    private void ApplyEffectValue(PostProcessingEffect effect, float value, HotBarPair pair)
    {
        // Store the value
        currentEffectValues[effect] = Mathf.Clamp(value, pair.data.MinValue, pair.data.MaxValue);
        
        // Apply the effect
        ApplyPostProcessingEffect(effect, value);
        
        // Recalculate deviation for UI update if needed
        RecalculateCurrentDeviation();
    }
    private IEnumerator TransitionEffectValue(PostProcessingEffect effect, float from, float to, float duration, HotBarPair pair)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float newValue = Mathf.Lerp(from, to, t);
            
            // Apply effect directly
            ApplyEffectValue(effect, newValue, pair);
            
            // Update UI if the effect is in the hotbar
            int sliderIndex = GetSliderIndexForEffect(effect);
            if (sliderIndex >= 0) 
            {
                sliders[sliderIndex].SetValueWithoutNotify(newValue);
            }
            
            yield return null;
        }

        // Final update
        ApplyEffectValue(effect, to, pair);
        
        // Update UI one last time if the effect is in the hotbar
        int finalSliderIndex = GetSliderIndexForEffect(effect);
        if (finalSliderIndex >= 0) 
        {
            sliders[finalSliderIndex].SetValueWithoutNotify(to);
        }
        
        transitionCoroutine = null;
    }

    private void UpdateSliderAndEffects(PostProcessingEffect effect, float newValue) {
        HotBarPair pair = GetPairForEffect(effect);
        newValue = Mathf.Clamp(newValue, pair.data.MinValue, pair.data.MaxValue);

        if (TryAdjustSlider(effect, newValue)) {
            int sliderIndex = GetSliderIndexForEffect(effect);
            if (sliderIndex >= 0) sliders[sliderIndex].value = newValue;
        }
    }

    private int GetSliderIndexForEffect(PostProcessingEffect effect) {
        for (int i = 0; i < currentHotBar.Length; i++) {
            if (currentHotBar[i].type == effect) return i;
        }
        return -1;
    }

    private void SelectSlider(int index) {
        selectedSliderIndex = Mathf.Clamp(index, 0, currentHotBar.Length - 1);
        UpdateSliderVisuals();
    }

    private void UpdateSliderVisuals() {
        for (int i = 0; i < sliders.Length; i++) {
            sliders[i].style.backgroundColor = new StyleColor(
                i == selectedSliderIndex ? Color.green : Color.clear
            );
        }
    }

    private void HandleTutorialUI(string sceneName) {
        if (sliderTutorialText == null) return;
        
        bool showTutorial = sceneName == "Introducing-Level";
        sliderTutorialText.SetActive(showTutorial);
        if (showTutorial) ResetSliderTutorialTimer();
    }

    private void ResetSliderTutorialTimer() {
        if (sliderTutorialCoroutine != null) StopCoroutine(sliderTutorialCoroutine);
        sliderTutorialCoroutine = StartCoroutine(HideTutorialAfterDelay(3f));
    }

    private IEnumerator HideTutorialAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        if (sliderTutorialText != null) sliderTutorialText.SetActive(false);
        sliderTutorialCoroutine = null;
    }

    private float CalculateNormalizedDeviation(PostProcessingEffect effect)
    {
        HotBarPair pair = GetPairForEffect(effect);
        float currentValue = currentEffectValues[effect];
        return Mathf.Abs(currentValue - pair.data.DefaultValue) / 
            (pair.data.MaxValue - pair.data.MinValue);
    }

    private float GetTargetValue(PostProcessingEffect effect, Setting setting)
    {
        float defaultValue = defaultEffectValues[effect];
        float min = GetPairForEffect(effect).data.MinValue;
        float max = GetPairForEffect(effect).data.MaxValue;

        return setting switch
        {
            Setting.Min => min,
            Setting.Default => defaultValue,
            Setting.Max => max,
            _ => defaultValue
        };
    }
    #endregion

    #region Data Structures
    [System.Serializable]
    private struct HotBarPair {
        public PostProcessingEffect type;
        public PostProcessingEffectData data;
    }

    public enum PostProcessingEffect {
        Brightness, AntiAliasing, MotionBlur, FilmGrain,
        ColorCorrection, ChromaticAberration, Bloom, Empty
    }

    public enum Setting {
        Max, Min, Default
    }

    [System.Serializable]
    public struct PostProcessingEffectData {
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue;
        [SerializeField] private float defaultValue;
        
        public float MinValue => minValue;
        public float MaxValue => maxValue;
        public float DefaultValue => defaultValue;
    }
    #endregion
}