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
<<<<<<< HEAD
        float percentage = GameManager.Instance.GetPostProcessingValue(GameManager.PostProcessingEffect.Brightness); 
=======
        float percentage = GameManager.Instance.GetPostProcessingValue_AsPercentage(GameManager.PostProcessingEffect.Brightness);
>>>>>>> 49602d7089c1f27c3e9a00aedfae213ee7e97d1f
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
