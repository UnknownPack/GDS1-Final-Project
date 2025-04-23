using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using SimplePieMenu;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    [Header("Post Processing Sliders")]
    [SerializeField] private List<HotBarPair> postProcessingSliderValues = new List<HotBarPair>();

    [Header("Tutorial UI (Introducing-Level Only)")]
    public GameObject sliderTutorialText;
    private UIDocument quickAccessDocument;

    private HotBarPair[] currentHotBar = new HotBarPair[3];
    private Slider[] sliders = new Slider[3];
    private int selectedSliderIndex = 0;
    
    private Coroutine sliderTutorialCoroutine;
    private EventCallback<ChangeEvent<float>>[] sliderCallbacks = new EventCallback<ChangeEvent<float>>[3];

    private PostProccessManager postProcessManager; 
    private Dictionary<PostProcessingEffect, Coroutine> activeTransitions = new Dictionary<PostProcessingEffect, Coroutine>();

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
        UpdateUIReferences();
        
        HandleTutorialUI(scene.name);
        SetTempSlider();
        ResetAllSlidersToDefault();
    }
    #endregion

    #region Initialization
    void Start() {
        postProcessManager = GetComponent<PostProccessManager>();
        InitializeUIElements();
    }

    private void InitializeUIElements() {
        quickAccessDocument = GetComponent<UIDocument>();
        GetUIReferences();
        SetupSlider(0, postProcessingSliderValues[0]);
        SetupSlider(1, postProcessingSliderValues[1]);
    }

    private void UpdateUIReferences() {
        quickAccessDocument = GetComponent<UIDocument>();
        GetUIReferences();
        SetupSlider(0, currentHotBar[0]);
        SetupSlider(1, currentHotBar[1]);
    }

    private void GetUIReferences() {
        sliders[0] = quickAccessDocument.rootVisualElement.Q<Slider>("hotkeyOne");
        sliders[1] = quickAccessDocument.rootVisualElement.Q<Slider>("hotkeyTwo");
        sliders[2] = quickAccessDocument.rootVisualElement.Q<Slider>("TempSlider");
        sliders[2].RegisterCallback<PointerDownEvent>(e => e.PreventDefault());
        sliders[2].RegisterCallback<PointerMoveEvent>(e => e.PreventDefault());
        sliders[2].SetEnabled(false); // Disables ALL interaction (mouse + keyboard)
    }

    private void SetTempSlider() {
        GameObject targetObject = GameObject.FindWithTag("TempSlider");
        if (targetObject == null) {SetTempSlider(false); return; }
        
        Camera mainCamera = Camera.main;
        if (mainCamera == null) { return; }

        // Convert world position to screen space
        Vector3 worldPos = targetObject.transform.position;
        Vector3 screenPos3D = mainCamera.WorldToScreenPoint(worldPos);
        
        // Flip Y-axis for UI Document coordinate system
        Vector2 screenPos = new Vector2(
            screenPos3D.x, 
            Screen.height - screenPos3D.y // Invert Y coordinate
        );

        // Convert to UI Document space
        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(
            quickAccessDocument.rootVisualElement.panel, 
            screenPos
        );

        // Get resolved dimensions
        float sliderWidth = sliders[2].resolvedStyle.width;
        float sliderHeight = sliders[2].resolvedStyle.height;

        // Apply position (centered)
        sliders[2].style.position = Position.Absolute;
        sliders[2].style.left = panelPos.x - sliderWidth / 2;
        sliders[2].style.top = panelPos.y - sliderHeight / 2;
    }

    private void SetTempSlider(bool enabled) {
        if (enabled) {
            sliders[2].SetEnabled(true);
            sliders[2].style.display = DisplayStyle.Flex;
        }
        else {
            sliders[2].SetEnabled(false);
            sliders[2].style.display = DisplayStyle.None;
        }

    }
    #endregion

    #region Slider Management
    private void SetupSlider(int index, HotBarPair pair) {
        currentHotBar[index] = pair;
        var slider = sliders[index];

        if (sliderCallbacks[index] != null) {
            slider.UnregisterValueChangedCallback(sliderCallbacks[index]);
        }

        slider.label = pair.type.ToString();
        slider.value = pair.data.DefaultValue;
        slider.lowValue = pair.data.MinValue;
        slider.highValue = pair.data.MaxValue;

        

        PostProcessingEffect effect = pair.type;
        sliderCallbacks[index] = evt => OnSliderChanged(evt, effect);
        slider.RegisterValueChangedCallback(sliderCallbacks[index]);
    }

    private void OnSliderChanged(ChangeEvent<float> evt, PostProcessingEffect effect) {
        ApplyPostProcessingEffect(effect, evt.newValue);
        ResetSliderTutorialTimer();
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
        }
    }
    #endregion

    #region Input Handling
    private void Update() => HandleHotbarInput();

    private void HandleHotbarInput() {
        SetTempSlider();
        HandleSelectionInput();
        HandleAdjustmentInput();
    }

    private void HandleSelectionInput() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlider(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlider(1);
        if (Input.GetKeyDown(KeyCode.R)) RestartLevel();
        if (Input.GetKeyDown(KeyCode.Backspace)) LoadNextScene();
    }

    private void HandleAdjustmentInput() {
        if (selectedSliderIndex < 0 || selectedSliderIndex >= currentHotBar.Length) return;

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
        const float epsilon = 0.0001f;
        HotBarPair pair = currentHotBar[selectedSliderIndex];
        float currentValue = sliders[selectedSliderIndex].value;
        float defaultValue = pair.data.DefaultValue;
        float min = pair.data.MinValue;
        float max = pair.data.MaxValue;
        bool minEqualsDefault = Mathf.Approximately(min, defaultValue);

        Setting current;
        float value = sliders[selectedSliderIndex].value;
        if (Mathf.Abs(value - min) < epsilon)
            current = Setting.Min;
        else if (Mathf.Abs(value - max) < epsilon)
            current = Setting.Max;
        else
            current = Setting.Default;
        
        // Get next setting
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
        for (int i = 0; i < currentHotBar.Length; i++) {
            if (currentHotBar[i].type == effect) {
                return sliders[i].value / currentHotBar[i].data.MaxValue;
            }
        }
        Debug.LogError($"Effect not found: {effect}");
        return 0;
    }

    public void RestartLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    public float GetDefaultValue(PostProcessingEffect effect) {
        foreach (var pair in postProcessingSliderValues) {
            if (pair.type == effect) {
                return pair.data.DefaultValue;
            }
        }
        Debug.LogError($"Effect not found: {effect}");
        return 0f;
    }

    public void ResetAllSlidersToDefault() {
        for (int i = 0; i < currentHotBar.Length; i++) {
            HotBarPair pair = currentHotBar[i];
            TransitionExternal(pair.type, Setting.Default, 0.1f);
        }
    }

    public void ReplaceSlider(PostProcessingEffect newEffect, int slotIndex = -1) {
        slotIndex = slotIndex < 0 ? selectedSliderIndex : Mathf.Clamp(slotIndex, 0, currentHotBar.Length - 1);
        
        for (int i = 0; i < 2; i++) {
            if (currentHotBar[i].type == newEffect) return;
        }

        TransitionExternal(currentHotBar[selectedSliderIndex].type, Setting.Default, 0.1f);

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
            ReplaceSlider(effect, 2);
        }
        HotBarPair newPair = postProcessingSliderValues.Find(p => p.type == effect);
        if (newPair.Equals(default(HotBarPair))) return;

        int index = FindSliderIndex(effect);
        if (index == -1) return;

        Slider targetSlider = sliders[index];
        float currentValue = targetSlider.value;
        float targetValue = GetTargetValue(newPair, setting);
        // Debug.Log(currentValue + " " + setting + " " + effect);

        if (Mathf.Approximately(currentValue, targetValue)) return;

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
        foreach (var item in activeTransitions.Keys)
        {
            Debug.Log(item);
            Debug.Log(activeTransitions.Count);
        }
    }

    private int FindSliderIndex(PostProcessingEffect effect) {
        for (int i = 0; i < currentHotBar.Length; i++) {
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
            // Debug.Log(effect + " " + newValue);
            yield return null;
        }
        targetSlider.value = to;
        activeTransitions.Remove(effect);
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