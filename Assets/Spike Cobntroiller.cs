using System;
using UnityEngine;
using UnityEngine.UI;

public class SpikeCobntroiller : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Vector3[] verticies;
    public int verticiesAmount = 10;
    public float span;
    public float height;
    private float x;
    [SerializeField] private Slider slider;
    private float sliderValue;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        UpdateVerticies();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateVerticies() {
        sliderValue = slider.value;
        float finalSpan = span * sliderValue;
        int finalVerticiesAmount = (int)(sliderValue * verticiesAmount) + 1;
        Debug.Log(finalVerticiesAmount);
        if (finalVerticiesAmount <= 2) {
            verticies = new Vector3[2];
            verticies[0] = new Vector3(gameObject.transform.position.x ,gameObject.transform.position.y, 0);
            verticies[0] = new Vector3(1/finalSpan + gameObject.transform.position.x ,gameObject.transform.position.y, 0);
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(verticies);
        }
        else {
            verticies = new Vector3[finalVerticiesAmount];
            for (int i = 1; i < finalVerticiesAmount; i += 2) {
                x = (float)(Math.PI / (2 * finalSpan)) + (float)(2 * Math.PI * ((i - 1)/2)) / finalSpan;
                verticies[i] = new Vector3(x + gameObject.transform.position.x, 1 * height + gameObject.transform.position.y, 0);
            }
            for (int i = 0; i < finalVerticiesAmount; i += 2) {
                x = (float)( - Math.PI / (2 * finalSpan)) + (float)(2 * Math.PI * (i/2)) / finalSpan;
                verticies[i] = new Vector3(x + gameObject.transform.position.x, -1 * height + gameObject.transform.position.y, 0);
            }
            lineRenderer.positionCount = finalVerticiesAmount;
            lineRenderer.SetPositions(verticies);
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
        }

    }
}
