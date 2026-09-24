using UnityEngine;
using UnityEngine.InputSystem;
public class Player : MonoBehaviour

{

    [Header("Movement")]
    public float speed = 10;
    public float jumpForce = 20;
    public float coyoteTime = 0.15f;
    public int maxJump = 2;

    [Header("Ground Check")]
    public Transform groundCheckTransform; // posisi dari GroundCheck
    public float groundCheckRadius;  //ukuran dari ground check
    public LayerMask groundLayer; //layer ground

    private InputSystem_Actions actions;
    private int jumpRemaining;

    bool isGrounded;
    bool wasGrounded;
    float move;
    float coyoteTimeCounter;
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        actions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();                  
        anim = GetComponent<Animator>();                   
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        actions.Player.Enable();
        actions.Player.Move.performed += Movement;
        actions.Player.Jump.performed += Jumping;

        actions.Player.Move.canceled += Movement;
    }

    private void OnDisable()
    {
        actions.Player.Disable();
        actions.Player.Move.performed -= Movement;
        actions.Player.Jump.performed -= Jumping;

        actions.Player.Move.canceled -= Movement;
    }

    void Movement(InputAction.CallbackContext ctx)
    {
        move = ctx.ReadValue<Vector2>().x;
    }

    void Jumping(InputAction.CallbackContext ctx)
    {
        bool firstJumpAllowed = coyoteTimeCounter > 0f;
        bool extraJumpAllowed = jumpRemaining < maxJump;

        if (ctx.performed && jumpRemaining > 0f && (firstJumpAllowed || extraJumpAllowed))
        {
            rb.linearVelocityY = jumpForce;
            coyoteTimeCounter = 0f;
            jumpRemaining--;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 5;
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            jumpRemaining = maxJump;
        }

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        wasGrounded = isGrounded;
        rb.linearVelocityX = move * speed;

        UpdateAnimation();
    }
    void UpdateAnimation()
    {
        
        bool isRunning = move != 0;
        anim.SetBool("isRunning", isRunning);
        anim.SetFloat("animSpeed", speed / 5f);

        
        anim.SetBool("isJumping", !isGrounded);

        
        if (move > 0)
        {
            spriteRenderer.flipX = false;   
        }
        else if (move < 0)
        {
            spriteRenderer.flipX = true;    
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
    }

}