using System.Collections.Generic;
using SimplePieMenu;
using UnityEngine;

public class PieMenuManager : MonoBehaviour
{

    [SerializeField] PieMenu pieMenu;
    private PieMenuDisplayer displayer;
    List<int> menuItemsIds = new List<int>();
    public int amountShown = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        displayer = GetComponent<PieMenuDisplayer>();
        pieMenu.OnPieMenuFullyInitialized += HideMenuItem;
    }
    void Start()
    {
        
    }

    void OnDestroy()
    {
        pieMenu.OnPieMenuFullyInitialized -= HideMenuItem;
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
        if (Input.GetKeyDown(KeyCode.P))
        {
            amountShown += 1;
            HideMenuItem();
            
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            amountShown -= 1;

            ShowMenuItem();
        }

    }

    private void HideMenuItem () {
        menuItemsIds = new();
        for (int i = amountShown; i > 0; i--) {
            menuItemsIds.Add(i);
        }
        PieMenuShared.References.MenuItemsManager.MenuItemHider.Hide(pieMenu, menuItemsIds);
    }

    private void ShowMenuItem () {
        PieMenuShared.References.MenuItemsManager.MenuItemHider.Restore(pieMenu);
        HideMenuItem();
    }
}
