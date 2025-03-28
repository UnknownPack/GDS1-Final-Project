using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
     
    private static GameManager instance;
    
    [Header("Post Processing Sliders")]
    [Tooltip("This list stores the min, max and current values a slider for a post-porcessing effect can have")]
    [SerializeField]private List<HotBarPair> postProcessingSliderValues = new List<HotBarPair>();
    
    private Dictionary<PostProcessingEffect, float> CurrentPostProcessingEffectValues;
    private HotBarPair[] CurrentHotBar = new HotBarPair[2];
    private Slider[] slider = new Slider[2];

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
     }
     #endregion
 
    void Start()
    { 
        postProccessManager = GetComponent<PostProccessManager>();
        #region Ui Intalization for variables  
        quickAccessDocument = GetComponent<UIDocument>();
        slider[0] = quickAccessDocument.rootVisualElement.Q<Slider>("hotKeyOne");
        slider[1] = quickAccessDocument.rootVisualElement.Q<Slider>("hotKeyTwo");
        #endregion 
    }

    // Update is called once per frame
    void Update()
    {
        ManageSliderProcessingValuess();
    }

    void SetSlider(int index, Slider slider, HotBarPair hotBarPair)
    {
        CurrentHotBar[index] = hotBarPair;
        slider.label = hotBarPair.type.ToString();
        //slider.value = hotBarPair.data.DefaultValue;
        slider.lowValue = hotBarPair.data.MinValue;
        slider.highValue = hotBarPair.data.MaxValue;
    }

    void ManageSliderProcessingValuess()
    {
        for (int i = 0; i < 2; i++)
        {
            postProccessManager.AdjustValue(CurrentHotBar[i].type, slider[i].value);
        }
    }

    void InitalizeDefaultSliderValues()
    {
        
    }
    
    
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

    [System.Serializable]private struct PostProcessingEffectData
    { 
        [SerializeField, Tooltip("minimum value the slider can change for the value.")]
        private float minValue;
        [SerializeField, Tooltip("maximum value the slider can change for the value.")]
        private float maxValue;
        [SerializeField, Tooltip("Default value, the slider and value is set to at the start of a new scene")]
        private float currentValue;
        
        public float MinValue => minValue;
        public float MaxValue => maxValue;
        public float CurrentValue => currentValue;
    }
}
