using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BoxMovement : MonoBehaviour
{
    [Header("Platform timing")]
    public float travelTime = 1f;      // seconds to go from A→B or B→A
    public float waitTime   = 2f;      // pause at each end

    [Header("Waypoints (must be exactly 2)")]
    public List<Vector2> targetPositions;

    [Header("Detector (optional)")]
    public GameObject detectorPair;
    
    private Rigidbody2D _rb;
    private Detector   _detector;
    private Animator   _animator;

    void Awake()
    {
        // 1) Grab & configure Rigidbody2D
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType           = RigidbodyType2D.Kinematic;
        _rb.gravityScale       = 0f;
        _rb.linearDamping               = 0f;
        _rb.angularDamping        = 0f;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // 2) Detector/animator (optional)
        if (detectorPair != null)
        {
            _detector = detectorPair.GetComponent<Detector>();
            _animator = detectorPair.GetComponent<Animator>();
        }
    }

    void Start()
    {
        if (targetPositions.Count != 2)
        {
            Debug.LogError("BoxMovement requires exactly 2 targetPositions!");
            enabled = false;
            return;
        }

        // jump straight to the second point and start moving
        _rb.position = targetPositions[1];
    }

    public IEnumerator movePlatform()
    { 
        if(_animator == null)
        {
            Debug.LogError("BoxMovement requires a animator!");
            yield break;
        }
        
        _animator.SetBool("IsDown", true);
        _detector.isSet = true;
        
        // --- Move B→A ---
        yield return MoveBetween(targetPositions[1], targetPositions[0]);
        
        yield return new WaitForSeconds(waitTime);
        
        // --- Move A→B ---
        yield return MoveBetween(targetPositions[0], targetPositions[1]);
        
        if (_animator) _animator.SetBool("IsDown", false);
        if (_detector) _detector.isSet = false; 
    }

    IEnumerator MoveBetween(Vector2 from, Vector2 to)
    {
        float elapsed = 0f;
        while (elapsed < travelTime)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);
            Vector2 nextPos = Vector2.Lerp(from, to, t);
            
            // Kinematic MovePosition so physics “sees” the platform moving
            _rb.MovePosition(nextPos);
            
            yield return new WaitForFixedUpdate();
        }
        // ensure exact end
        _rb.MovePosition(to);
    }
}
