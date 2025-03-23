using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpikeCobntroiller : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Vector3[] verticies;
    public int verticiesAmount = 10;
    public float period;
    public float height;
    public float heightScaleFactor;
    private float x;
    [SerializeField] private Slider slider;
    private float sliderValue;
    private Transform point1;
    private Transform point2;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        
        point1 = gameObject.transform.GetChild(0);
        point2 = gameObject.transform.GetChild(1);
        UpdateVerticies();
        
    }

    // Update is called once per frame
    void Update()
    {   
        UpdateVerticies();
    }

    public void UpdateVerticies() {
        // sliderValue = slider.value;
        // float finalSpan = span * sliderValue;
        // int finalVerticiesAmount = (int)(sliderValue * verticiesAmount) + 1;
        // Debug.Log(finalVerticiesAmount);
        // if (finalVerticiesAmount <= 2) {
        //     verticies = new Vector3[2];
        //     verticies[0] = new Vector3(gameObject.transform.position.x ,gameObject.transform.position.y, 0);
        //     verticies[1] = new Vector3(1/finalSpan + gameObject.transform.position.x ,gameObject.transform.position.y, 0);
        //     lineRenderer.positionCount = 2;
        //     lineRenderer.SetPositions(verticies);
        // }
        // else {
        //     verticies = new Vector3[finalVerticiesAmount];
        //     for (int i = 1; i < finalVerticiesAmount; i += 2) {
        //         x = (float)(Math.PI / (2 * finalSpan)) + (float)(2 * Math.PI * ((i - 1)/2)) / finalSpan;
        //         verticies[i] = new Vector3(x + gameObject.transform.position.x, 1 * height + gameObject.transform.position.y, 0);
        //     }
        //     for (int i = 0; i < finalVerticiesAmount; i += 2) {
        //         x = (float)( - Math.PI / (2 * finalSpan)) + (float)(2 * Math.PI * (i/2)) / finalSpan;
        //         verticies[i] = new Vector3(x + gameObject.transform.position.x, -1 * height + gameObject.transform.position.y, 0);
        //     }
        //     lineRenderer.positionCount = finalVerticiesAmount;
        //     lineRenderer.SetPositions(verticies);
        //     lineRenderer.startColor = Color.red;
        //     lineRenderer.endColor = Color.red;
        // }
        sliderValue = slider.value;
        int finalVerticiesAmount = (int)((sliderValue/heightScaleFactor) * verticiesAmount) + verticiesAmount;
        finalVerticiesAmount = (finalVerticiesAmount % 2 == 0) ? finalVerticiesAmount + 1 : finalVerticiesAmount;

        verticies = new Vector3[finalVerticiesAmount];
        verticies[0] = point1.position;
        verticies[finalVerticiesAmount - 1] = point2.position;
        float temp = -0.01f * sliderValue * height + height;
        for (int i = 1; i < finalVerticiesAmount - 1; i++) {
            Vector3 newPoint = point1.position + i * ((point2.position - point1.position)/(finalVerticiesAmount - 1));
            temp = temp * -1;
            verticies[i] = new Vector3(newPoint.x, newPoint.y + temp, 0);
        }
        lineRenderer.positionCount = finalVerticiesAmount;
        lineRenderer.SetPositions(verticies);

    }
}
