using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private float groundCheckDistance = 1.1f;
    [SerializeField] private float MovementSpeed = 1.1f;
    [SerializeField] private float MaxMovementSpeedSpeed = 1.1f;
    [SerializeField] private float JumpForce = 1.1f;

    private Vector2 currentMovementVector;
    private float currentSpeed;
    
    private Collider2D collider2D;
    private Rigidbody2D rigidbody2D;
    private InputActionAsset inputActions;
    private InputAction moveAction, jumpAction;
    private Vector2 currentVector;

    void InializeActions()
    {
        inputActions = GetComponent<InputActionAsset>();
        moveAction = inputActions.FindAction("Move");
        moveAction.Enable();
        jumpAction = inputActions.FindAction("Jump");
        jumpAction.Enable(); 
        jumpAction.performed += OnJumpAction;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InializeActions();

    }

    // Update is called once per frame
    void Update()
    {
        currentMovementVector = moveAction.ReadValue<Vector2>(); 
        currentSpeed = currentMovementVector.x * MovementSpeed * 10f; 
        rigidbody2D.AddForce(new Vector2(Mathf.Clamp(currentSpeed, -MaxMovementSpeedSpeed, MaxMovementSpeedSpeed), 0));
    }
    
    private void OnJumpAction(InputAction.CallbackContext context) 
    {
        if(IsGrounded())
            rigidbody2D.AddForce(Vector2.up * JumpForce * 10f);
    }

    private bool IsGrounded() { return Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, ~LayerMask.GetMask("Player", "Ignore Raycast")); }
}
