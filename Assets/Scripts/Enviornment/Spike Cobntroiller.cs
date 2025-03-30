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
    private float sliderValue;
    private Transform point1;
    private Transform point2;
    private BoxCollider2D boxCollider2D;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        point1 = gameObject.transform.GetChild(0);
        point2 = gameObject.transform.GetChild(1);
        UpdateVerticies(0);
        boxCollider2D.offset = (point1.transform.position + point2.transform.position) / 2f;
        boxCollider2D.size = new Vector2(Vector2.Distance(point1.transform.position, point2.transform.position), height * 2);
        boxCollider2D.isTrigger = true;
        
    }

    // Update is called once per frame
    void Update()
    {   
        
    }

    public void UpdateVerticies(float value) {

        sliderValue = value;
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
        temp = Mathf.Max(Math.Abs(temp), 0.01f);
        boxCollider2D.size = new Vector2(Vector2.Distance(point1.transform.position, point2.transform.position), temp * 2);
        boxCollider2D.isTrigger = value <= 95;
    }
}
