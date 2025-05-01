using UnityEngine;

public class MoveTest : MonoBehaviour
{
    Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            rb.AddForce(Vector2.up * 10f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            rb.AddForce(Vector2.down * 10f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(Vector2.left * 10f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(Vector2.right * 10f);
        }
    }
}
