using System;
using System.Collections;
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
    [SerializeField] private float GreenBouncePadPower = 50f; 
    
    private Vector2 currentMovementVector;
    private float currentSpeed, currentJumpCharge = 0, currentJumpDirection = 0;
    private bool isCharging = false; 
    
    [SerializeField]private bool noGravity = false;

    private Collider2D collider2D;
    private Rigidbody2D rigidbody2D;
    private UnityEngine.InputSystem.PlayerInput playerInput;
    private Animator animator;
    private InputAction moveAction;
    private Vector2 currentVector;
    private Vector2 noGravityVelocity;
    Vector2 lastPosition;
    Vector2 currentVelocity;
    private float lastDirection;

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();
        playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        animator = GetComponent<Animator>();
        moveAction = playerInput.actions.FindAction("Move");   
        moveAction.Enable();
        lastPosition = transform.position;    
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
                bool isMoving = currentMovementVector.x != 0;
                animator.SetBool("isMoving", isMoving); 
                if(currentMovementVector.x != 0)
                    lastDirection = currentMovementVector.x; 
                float direction = (lastDirection > 0) ? 0 : 180;
                transform.rotation = Quaternion.Euler(0f, direction, 0f);
            }
            else
            {
                #region Jumping 
                isCharging = currentMovementVector.x != 0;   
                if(IsGrounded())
                {
                    if (isCharging)
                    {
                        animator.SetBool("Charge", true);
                        animator.SetBool("Jump", false);
                        currentJumpDirection = currentMovementVector.x;
                        currentJumpCharge += chargeRate * Time.deltaTime;
                        currentJumpCharge = Mathf.Clamp(currentJumpCharge, 0f, maxCharge);  
                        if(currentMovementVector.x != 0)
                            lastDirection = currentMovementVector.x; 
                        float direction = (lastDirection > 0) ? 0 : 180;
                        transform.rotation = Quaternion.Euler(0f, direction, 0f);
                    }
                    else
                    {
                        animator.SetBool("Jump", true);
                        animator.SetBool("Charge", false);
                        float chargePercent = currentJumpCharge / maxCharge;
                        Vector2 force = new Vector2(currentJumpDirection * jumpForceHorizontal * chargePercent,
                            jumpForceVertical * chargePercent);
                        rigidbody2D.AddForce(force, ForceMode2D.Impulse);
                        currentJumpDirection = 0f;
                        currentJumpCharge = 0f; 
                    }
                    animator.SetBool("isGrounded", IsGrounded()); 
                } 
                #endregion
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
        } 
         
        
        float dir = (lastDirection > 0) ? 0 : 180;
        transform.rotation = Quaternion.Euler(0f, dir, 0f);

        if (Camera.main != null)
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
        if(other.gameObject.CompareTag("Bounce"))
        { 
            rigidbody2D.AddForce(transform.up * GreenBouncePadPower * 10, ForceMode2D.Impulse); 
        } 
        if (other.gameObject.CompareTag("Danger")) 
        { 
            StartCoroutine(DeathScene()); 
        }
        else{
            if (rb != null)
            {
                // Vector3 direction = (other.transform.position - transform.position).normalized;
                // Vector2 forceProject = rb.linearVelocity * rb.mass; 
                // rigidbody2D.AddForce(direction * forceProject, ForceMode2D.Impulse); 
            } 
        }  
    }

    private void OnCollisionStay(Collision other)
    {
        if(other.gameObject.CompareTag("Bounce"))
        { 
            rigidbody2D.AddForce(transform.up * GreenBouncePadPower * 10, ForceMode2D.Impulse); 
        } 

    }

    IEnumerator DeathScene()
    { 
        animator.Play("Death"); 
        yield return new WaitForSeconds(0.25f); 
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll; 
        GameManager.Instance.RestartLevel();
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Pedestal"))
        {
            animator.SetTrigger("Interact");
            string pedestalName = other.gameObject.name;
            PieMenuManager.Instance.HandleAddingItem(pedestalName);
            Destroy(other.GetComponent<BoxCollider2D>());
        }
        if (other.CompareTag("AdditionPedestal"))
        {
            string pedestalName = other.gameObject.name;
            GameManager.Instance.AddSlider(pedestalName);
        }
        if (other.CompareTag("Trigger"))
        {
            Trigger trigger = other.gameObject.GetComponent<Trigger>();
            GameManager.Instance.TransitionExternal(trigger.postProcessingEffect, trigger.setting, 0.5f);
        }
        if (other.CompareTag("BossNextLevel"))
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(nextSceneIndex);
        }
        if (other.gameObject.CompareTag("DeathBox") || other.gameObject.CompareTag("Spikes")) 
        { 
            StartCoroutine(DeathScene()); 
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("DeathBox") || other.gameObject.CompareTag("Spikes")) 
        { 
            StartCoroutine(DeathScene()); 
        }
    }

    #region Helper Methods
    private bool IsGrounded() 
    { 
        if (noGravity) return false;
        return Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, ~LayerMask.GetMask("Player", "Ignore Raycast")); 
    }

    #endregion
    #region Public Methods
    public void SetGravityStatus(bool status){noGravity = status; rigidbody2D.gravityScale = 0;}
    public void SetJumpStatus(bool status){canJump = status; animator.SetBool("Jump", false);
        currentJumpCharge = 0;
    }

    #endregion
}
