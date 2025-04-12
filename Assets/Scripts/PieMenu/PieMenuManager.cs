using System.Collections.Generic;
using SimplePieMenu;
using UnityEngine;

public class PieMenuManager : MonoBehaviour
{
    [SerializeField] PieMenu pieMenu;
    private PieMenuDisplayer displayer;

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
        displayer = GetComponent<PieMenuDisplayer>();
        pieMenu.OnPieMenuFullyInitialized += HideMenuItems;
    }

    void OnDestroy()
    {
        pieMenu.OnPieMenuFullyInitialized -= HideMenuItems;
    }

    void HideMenuItems() {
        List<int> menuItemsIds = new();
        for (int i = 0; i < 6; i++) {
            menuItemsIds.Add(i);
        }
        PieMenuShared.References.MenuItemsManager.MenuItemHider.Hide(pieMenu,
            menuItemsIds);
        Redraw();
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
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            DisableMenuItem(1);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            EnableMenuItem(1);
        }
    }

    private void DisableMenuItem(int menuItemId)
    {
        PieMenuShared.References.MenuItemsManager.MenuItemHider.Hide(pieMenu, new List<int> { menuItemId });
        Redraw();
    }

    private void EnableMenuItem(int menuItemId)
    {
        PieMenuShared.References.MenuItemsManager.MenuItemHider.Restore(pieMenu, new List<int> { menuItemId });
        Redraw();
    }

    private void Redraw()
    {
        var generalSettingsHandler = PieMenuShared.References.GeneralSettingsHandler;
        generalSettingsHandler.HandleRotationChange(pieMenu, 0);
    }

    // Trigger detection and adding to pie menu
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pedestal"))
        {
            string pedestalName = other.gameObject.name;

            if (postProcessingDict.ContainsKey(pedestalName))
            {
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
}
