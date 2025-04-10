using System.Collections.Generic;
using SimplePieMenu;
using UnityEngine;

public class PieMenuManager : MonoBehaviour
{

    [SerializeField] PieMenu pieMenu;
    private PieMenuDisplayer displayer;
    private int menuItemId = 1;


    void Awake()
    {
        displayer = GetComponent<PieMenuDisplayer>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
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
        if (Input.GetKeyDown(KeyCode.P)) {
            DisableMenuItem(1);
        }
        if (Input.GetKeyDown(KeyCode.O)) {
            EnableMenuItem(1);
        }
    }

    private void DisableMenuItem(int menuItemId) {
        PieMenuShared.References.MenuItemsManager.MenuItemHider.Hide(pieMenu, new List<int> {menuItemId});
        Redraw();    
    }

    private void EnableMenuItem(int menuItemId) {
        PieMenuShared.References.MenuItemsManager.MenuItemHider.Restore(pieMenu, new List<int> {menuItemId});
        Redraw();    
    }

    private void Redraw() {
        var generalSettingsHandler = PieMenuShared.References.GeneralSettingsHandler;
        generalSettingsHandler.HandleRotationChange(pieMenu, 0);
    }
}
