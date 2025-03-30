using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private float groundCheckDistance = 1.1f;
    [SerializeField] private float MovementSpeed = 1.1f;
    [SerializeField] private float MaxMovementSpeed = 1.1f;
    [SerializeField] private float JumpForce = 1.1f;
    [SerializeField] private bool canJump = false;

    private Vector2 currentMovementVector;
    private float currentSpeed;

    private Collider2D collider2D;
    private Rigidbody2D rigidbody2D;
    private UnityEngine.InputSystem.PlayerInput playerInput;
    private InputAction moveAction, jumpAction;
    private Vector2 currentVector;

    [SerializeField] float defaultGravityScale = 1f;  // Default gravity scale

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();
        playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>(); 
        moveAction = playerInput.actions.FindAction("Move");
        jumpAction = playerInput.actions.FindAction("Jump");  
        
        moveAction.Enable(); 
        if(canJump)
        {
            jumpAction.Enable();
            jumpAction.performed += OnJumpAction;
        }
    }

    void Update()
    {
        currentMovementVector = moveAction.ReadValue<Vector2>(); 
        currentSpeed = currentMovementVector.x * MovementSpeed * 10f; 

        // Update the rigidbody's velocity instead of using AddForce for smoother movement
        rigidbody2D.velocity = new Vector2(Mathf.Clamp(currentSpeed, -MaxMovementSpeed, MaxMovementSpeed), rigidbody2D.velocity.y);
        
        // Turn gravity off if the player is grounded and standing still
        if (IsGrounded() && Mathf.Abs(rigidbody2D.velocity.x) < 0.1f && Mathf.Abs(rigidbody2D.velocity.y) < 0.1f)
        {
            rigidbody2D.gravityScale = 0; // Set gravity scale to 0 when standing still on the ground
        }
        else
        {
            rigidbody2D.gravityScale = defaultGravityScale; // Reset gravity scale to default when moving or jumping
        }

        Camera.main.transform.position = transform.position;
    }

    private void OnJumpAction(InputAction.CallbackContext context) 
    {
        if (IsGrounded())
        {
            rigidbody2D.AddForce(Vector2.up * JumpForce * 10f);
            rigidbody2D.gravityScale = defaultGravityScale; // Ensure gravity is enabled when jumping
        }
    }

    private bool IsGrounded() 
    { 
        return Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, ~LayerMask.GetMask("Player", "Ignore Raycast")); 
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Spikes")) 
        {
            Debug.Log("Player hit spikes");
            GameManager.Instance.RestartLevel();
        }
    }
}
