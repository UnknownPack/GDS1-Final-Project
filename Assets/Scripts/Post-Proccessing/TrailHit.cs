using UnityEngine;

public class TrailHit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(new Vector2(100f, 0f));
            }
        }
        else if (other.CompareTag("MovingPlatform"))
        {
            Debug.Log("interactive with blue block");
        }
    }

}
