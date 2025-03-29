using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
     
    private static GameManager instance;
    
    [Header("Post Processing Sliders")]
    [Tooltip("This list stores the min, max and current values a slider for a post-porcessing effect can have")]
    [SerializeField]private List<HotBarPair> postProcessingSliderValues = new List<HotBarPair>();
    
    private Dictionary<PostProcessingEffect, float> CurrentPostProcessingEffectValues;
    private HotBarPair[] CurrentHotBar = new HotBarPair[3];
    private Slider[] slider = new Slider[3];

    private PostProccessManager postProccessManager; 
    UIDocument quickAccessDocument; 
    
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
     }
     
     #endregion
 
    void Start()
    { 
        postProccessManager = GetComponent<PostProccessManager>();
        #region Ui Intalization for variables  
        quickAccessDocument = GetComponent<UIDocument>();
        slider[0] = quickAccessDocument.rootVisualElement.Q<Slider>("hotKeyOne");
        slider[1] = quickAccessDocument.rootVisualElement.Q<Slider>("hotKeyTwo");
        slider[2] = quickAccessDocument.rootVisualElement.Q<Slider>("hotKeyThree");
        #endregion

        for (int i = 0; i < 3; i++)
        {
            SetSlider(i, slider[i], postProcessingSliderValues[i]);
        } 
    }

    // Update is called once per frame
    void Update()
    {
        //ManageSliderProcessingValuess();
    }

    void SetSlider(int index, Slider slider, HotBarPair hotBarPair)
    {
        CurrentHotBar[index] = hotBarPair; 
        slider.value = CurrentPostProcessingEffectValues[hotBarPair.type];
        slider.lowValue = hotBarPair.data.MinValue;
        slider.highValue = hotBarPair.data.MaxValue;
    }

    void ManageSliderProcessingValuess()
    {
        for (int i = 0; i < 3; i++)
        {
            postProccessManager.AdjustValue(CurrentHotBar[i].type, slider[i].value);
            CurrentPostProcessingEffectValues[CurrentHotBar[i].type] = slider[i].value;
        }
    }

    // intalize default values at  the start of each scene
    void InitalizeDefaultSliderValues()
    {
        CurrentPostProcessingEffectValues = new Dictionary<PostProcessingEffect, float>();
        foreach (HotBarPair hotBarPair in postProcessingSliderValues)
        {
            CurrentPostProcessingEffectValues.Add(hotBarPair.type, hotBarPair.data.DefaultValue);
        }
    }
    
    #region Public Methods

    public float GetPostProcessingValue_AsPercentage(PostProcessingEffect postProcessingEffect)
    {
        foreach (HotBarPair hotBarPair in postProcessingSliderValues)
        {
            if(hotBarPair.type == postProcessingEffect)
                return CurrentPostProcessingEffectValues[postProcessingEffect] / hotBarPair.data.MaxValue;
        } 
        Debug.LogError("No Post Processing Effect found: " + postProcessingEffect);
        return 0;
    } 

    public float GetPostProcessingValue_AsFloat(PostProcessingEffect postProcessingEffect){
        foreach (HotBarPair hotBarPair in postProcessingSliderValues)
        {
            if(hotBarPair.type == postProcessingEffect)
                return CurrentPostProcessingEffectValues[postProcessingEffect];
        } 
        Debug.LogError("No Post Processing Effect found: " + postProcessingEffect);
        return 0;
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
}
