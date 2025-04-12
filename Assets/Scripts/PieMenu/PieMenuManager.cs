using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimplePieMenu;
using UnityEngine;

public class PieMenuManager : MonoBehaviour
{
    public static PieMenuManager Instance { get; private set; }
    [SerializeField] PieMenu pieMenu;
    private PieMenuDisplayer displayer;
    [SerializeField] private List<int> enabledItems = new();
    [SerializeField] private List<int> allWheelItems = new() {0, 1, 2, 3, 4, 5, 6};
    private bool menuInitilised = false;

    // Dictionary now maps string names to their enum index values (ints)
    private Dictionary<string, int> postProcessingDict = new Dictionary<string, int>
    {
        { "Brightness", 0 },
        { "AntiAliasing", 1 },
        { "MotionBlur", 2 },
        { "FilmGrain", 3 },
        { "ColorCorrection", 4 },
        { "ChromaticAberration", 5 },
        { "Bloom", 6 }
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        displayer = GetComponent<PieMenuDisplayer>();
        pieMenu.OnPieMenuFullyInitialized += HideMenuItems;
    }

    void OnDestroy()
    {
        pieMenu.OnPieMenuFullyInitialized -= HideMenuItems;
    }

    void OnEnable() {
            if (pieMenu == null)
        {
            pieMenu = FindObjectOfType<PieMenu>(true); // Include inactive objects
            if (pieMenu != null)
            {
                // Re-subscribe to events
                pieMenu.OnPieMenuFullyInitialized -= HideMenuItems;
                pieMenu.OnPieMenuFullyInitialized += HideMenuItems;
                
                // Re-hide items if we were already initialized
                if (menuInitilised)
                {
                    StartCoroutine(DelayedHideMenuItems());
                }
            }
        }
        else if (menuInitilised)
        {
            // If we already have a reference and were initialized, ensure items are hidden
            StartCoroutine(DelayedHideMenuItems());
    }
    }


    void HideMenuItems() {
        menuInitilised = true;
        List<int> menuItemsIds = new();
        List<int> disabledItems = allWheelItems.Except(enabledItems).ToList();
        PieMenuShared.References.MenuItemsManager.MenuItemHider.Hide(pieMenu, disabledItems);
    }

    void Start()
    {

    }

    void Update()
    {
        Display();
    }

    private void Display()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            displayer.ShowPieMenu(pieMenu);
            // StartCoroutine(DelayedHideMenuItems());
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Redraw();
        }
    }

    private IEnumerator DelayedHideMenuItems()
    {
        // Wait for a frame to ensure menu is fully loaded
        while (!menuInitilised) {yield return null;}
        HideMenuItems();
    }

    public void EnableMenuItem(int menuItemId)
    {
        PieMenuShared.References.MenuItemsManager.MenuItemHider.Restore(pieMenu, new List<int> { menuItemId });
        enabledItems.Add(menuItemId);
        Redraw();
    }

    private void Redraw()
    {
        var generalSettingsHandler = PieMenuShared.References.GeneralSettingsHandler;
        generalSettingsHandler.HandleRotationChange(pieMenu, 0);
    }

    // Trigger detection and adding to pie menu
    public void HandleAddingItem(string pedestalName)
    {
        if (postProcessingDict.ContainsKey(pedestalName))
        {
            Debug.Log(pedestalName);
            int effectIndex = postProcessingDict[pedestalName];
            EnableMenuItem(effectIndex);
            Redraw();
        }
        else
        {
            Debug.LogWarning($"No effect index found for pedestal: {pedestalName}");
        }
    }
}
