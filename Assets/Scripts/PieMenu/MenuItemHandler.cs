using SimplePieMenu;
using UnityEngine;

public class MenuItemHandler : MonoBehaviour, IMenuItemClickHandler
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameManager.PostProcessingEffect postProcessingEffect;
    public void Handle()
    {
        GameManager.Instance.ReplaceSlider(postProcessingEffect);
    }
}
