using UnityEngine;

public class ZeroGTest : MonoBehaviour
{
    Rigidbody2D _rb;
    Collider  _col;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider>();

        // Zero gravity & drag
        _rb.gravityScale  = 0;
        _rb.linearDamping        = 0f;
        _rb.angularDamping = 0f;
    }

        void OnCollisionEnter2D(Collision2D col)
    {
        // reflect velocity about the contact normal
        Vector2 n = col.contacts[0].normal;
        _rb.linearVelocity = Vector2.Reflect(_rb.linearVelocity, n);
    }
}
