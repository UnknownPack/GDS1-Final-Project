using UnityEngine;

public class Colour : MonoBehaviour
{

    public enum ColorState { Red, Blue, Green }
    private ColorState currentState;
    public ColorState startState = ColorState.Red;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentState = startState;
        GetColor();
    }

    public bool Approximately(float a, float b, float tolerance = 0.1f) {
        return Mathf.Abs(a - b) < tolerance;
    }

    public void SetState(float value) {
        if (Approximately(value, 0, 0.25f)) {
            currentState = startState;
        }
        else if (Approximately(value, 1, 0.25f)) {
            currentState = (ColorState)(((int)startState + 1) % 3);
        }
        else if (Approximately(value, -1, 0.25f)) {
            currentState = (ColorState)(((int)startState + 2) % 3);
        }
        GetColor();
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
