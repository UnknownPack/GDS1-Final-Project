using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using SimplePieMenu;
using UnityEditorInternal;
using UnityEngine.Rendering;
using System;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    private static GameManager instance; 

    [Header("Post Processing Sliders")]
    [SerializeField] private List<HotBarPair> postProcessingSliderValues = new List<HotBarPair>();

    [Header("Tutorial UI (Introducing-Level Only)")]
    public GameObject sliderTutorialText;
    private UIDocument quickAccessDocument;

    private List<HotBarPair> currentHotBar = new List<HotBarPair>();
    private List<Slider> sliders = new List<Slider>();
    [SerializeField]
    private int maxHotBarSize = 7;
    [SerializeField]
    private int unlockedSlots = 2;
    private int selectedSliderIndex = 0;
    
    private Coroutine sliderTutorialCoroutine;
    private List<EventCallback<ChangeEvent<float>>> sliderCallbacks = new List<EventCallback<ChangeEvent<float>>>();

    private PostProccessManager postProcessManager; 
    private Dictionary<PostProcessingEffect, Coroutine> activeTransitions = new Dictionary<PostProcessingEffect, Coroutine>();

    [SerializeField] private GameObject transition;
    private PixelTransitionController transitionInstance;

    #region Singleton Pattern
    public static GameManager Instance {
        get {
            if (instance == null) {
                GameObject gmObject = new GameObject("GameManager");
                instance = gmObject.AddComponent<GameManager>(); 
                DontDestroyOnLoad(instance.gameObject);
                instance.InitializeGameManager();
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
        UpdateUIReferences();
        
        HandleTutorialUI(scene.name);
        for (int i = 0; i < currentHotBar.Count; i++) {
            Debug.Log(currentHotBar[i].type + " " + sliders[i].value);
            ApplyPostProcessingEffect(currentHotBar[i].type, sliders[i].value);
        }
        
        //Updates the playerpref so the player can access the most recent level they have played
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        if (PlayerPrefs.HasKey("CurrentLevelUnlocked") && PlayerPrefs.GetInt("CurrentLevelUnlocked") < currentLevelIndex)
        {
            PlayerPrefs.SetInt("CurrentLevelUnlocked", currentLevelIndex);  
            PlayerPrefs.Save();
        }
    }
    #endregion

    #region Initialization
    void Start() {
        postProcessManager = GetComponent<PostProccessManager>(); 
        tempSlider = quickAccessDocument.rootVisualElement.Q<Slider>("TempSlider");
        if (tempSlider != null) {
            tempSlider.style.display = DisplayStyle.None;
            tempSlider.SetEnabled(false);
        }
        InitializeUIElements();
    }

    public void InitializeGameManager()
    {
        Start(); 
    } 

    private void InitializeUIElements() {
        quickAccessDocument = GetComponent<UIDocument>();
        GetUIReferences();
        currentHotBar.Clear();
        for (int i = 0; i < maxHotBarSize && i < postProcessingSliderValues.Count; i++) {
            var slider = sliders[i];
            if (i < unlockedSlots) {
                currentHotBar.Add(postProcessingSliderValues[i]);
                SetupSlider(i, postProcessingSliderValues[i]);
            }
            else {
                HideSlider(i, false);
            }
        }
    }

    private void UpdateUIReferences() {
        quickAccessDocument = GetComponent<UIDocument>();
        GetUIReferences();
        // GetUIReferences();
        // SetupSlider(0, currentHotBar[0]);
        // SetupSlider(1, currentHotBar[1]);
        // if (sliders[2] != null) {
        //     HotBarPair pair = postProcessingSliderValues.Find(p => p.type == currentHotBar[2].type);
        //     TransitionExternal(pair.type, Setting.Default, 0.0f);
        // }
    }

    private void GetUIReferences() {
        sliders.Clear();
        sliderCallbacks.Clear();

        for (int i = 1; i <= maxHotBarSize; i++) {
            var slider = quickAccessDocument.rootVisualElement.Q<Slider>($"hotkey{i}");
            if (slider == null) continue;
            sliderCallbacks.Add(null);
            sliders.Add(slider);
            slider.RegisterCallback<PointerDownEvent>(e => e.PreventDefault());
            slider.RegisterCallback<PointerMoveEvent>(e => e.PreventDefault());

            
        }
    }

    // private void SetTempSlider() {
    //     GameObject targetObject = GameObject.FindWithTag("TempSlider");
    //     if (targetObject == null) {HideSlider(3, false); return; }
        
    //     Camera mainCamera = Camera.main;
    //     if (mainCamera == null) { return; }

    //     // Convert world position to screen space
    //     Vector3 worldPos = targetObject.transform.position;
    //     Vector3 screenPos3D = mainCamera.WorldToScreenPoint(worldPos);
        
    //     // Flip Y-axis for UI Document coordinate system
    //     Vector2 screenPos = new Vector2(
    //         screenPos3D.x, 
    //         Screen.height - screenPos3D.y // Invert Y coordinate
    //     );

    //     // Convert to UI Document space
    //     Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(
    //         quickAccessDocument.rootVisualElement.panel, 
    //         screenPos
    //     );

    //     // Get resolved dimensions
    //     float sliderWidth = sliders[2].resolvedStyle.width;
    //     float sliderHeight = sliders[2].resolvedStyle.height;

    //     // Apply position (centered)
    //     sliders[2].style.position = Position.Absolute;
    //     sliders[2].style.left = panelPos.x - sliderWidth / 2;
    //     sliders[2].style.top = panelPos.y - sliderHeight / 2;
    // }
    #endregion
    
    #region Level Tracking Handling

    void InitializeLevelTracking()
    {
        if (!PlayerPrefs.HasKey("CurrentLevelUnlocked"))
        {
            PlayerPrefs.SetInt("CurrentLevelUnlocked", 0);  
            PlayerPrefs.Save();
        }

    }
    #endregion
    #region Slider Management
    private void SetupSlider(int index, HotBarPair pair) {
        if (index < 0 || index >= sliders.Count) return;
        if (currentHotBar.Count <= index) return;

        var slider = sliders[index];
        var oldHotBarPair = currentHotBar[index];
        var callback = sliderCallbacks[index];
        var existingCallback = sliderCallbacks[index];
        if (existingCallback != null) {
            // Unregister the previous value-changed callback
            slider.value = oldHotBarPair.data.DefaultValue;
            slider.UnregisterValueChangedCallback(existingCallback);
        }

        currentHotBar[index] = pair;

        slider.label = pair.type.ToString();
        slider.lowValue = pair.data.MinValue;
        slider.highValue = pair.data.MaxValue;
        slider.value = pair.data.DefaultValue;

        EventCallback<ChangeEvent<float>> newCallback = evt => OnSliderChanged(evt, pair.type);
        sliderCallbacks[index] = newCallback;
        slider.RegisterValueChangedCallback(newCallback);
        TransitionExternal(oldHotBarPair.type, Setting.Default, 0.0f);
        //get all elemtns in callbacks
    }

    private void OnSliderChanged(ChangeEvent<float> evt, PostProcessingEffect effect) {
        ApplyPostProcessingEffect(effect, evt.newValue);
        ResetSliderTutorialTimer();
    }

    private void ApplyPostProcessingEffect(PostProcessingEffect effect, float value) {
        switch (effect) {
            case PostProcessingEffect.Brightness: postProcessManager.ChangeBrightness(value); break;
            case PostProcessingEffect.AntiAliasing: postProcessManager.ChangeAntiAlyasing(value); break;
            case PostProcessingEffect.MotionBlur: postProcessManager.ChangeMotionBlur(value); break;
            case PostProcessingEffect.FilmGrain: postProcessManager.ChangeFilmGrain(value); break;
            case PostProcessingEffect.ColorCorrection: postProcessManager.ChangeColorCorrection(value); break;
            case PostProcessingEffect.ChromaticAberration: postProcessManager.ChangeChromaticAberration(value); break;
            case PostProcessingEffect.Bloom: postProcessManager.ChangeBloom(value); break;
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
        for (KeyCode k = KeyCode.Alpha1; k <= KeyCode.Alpha8; k++) {
            if (Input.GetKeyDown(k)) {
                int idx = k - KeyCode.Alpha1;
                SelectSlider(idx);
            }
        }
        if (Input.GetKeyDown(KeyCode.R)) RestartLevel();
        if (Input.GetKeyDown(KeyCode.Backspace)) LoadNextScene();
    }

    private void HandleAdjustmentInput() {
        if (selectedSliderIndex < 0 || selectedSliderIndex >= unlockedSlots) return;

        HotBarPair pair = currentHotBar[selectedSliderIndex];
        float currentValue = sliders[selectedSliderIndex].value;
        float range = pair.data.MaxValue - pair.data.MinValue;

        if (Input.GetKey(KeyCode.W)) AdjustValue(currentValue + range * 0.01f);
        if (Input.GetKey(KeyCode.S)) AdjustValue(currentValue - range * 0.01f);
        if (Input.GetKeyDown(KeyCode.Q)) CycleSliderState(false);
        if (Input.GetKeyDown(KeyCode.E)) CycleSliderState(true);
    }

    private void AdjustValue(float newValue) {
        Slider selectedSlider = sliders[selectedSliderIndex];
        HotBarPair pair = currentHotBar[selectedSliderIndex];
        newValue = Mathf.Clamp(newValue, pair.data.MinValue, pair.data.MaxValue);
        selectedSlider.value = newValue;
    }

    private void CycleSliderState(bool increase) {
        const float eps = 0.0001f;
        var pair = currentHotBar[selectedSliderIndex];
        var slider = sliders[selectedSliderIndex];
        float min = pair.data.MinValue, max = pair.data.MaxValue, def = pair.data.DefaultValue;
        float val = slider.value;
        bool minEqualsDefault = Mathf.Abs(min - def) < eps;

        Setting current = Mathf.Abs(val - min) < eps ? Setting.Min : Mathf.Abs(val - max) < eps ? Setting.Max : Setting.Default;
        Setting next;
        if (increase) { // E key - go up
            next = current == Setting.Min ? (minEqualsDefault ? Setting.Max : Setting.Default) :
                current == Setting.Default ? Setting.Max : Setting.Max;
        } else { // Q key - go down
            next = current == Setting.Max ? (minEqualsDefault ? Setting.Min : Setting.Default) :
                current == Setting.Default ? Setting.Min : Setting.Min;
        }

        TransitionExternal(pair.type, next, 0.1f);
    }


    #endregion

    #region Public Methods
    public float GetPostProcessingValue(PostProcessingEffect effect) {
        for (int i = 0; i < currentHotBar.Count; i++) {
            if (currentHotBar[i].type == effect) {
                return sliders[i].value / currentHotBar[i].data.MaxValue;
            }
        }
        Debug.LogError($"Effect not found: {effect}");
        return 0;
    }

    public void RestartLevel()
    {
        if (transitionInstance == null)
        {
            Debug.LogWarning("No transition instance assigned");
            return;
        }
        transitionInstance.FadeToScene(SceneManager.GetActiveScene().name); 
    }
    
    public void TransitionToDifferentScene(string scene) => transitionInstance.FadeToScene(scene);

    public void TransitionToNextScene()
    {
        if (transitionInstance == null)
        {
            Debug.LogWarning("No transition instance assigned");
            return;
        }
        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextScene < SceneManager.sceneCountInBuildSettings)
        {
            transitionInstance.FadeToSceneInt(nextScene);
        }
        else 
            Debug.LogError($"Scene not found: {SceneManager.GetActiveScene().name}! Exceeded scene count: {SceneManager.sceneCount}");
    }

    public float GetDefaultValue(PostProcessingEffect effect) {
        foreach (var pair in postProcessingSliderValues) {
            if (pair.type == effect) {
                return pair.data.DefaultValue;
            }
        }
        Debug.LogError($"Effect not found: {effect}");
        return 0f;
    }

    public void SetUIDisplay(DisplayStyle display) { quickAccessDocument.rootVisualElement.style.display = display; }

    public void ResetAllSlidersToDefault() {
        for (int i = 0; i < currentHotBar.Count; i++) {
            HotBarPair pair = currentHotBar[i];
            TransitionExternal(pair.type, Setting.Default, 0.0f);
        }
    }

    public void ReplaceSlider(PostProcessingEffect newEffect, int slotIndex = -1) {
        slotIndex = slotIndex < 0 ? selectedSliderIndex : Mathf.Clamp(slotIndex, 0, currentHotBar.Count - 1);
        
        for (int i = 0; i < unlockedSlots; i++) {
            if (currentHotBar[i].type == newEffect) return;
        }

        // TransitionExternal(currentHotBar[selectedSliderIndex].type, Setting.Default, 0.0f);

        HotBarPair newPair = postProcessingSliderValues.Find(p => p.type == newEffect);
        if (newPair.Equals(default(HotBarPair))) return;

        currentHotBar[slotIndex] = newPair;
        SetupSlider(slotIndex, newPair);
    }

    public void TransitionExternal(PostProcessingEffect effect, Setting setting, float duration) {
        bool isInHotbar = false;
        foreach (HotBarPair pair in currentHotBar) {
            if (pair.type == effect) isInHotbar = true;
        }
        if (!isInHotbar) {
            AddTempSlider(effect);
        }
        HotBarPair newPair = postProcessingSliderValues.Find(p => p.type == effect);
        if (newPair.Equals(default(HotBarPair))) return;

        int index = FindSliderIndex(effect);
        if (index == -1) return;

        Slider targetSlider = sliders[index];
        if (targetSlider == null) return;
        float currentValue = targetSlider.value;
        float targetValue = GetTargetValue(newPair, setting);
        // Debug.Log(currentValue + " " + setting + " " + effect);

        // if (Mathf.Approximately(currentValue, targetValue)) return;

        if (activeTransitions.TryGetValue(effect, out Coroutine runningCoroutine)) 
        {
            if (runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
            }
            // Remove the entry regardless
            activeTransitions.Remove(effect);
        }
        // Then add the new coroutine to the dictionary
        activeTransitions[effect] = StartCoroutine(TransitionEffectValue(targetSlider, currentValue, targetValue, duration, effect));
    }

    public void HideSlider(int index, bool enabled) {
        if (enabled) {
            sliders[index].SetEnabled(true);
            sliders[index].style.display = DisplayStyle.Flex;
        }
        else {
            sliders[index].SetEnabled(false);
            sliders[index].style.display = DisplayStyle.None;
        }

    }

    public void AddSlider(String effectName) {
        PostProcessingEffect effect = (PostProcessingEffect)Enum.Parse(typeof(PostProcessingEffect), effectName);
        unlockedSlots++;
        if (currentHotBar.Count >= maxHotBarSize) return;
        HotBarPair newPair = postProcessingSliderValues.Find(p => p.type == effect);
        if (newPair.Equals(default(HotBarPair))) return;
        //if the new effect is already in the hotbar, remove it
        for (int i = 0; i < currentHotBar.Count; i++) {
            if (currentHotBar[i].type == effect) {
                currentHotBar.RemoveAt(i);
                break;
            }
        }

        currentHotBar.Add(newPair);
        SetupSlider(currentHotBar.Count - 1, newPair);
        HideSlider(currentHotBar.Count - 1, true);
    }

    public void AddTempSlider(PostProcessingEffect effect) {
        HotBarPair newPair = postProcessingSliderValues.Find(p => p.type == effect);

        currentHotBar.Add(newPair);
        SetupSlider(0, newPair);
    }

    private int FindSliderIndex(PostProcessingEffect effect) {
        for (int i = 0; i < currentHotBar.Count; i++) {
            if (currentHotBar[i].type == effect) {
                return i;
            }
        }
        return -1;
    }

    private float GetTargetValue(HotBarPair pair, Setting setting) {
        return setting switch {
            Setting.Min => pair.data.MinValue,
            Setting.Default => pair.data.DefaultValue,
            Setting.Max => pair.data.MaxValue,
            _ => pair.data.DefaultValue
        };
    }
    
    #endregion

    #region Helper Methods
    private IEnumerator TransitionEffectValue(Slider targetSlider, float from, float to, float duration, PostProcessingEffect effect) {
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float newValue = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            targetSlider.value = newValue;
            yield return null;
        }
        targetSlider.value = to;
        ApplyPostProcessingEffect(effect, to);
        activeTransitions.Remove(effect);
    }

    private void SelectSlider(int index) {
        selectedSliderIndex = Mathf.Clamp(index, 0, currentHotBar.Count - 1);
        UpdateSliderVisuals();
    }

    private void UpdateSliderVisuals() {
        for (int i = 0; i < sliders.Count; i++) {
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

    private void LoadNextScene() {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(nextIndex);
    }
    #endregion

    #region Custom Structs
    [System.Serializable]
    private struct HotBarPair {
        public PostProcessingEffect type;
        public PostProcessingEffectData data;
    }

    public enum PostProcessingEffect {
        Brightness, AntiAliasing, MotionBlur, FilmGrain,
        ColorCorrection, ChromaticAberration, Bloom
    }

    public enum Setting { Max, Min, Default }

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
