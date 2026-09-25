using UnityEngine;
using UnityEngine.InputSystem;
public class Player : MonoBehaviour

{

    [Header("Movement")]
    public float speed = 10;
    public float jumpForce = 20;
    public float coyoteTime = 0.15f;
    public int maxJump = 2;

    [Header("Aim & Shot")]
    public Transform aimCursorVisual;
    public float aimRadius = 3f;
    public float fireRate = 0.2f;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;

    [Header("Ground Check")]
    public Transform groundCheckTransform; // posisi dari GroundCheck
    public float groundCheckRadius;  //ukuran dari ground check
    public LayerMask groundLayer; //layer ground

    private float fireCooldown;
    private InputSystem_Actions actions;
    private int jumpRemaining;
    private Vector2 clampedOffset;

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
        rb.gravityScale = 5;
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
        HandleAim();
        HandleShoting();
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

    private void HandleAim()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = mouseWorldPos - (Vector2)transform.position;

        clampedOffset = Vector2.ClampMagnitude(direction, aimRadius);

        aimCursorVisual.position = (Vector2)transform.position + clampedOffset;
    }

    private void HandleShoting()
    {
        fireCooldown -= Time.deltaTime;

        if(Mouse.current.leftButton.isPressed && fireCooldown < 0f)
        {
            Shoot();
            fireCooldown = fireRate;
        }
    }

    private void Shoot()
    {
        Vector2 shootDirection = clampedOffset.normalized;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = shootDirection * bulletSpeed;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
    }

}