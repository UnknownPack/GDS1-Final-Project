using UnityEngine;

public class Colour : MonoBehaviour
{

    public enum ColorState { Red, Blue, Green }
    public ColorState currentState = ColorState.Red;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void CycleUp()
    {
        currentState = (ColorState)(((int)currentState + 1) % 3);
    }

    public void CycleDown()
    {
        currentState = (ColorState)(((int)currentState + 2) % 3);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Color GetColor()
    {
        switch (currentState)
        {
            case ColorState.Red: 
                gameObject.tag = "DeathBox";
                return Color.red;
            case ColorState.Green: 
                gameObject.tag = "Bounce";
                return Color.green;
            case ColorState.Blue: 
                gameObject.tag = "Safe";
                return Color.blue;
            default: 
                gameObject.tag = "DeathBox";
                return Color.red;
        }
    }
}
