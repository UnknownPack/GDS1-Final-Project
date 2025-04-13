using System;
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
    [SerializeField] float defaultGravityScale = 1f; 

    [Header("JumpStats")]
    [SerializeField] private float chargeRate = 2.5f;
    [SerializeField] private float maxCharge = 50f;
    [SerializeField] private float jumpForceVertical = 50f;
    [SerializeField] private float jumpForceHorizontal = 25f; 
    
    private Vector2 currentMovementVector;
    private float currentSpeed, currentJumpCharge = 0, currentJumpDirection = 0;
    private bool isCharging = false; 
    
    [SerializeField]private bool noGravity = false;

    private Collider2D collider2D;
    private Rigidbody2D rigidbody2D;
    private UnityEngine.InputSystem.PlayerInput playerInput;
    private InputAction moveAction;
    private Vector2 currentVector; 

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();
        playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>(); 
        moveAction = playerInput.actions.FindAction("Move");   
        moveAction.Enable();    
    } 
    void Update()
    { 
        currentMovementVector = moveAction.ReadValue<Vector2>();
        if (!noGravity)
        {
            if (!canJump)
            { 
                currentSpeed = currentMovementVector.x * MovementSpeed * 10f; 
                rigidbody2D.linearVelocity = new Vector2(Mathf.Clamp(currentSpeed, -MaxMovementSpeed, MaxMovementSpeed),rigidbody2D.linearVelocity.y);
            }
            else
            { 
                isCharging = currentMovementVector.x != 0;   
                if (isCharging)
                { 
                    currentJumpDirection = currentMovementVector.x;
                    currentJumpCharge += chargeRate * Time.deltaTime;
                    currentJumpCharge = Mathf.Clamp(currentJumpCharge, 0f, maxCharge); 
                }
                else
                {
                    float chargePercent = currentJumpCharge / maxCharge;
                    Vector2 force = new Vector2(currentJumpDirection * jumpForceHorizontal * chargePercent,
                        jumpForceVertical * chargePercent);
                    rigidbody2D.AddForce(force, ForceMode2D.Impulse);
                    currentJumpDirection = 0f;
                    currentJumpCharge = 0f;
                }  
            } 
        }
         
        bool isIdle = Mathf.Abs(rigidbody2D.linearVelocity.x) < 0.1f && Mathf.Abs(rigidbody2D.linearVelocity.y) < 0.1f;
        if ((IsGrounded() && isIdle) || noGravity)
        {
            rigidbody2D.gravityScale = 0; // Set gravity scale to 0 when standing still on the ground
        }
        else
        {
            rigidbody2D.gravityScale = defaultGravityScale; // Reset gravity scale to default when moving or jumping
        }

        if (Camera.main != null)
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        
        Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
        if(other.gameObject.CompareTag("Ground"))
        { 
            if (rb != null)
            {
                Vector3 direction = (other.transform.position - transform.position).normalized; 
                rigidbody2D.AddForce(direction * jumpForceVertical * 10, ForceMode2D.Impulse); 
            }
        }
        else{
                    if (rb != null)
                    {
                        Vector3 direction = (other.transform.position - transform.position).normalized;
                        Vector2 forceProject = rb.linearVelocity * rb.mass;
                        rigidbody2D.AddForce(direction * forceProject, ForceMode2D.Impulse); 
                    }
            }
        
             
 
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Spikes")) 
        {
            Debug.Log("Player hit spikes");
            GameManager.Instance.RestartLevel();
        }

        if (other.CompareTag("Pedestal"))
        {
            string pedestalName = other.gameObject.name;
            PieMenuManager.Instance.HandleAddingItem(pedestalName);
            Destroy(other.GetComponent<BoxCollider2D>());
        }
        if (other.CompareTag("Trigger"))
        {
            Trigger trigger = other.gameObject.GetComponent<Trigger>();
            GameManager.Instance.TransitionExternal(trigger.postProcessingEffect, trigger.setting);
        }
        if (other.CompareTag("DeathBox"))
        {
            Debug.Log("Player hit the deathbox");
            GameManager.Instance.RestartLevel();
        }
    }

    #region Helper Methods
    private bool IsGrounded() 
    { 
        return Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, ~LayerMask.GetMask("Player", "Ignore Raycast")); 
    }

    #endregion
    #region Public Methods
    public void SetGravityStatus(bool status){noGravity = status;}
    public void SetJumpStatus(bool status){canJump = status;}

    #endregion
}
