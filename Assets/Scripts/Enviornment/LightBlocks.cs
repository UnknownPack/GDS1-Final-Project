using UnityEngine;

public class LightBlocks : MonoBehaviour
{
    [Header("light Block Settings")] 
    [SerializeField, Tooltip("if true, square will be transparent in when completly dark. If false, the inverse will be true ")] 
    private bool OnlyShowInDark;
    SpriteRenderer spriteRenderer;
    Collider2D collider2D;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider2D = GetComponent<Collider2D>();
    }


    void Update()
    {
        Color colour = spriteRenderer.color;
        float percentage = GameManager.Instance.GetPostProcessingValue_AsPercentage(GameManager.PostProcessingEffect.Brightness);
        if (OnlyShowInDark)
        {
            float alpha = 1f - percentage;
            spriteRenderer.color = new Color(colour.r, colour.g, colour.b, alpha);
            collider2D.enabled = alpha > 0f;
        }
        else
        {
            float alpha = percentage;
            spriteRenderer.color = new Color(colour.r, colour.g, colour.b, alpha);
            collider2D.enabled = alpha > 0f;
        }
    }
}
