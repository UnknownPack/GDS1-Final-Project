using System.Collections;
using System.Collections.Generic; 
using UnityEngine; 
using UnityEngine.SceneManagement;
using UnityEngine.UIElements; 
using System;
using System.Linq;
using UnityEditor;

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
    private Slider tempSlider;
    private bool isTempSliderActive = false;
    private const string k_TempObjectTag = "TempSlider";
    private PostProcessingEffect tempEffect;

    private PostProccessManager postProcessManager; 
    private Dictionary<PostProcessingEffect, Coroutine> activeTransitions = new Dictionary<PostProcessingEffect, Coroutine>();
    private Dictionary<PostProcessingEffect, Sprite> iconDictionary;

    [SerializeField] private GameObject transition;
    private PixelTransitionController transitionInstance;

    #region Singleton Pattern
    public static GameManager Instance {
        get {
            if (instance == null) { 
                GameObject gameManager = Instantiate(Resources.Load<GameObject>("PostProccessing"));
                instance = gameManager.GetComponent<GameManager>(); 
                instance.SetUiDispaly(DisplayStyle.None);
                DontDestroyOnLoad(instance.gameObject);
                instance.Initialize();
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
        // Clear any existing temp slider effects first
        if (currentHotBar.All(pair => pair.type != tempEffect) && tempSlider != null)
        {
            TransitionExternal(tempEffect, Setting.Default, 0);
        }
    
        // Get unlock manager to update unlocked slots
        UnlockManager unlockManager = (UnlockManager)FindAnyObjectByType(typeof(UnlockManager));
        if (unlockManager)
        {
            unlockedSlots = unlockManager.unlockedSlots;
            maxHotBarSize = unlockManager.maxHotBarSize;
        }
    
        // Update UI references (this will handle callbacks properly)
        UpdateUIReferences();
    
        // Handle tutorial UI
        HandleTutorialUI(scene.name);
    
        // Apply current values to post processing (after UI is set up)
        for (int i = 0; i < currentHotBar.Count && i < sliders.Count; i++) {
            Debug.Log($"Applying effect {i}: {currentHotBar[i].type} with value {sliders[i].value}");
            ApplyPostProcessingEffect(currentHotBar[i].type, sliders[i].value);
        }
    
        // Setup temp slider
        tempSlider = quickAccessDocument.rootVisualElement.Q<Slider>("TempSlider");
        HideTempSlider();
        isTempSliderActive = false;
    
        // Update player prefs for level progression
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        if (PlayerPrefs.HasKey("CurrentLevelUnlocked") && PlayerPrefs.GetInt("CurrentLevelUnlocked") < currentLevelIndex)
        {
            PlayerPrefs.SetInt("CurrentLevelUnlocked", currentLevelIndex);  
            PlayerPrefs.Save();
        }
    }
    #endregion

    #region Initialization

    private void Initialize()
    {
        Start();
    }
    void Start() {
        postProcessManager = GetComponent<PostProccessManager>(); 

        // Hide temp slider until explicitly needed
        tempSlider = quickAccessDocument.rootVisualElement.Q<Slider>("TempSlider");
        if (tempSlider != null) {
            tempSlider.style.display = DisplayStyle.None;
            tempSlider.SetEnabled(false);
        } 
        transitionInstance = FindFirstObjectByType<PixelTransitionController>(); 
        
        
        iconDictionary = new Dictionary<PostProcessingEffect, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/MenuIconsNew");
        // this shit is so fucking messy but can't organise sprites in folders in unity
        iconDictionary[PostProcessingEffect.Brightness] = sprites[2];
        iconDictionary[PostProcessingEffect.AntiAliasing] = sprites[6];
        iconDictionary[PostProcessingEffect.MotionBlur] = sprites[5];
        iconDictionary[PostProcessingEffect.FilmGrain] = sprites[4];
        iconDictionary[PostProcessingEffect.ColorCorrection] = sprites[3];
        iconDictionary[PostProcessingEffect.ChromaticAberration] = sprites[0];
        iconDictionary[PostProcessingEffect.Bloom] = sprites[1];
        
        InitializeUIElements();
        
        foreach (PostProcessingEffect effect in System.Enum.GetValues(typeof(PostProcessingEffect)))
        {
            Debug.Log(effect); // or do something with each effect
        }

        
        if (!PlayerPrefs.HasKey("CurrentLevelUnlocked"))
        {
            PlayerPrefs.SetInt("CurrentLevelUnlocked", 0);
            PlayerPrefs.Save();
        }
    }

    private void LateUpdate() {

        if (tempSlider != null && tempSlider.style.display == DisplayStyle.Flex)
            PositionAndSyncTempSlider();
    }

    private void InitializeUIElements() {
        quickAccessDocument = GetComponent<UIDocument>();
        GetUIReferences();
        currentHotBar.Clear();
        for (int i = 0; i < maxHotBarSize && i < postProcessingSliderValues.Count && i < sliders.Count; i++)
        {
            var slider = sliders[i];
            if (i < unlockedSlots)
            {
                currentHotBar.Add(postProcessingSliderValues[i]);
                SetupSlider(i, postProcessingSliderValues[i]);
                HideSlider(i, true);
            }
            else
            {
                HideSlider(i, false);
            }
        }
        // SetTempSlider();
    }

    private void UpdateUIReferences() {
        quickAccessDocument = GetComponent<UIDocument>();
        GetUIReferences();
    }

    private void GetUIReferences() {
        // Unregister existing callbacks before clearing
        for (int i = 0; i < sliders.Count && i < sliderCallbacks.Count; i++) {
            if (sliderCallbacks[i] != null && sliders[i] != null) {
                sliders[i].UnregisterValueChangedCallback(sliderCallbacks[i]);
            }
        }
        
        // Remove sliders that are no longer unlocked
        for (int i = currentHotBar.Count - 1; i >= 0; i--) {
            if (i >= unlockedSlots) {
                TransitionExternal(currentHotBar[i].type, Setting.Default, 0);
                Debug.Log("Removing slider " + currentHotBar[i].type + ", unlocked: " + unlockedSlots + ", i: " + i);
                currentHotBar.RemoveAt(i);
            }
        }

        // Clear and rebuild slider references
        sliders.Clear();
        sliderCallbacks.Clear();

        // Rebuild all slider references
        for (int i = 1; i <= maxHotBarSize; i++) {
            var slider = quickAccessDocument.rootVisualElement.Q<Slider>($"hotkey{i}");
            if (slider == null) continue;
            
            sliders.Add(slider);
            sliderCallbacks.Add(null); // Add placeholder for callback
            
            slider.RegisterCallback<PointerDownEvent>(e => e.PreventDefault());
            slider.RegisterCallback<PointerMoveEvent>(e => e.PreventDefault());
        }
        Debug.Log($"funny1 complete - unlockedSlots: {unlockedSlots}, currentHotBar.Count: {currentHotBar.Count}, sliders.Count: {sliders.Count}, postProcessingSliderValues.Count: {postProcessingSliderValues.Count}");

        while (currentHotBar.Count < unlockedSlots && currentHotBar.Count < maxHotBarSize) {
            Debug.Log($"While Loop: START. currentHotBar.Count: {currentHotBar.Count}, unlockedSlots: {unlockedSlots}");
            int index = currentHotBar.Count;
            Debug.Log($"While Loop: index = {index}. Checking postProcessingSliderValues[{index}].");

            // Defensive check before accessing the list
            if (index >= postProcessingSliderValues.Count) {
                Debug.LogError($"CRITICAL ERROR: Trying to access postProcessingSliderValues[{index}] but Count is only {postProcessingSliderValues.Count}. This will crash. Ensure 'Post Processing Slider Values' list in GameManager Inspector has enough entries.");
                return; // Exit GetUIReferences to prevent the crash and see this error.
            }

            currentHotBar.Add(postProcessingSliderValues[index]);
            Debug.Log($"While Loop: Added {postProcessingSliderValues[index].type.ToString()} to currentHotBar.");

            SetupSlider(index, currentHotBar[index]);
            HideSlider(index, true);
            Debug.Log($"Added slider {index}: {postProcessingSliderValues[index].type}"); // Your existing log
            Debug.Log($"While Loop: END Iteration. currentHotBar.Count is now {currentHotBar.Count}");
        }

        Debug.Log($"funny complete - unlockedSlots: {unlockedSlots}, currentHotBar.Count: {currentHotBar.Count}, sliders.Count: {sliders.Count}");
        
        VisualElement helperIcon1 = quickAccessDocument.rootVisualElement.Q<VisualElement>("HelperIcons1");
        VisualElement helperIcon2 = quickAccessDocument.rootVisualElement.Q<VisualElement>("HelperIcons2");
        // Setup ALL unlocked sliders with proper callbacks
        Debug.Log($"funny complete - unlockedSlots: {unlockedSlots}, currentHotBar.Count: {currentHotBar.Count}, sliders.Count: {sliders.Count}");
        for (int i = 0; i < unlockedSlots && i < currentHotBar.Count && i < sliders.Count; i++) {
            SetupSlider(i, currentHotBar[i]);
            HideSlider(i, true);
            Debug.Log($"Setting up unlocked slider {i}: {currentHotBar[i].type}");
            if (i == 0)
            {
                helperIcon1.style.display = DisplayStyle.Flex;
            }

            if (i == 1)
            {
                helperIcon2.style.display = DisplayStyle.Flex;
            }
            
        }

        // Hide sliders that aren't unlocked
        for (int i = unlockedSlots; i < sliders.Count; i++) {
            HideSlider(i, false);
            if (i == 0)
            {
                helperIcon1.style.display = DisplayStyle.None;
            }
            if (i == 1)
            {
                helperIcon2.style.display = DisplayStyle.None;
            }
        }

        // Reset all hotbar effects to default
        foreach (var hotbar in currentHotBar) {
            TransitionExternal(hotbar.type, Setting.Default, 0f);
        }
        
        Debug.Log($"GetUIReferences complete - unlockedSlots: {unlockedSlots}, currentHotBar.Count: {currentHotBar.Count}, sliders.Count: {sliders.Count}");
    }
    

    private void PositionAndSyncTempSlider()
    {
        // assumes tempSlider.style.display == Flex and the tagged object exists
        GameObject target = GameObject.FindWithTag(k_TempObjectTag);
        if (target == null) { HideTempSlider(); return; }

        Camera cam = Camera.main;
        Vector3 screenPos = cam.WorldToScreenPoint(target.transform.position);
        Vector2 uiPos = new Vector2(screenPos.x, Screen.height - screenPos.y);
        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(
            quickAccessDocument.rootVisualElement.panel, uiPos);

        float w = tempSlider.resolvedStyle.width;
        float h = tempSlider.resolvedStyle.height;
        tempSlider.style.position = Position.Absolute;
        tempSlider.style.left     = panelPos.x - w * 0.5f;
        tempSlider.style.top      = panelPos.y - h * 0.5f;
    }

    private void HideTempSlider()
    {
        if (tempSlider == null) return;
        tempSlider.style.display = DisplayStyle.None;
        tempSlider.SetEnabled(false);
    }


    #endregion

    #region Slider Management
    private void SetupSlider(int index, HotBarPair pair)
    {
        /*if (index < 0 || index >= sliders.Count)  return;
        if (index >= sliderCallbacks.Count) return;

        var slider = sliders[index];
    
        // Unregister existing callback for this slider
        var existingCallback = sliderCallbacks[index];
        if (existingCallback != null) {
            slider.UnregisterValueChangedCallback(existingCallback);
        }

        // Setup slider properties
        slider.label = pair.type.ToString();
        slider.lowValue = pair.data.MinValue;
        slider.highValue = pair.data.MaxValue;
        slider.value = pair.data.DefaultValue;

        // Create and register new callback
        EventCallback<ChangeEvent<float>> newCallback = evt => OnSliderChanged(evt, pair.type);
        sliderCallbacks[index] = newCallback;
        slider.RegisterValueChangedCallback(newCallback);
    
        // Apply the default value immediately
        ApplyPostProcessingEffect(pair.type, pair.data.DefaultValue);
    
        Debug.Log($"Setup slider {index} for {pair.type} with callback registered");*/
        
        if (index < 0 || index >= sliders.Count) {
            return;
        }

        if (index >= sliderCallbacks.Count) {
            return;
        }

        var slider = sliders[index];

        // Unregister existing callback for this slider
        var existingCallback = sliderCallbacks[index];
        if (existingCallback != null) {
            slider.value = currentHotBar[index].data.DefaultValue;
            slider.UnregisterValueChangedCallback(existingCallback);
        }

        // Setup slider properties
        slider.label = pair.type.ToString();
        slider.lowValue = pair.data.MinValue;
        slider.highValue = pair.data.MaxValue;
        slider.value = pair.data.DefaultValue;

        VisualElement image = quickAccessDocument.rootVisualElement.Q<VisualElement>($"image{index + 1}");
        Debug.Log(iconDictionary[pair.type]);
        image.style.backgroundImage = new StyleBackground(iconDictionary[pair.type]);

        // Create and register new callback
        EventCallback<ChangeEvent<float>> newCallback = evt => OnSliderChanged(evt, pair.type);
        sliderCallbacks[index] = newCallback;
        slider.RegisterValueChangedCallback(newCallback);
        
        // Apply the default value immediately
        slider.value = pair.data.DefaultValue;

        Debug.Log($"Finished setting up slider {index} for {pair.type}");

    }
    
    private void manageSliders()
    {
        /*VisualElement image1 = quickAccessDocument.rootVisualElement.Q<Slider>($"hotkey{1}").ElementAt(1);
        VisualElement image2 = quickAccessDocument.rootVisualElement.Q<Slider>($"hotkey{2}").ElementAt(1);
        if(image1 == null || image2 == null) { Debug.LogError("Image of sliders not found");return; }
        image1.style.backgroundImage = new StyleBackground(iconDictionary[currentHotBar[0].type]);
        image2.style.backgroundImage = new StyleBackground(iconDictionary[currentHotBar[1].type]);*/
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
        manageSliders();
    }
    #endregion

    #region Input Handling
    private void Update()
    {
        HandleHotbarInput();
    }

 
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

        try
        {
            HotBarPair pair = currentHotBar[selectedSliderIndex];
            float currentValue = sliders[selectedSliderIndex].value;
            float range = pair.data.MaxValue - pair.data.MinValue;
            
            if (Input.GetKey(KeyCode.W)) AdjustValue(currentValue + range * 0.01f);
            if (Input.GetKey(KeyCode.S)) AdjustValue(currentValue - range * 0.01f);
            if (Input.GetKeyDown(KeyCode.Q)) CycleSliderState(false);
            if (Input.GetKeyDown(KeyCode.E)) CycleSliderState(true);
        }
        catch (Exception e)
        {
            //Console.WriteLine(e);
            throw;
        } 
 
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
        Debug.Log("hotbar type " + pair.type + " new value " + next);
        TransitionExternal(pair.type, next, 0.1f);
    }


    #endregion

    #region Public Methods

    public void SetUiDispaly(DisplayStyle style)
    {
        quickAccessDocument.rootVisualElement.style.display = style;
    }
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
            transitionInstance = FindFirstObjectByType<PixelTransitionController>();
            if(transitionInstance != null)
            {
                transitionInstance.FadeToScene(SceneManager.GetActiveScene().buildIndex);
            }
            else
            { 
                // Fallback: If no transition system is available, just reload the scene directly
                Debug.LogWarning("No transition instance found, falling back to direct scene reload");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        else 
        {
            transitionInstance.FadeToScene(SceneManager.GetActiveScene().buildIndex); 
        }
    }
    
    public void TransitionToDifferentScene(string scene) => transitionInstance.FadeToScene(scene);

    public void TransitionToNextScene()
    {
        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextScene < SceneManager.sceneCountInBuildSettings)
        {
            if (transitionInstance == null)
            {
                transitionInstance = FindFirstObjectByType<PixelTransitionController>();
                if (transitionInstance != null)
                    transitionInstance.FadeToScene(nextScene);
                else
                { 
                    Debug.LogError($"No transition instance found again");
                    return;
                }
                transitionInstance.FadeToScene(nextScene);
            }
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

        
        SetupSlider(slotIndex, newPair);
        currentHotBar[slotIndex] = newPair;
    }

    public void TransitionExternal(PostProcessingEffect effect, Setting setting, float duration) {
        // Find the effect data first
        HotBarPair newPair = postProcessingSliderValues.Find(p => p.type == effect);
        if (newPair.Equals(default(HotBarPair))) return;

        // Check if effect is in hotbar
        bool isInHotbar = false;
        int index = -1;
        for (int i = 0; i < currentHotBar.Count; i++) {
            if (currentHotBar[i].type == effect) {
                isInHotbar = true;
                index = i;
                break;
            }
        }

        // Get appropriate slider
        Slider targetSlider;
        if (!isInHotbar) {
            targetSlider = AddTempSlider(effect);
        }
        else {
            targetSlider = sliders[index];
        }

        if (targetSlider == null) return;

        float currentValue = targetSlider.value;
        float targetValue = GetTargetValue(newPair, setting);

        // Handle existing transition
        if (activeTransitions.TryGetValue(effect, out Coroutine runningCoroutine)) 
        {
            if (runningCoroutine != null) {
                StopCoroutine(runningCoroutine);
            }
            activeTransitions.Remove(effect);
        }

        // Start new transition
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
        if (currentHotBar.Any(p => p.type == effect)) return;
        if (unlockedSlots == maxHotBarSize) return;
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

    public Slider AddTempSlider(PostProcessingEffect effect) {
        if (tempSlider == null) return null;
        // If the slider is already showing this effect, just return it
        if (isTempSliderActive) {
            return tempSlider;
        }
        isTempSliderActive = true;
        tempEffect = effect;
        var pair = postProcessingSliderValues.Find(p => p.type == effect);
        if (pair.Equals(default(HotBarPair))) return null;

        // Configure range & label & value
        tempSlider.lowValue = pair.data.MinValue;
        tempSlider.highValue = pair.data.MaxValue;
        tempSlider.value = pair.data.DefaultValue;
        tempSlider.label = effect.ToString();
        
        VisualElement image = quickAccessDocument.rootVisualElement.Q<VisualElement>("Tempimage");
        image.style.backgroundImage = new StyleBackground(iconDictionary[effect]);

        // Re-hook callback
        tempSlider.UnregisterValueChangedCallback(OnTempSliderChanged);
        tempSlider.RegisterValueChangedCallback(OnTempSliderChanged);

        // Show and enable interaction
        tempSlider.style.display = DisplayStyle.Flex;
        tempSlider.SetEnabled(false);
        tempSlider.pickingMode = PickingMode.Ignore;

        return tempSlider;
    }

     private void OnTempSliderChanged(ChangeEvent<float> evt)
    {
        // apply whichever effect is currently stored
        ApplyPostProcessingEffect(tempEffect, evt.newValue);
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