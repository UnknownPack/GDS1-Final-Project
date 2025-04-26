using System;
using UnityEngine;

public class Detector : MonoBehaviour
{
    public bool isSet = true;
    private Animator animator;
    public GameObject movingBoxPair;
    private BoxMovement boxMovement;
    private Vector3 lockedPosition;
    void Start()
    {
        animator = GetComponent<Animator>();
        boxMovement = movingBoxPair.GetComponent<BoxMovement>();
        transform.SetParent(null, true); 

    } 

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log(other.name); 
            if(isSet)
            { 
                StartCoroutine(boxMovement.movePlatform()); 
                isSet = false;
            }
        }
    }
}
