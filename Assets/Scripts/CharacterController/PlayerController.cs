using UnityEngine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private float groundCheckDistance = 1.1f;
    [SerializeField] private float MovementSpeed = 1.1f;
    [SerializeField] private float MaxMovementSpeedSpeed = 1.1f;
    [SerializeField] private float JumpForce = 1.1f;
    [SerializeField] private bool canJump = false;

    private Vector2 currentMovementVector;
    private float currentSpeed;
    
    private Collider2D collider2D;
    private Rigidbody2D rigidbody2D;
    private UnityEngine.InputSystem.PlayerInput playerInput;
    private InputAction moveAction, jumpAction;
    private Vector2 currentVector;
 
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
        rigidbody2D.AddForce(new Vector2(Mathf.Clamp(currentSpeed, -MaxMovementSpeedSpeed, MaxMovementSpeedSpeed), 0));
        Camera.main.transform.position = transform.position;
    }
    
    private void OnJumpAction(InputAction.CallbackContext context) 
    {
        if(IsGrounded())
            rigidbody2D.AddForce(Vector2.up * JumpForce * 10f);
    }

    private bool IsGrounded() { return Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, ~LayerMask.GetMask("Player", "Ignore Raycast")); }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Spikes")) 
        {
            Debug.Log("Player hit spikes");
            GameManager.Instance.RestartLevel();
        }
    }
}
