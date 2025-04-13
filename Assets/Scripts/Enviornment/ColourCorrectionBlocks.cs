using UnityEngine;

public class ColourCorrectionBlocks : MonoBehaviour
{
    private ColourCorrectionType cuurentColourCorrectionType;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.AddToColourCorrectionBlockList(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPadType(ColourCorrectionType ColourCorrectionType)
    {
        cuurentColourCorrectionType = ColourCorrectionType;
        if (cuurentColourCorrectionType == ColourCorrectionType.JumpPad)
        {
            
        }
        else if (cuurentColourCorrectionType == ColourCorrectionType.DeathPad)
        {
            
        }
    }

    public enum ColourCorrectionType
    {
        JumpPad,
        DeathPad,
        NormalPad
    }
}
