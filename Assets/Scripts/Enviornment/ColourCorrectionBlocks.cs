using UnityEngine;

public class ColourCorrectionBlocks : MonoBehaviour
{
    [SerializeField, Tooltip("Predefine the rbg of the bloock")]private Vector3 vectorType; 
    private SpriteRenderer spriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GameManager.Instance.AddToColourCorrectionBlockList(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void setVectorType(Vector3 VectorType)
    {
        vectorType = VectorType;
        float[] vector3 = new []{vectorType.x, vectorType.y, vectorType.z};
        float highest = float.MinValue;
        float index = -1;
        for (int i = 0; i < vector3.Length; i++)
        {
            if(vector3[i] > highest)
            {
                highest = vector3[i];
                index = i;
            }
        }

        if (index == -1)
        {
            Debug.LogError("Error in Method Execution");
            return;
        }

        switch (index)
        {
           case 0:
               gameObject.tag = "DeathBox";
               break;
           case 1:
               gameObject.tag = "";
               break;
           case 2:
               gameObject.tag = "Safe";
               break; 
        }
    }
    
    public SpriteRenderer GetSpriteRenderer(){return spriteRenderer;}
 
    public struct ColorChannelMatrix
    {
        public Vector3 RedChannel;
        public Vector3 GreenChannel;
        public Vector3 BlueChannel;

        public ColorChannelMatrix(Vector3 red, Vector3 green, Vector3 blue)
        {
            RedChannel = red;
            GreenChannel = green;
            BlueChannel = blue;
        }
    }
}
