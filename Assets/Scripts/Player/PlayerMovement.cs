using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;   

    [Header("Flying")]                                        
    [SerializeField] private float flyForce = 8f;             
    [SerializeField] private float flyDuration = 0.6f;        

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundLayer;

    [Header("VFX")]                                          
    [SerializeField] private ParticleSystem dustFX;


    public float MoveInput { get; private set; }
    public float Speed => speed;
    public bool isGround { get; private set; }
    public bool isMoving { get; private set; }
    public bool isFlying { get; private set; }               
    public bool IsKnockedBack => knockbackTimer > 0f;         
    private bool wasGround;
    private float coyoteTimeCounter;
    private InputSystem_Actions actions;
    private Rigidbody2D rb;
    private const int TotalJumps = 2;
    private int maxJump = 2;
    private float jumpBufferCounter;
    private float flyTimer;
    private float knockbackTimer;
    private bool canFly = true;

    private void Awake()
    {
        actions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();

    }

    private void OnEnable()
    {
        actions.Player.Enable();
        actions.Player.Jump.performed += Jumping;
        actions.Player.Sprint.performed += OnFlyPerformed;
        actions.Player.Sprint.canceled += OnFlyCanceled;
    }
    private void OnDisable()
    {
        actions.Player.Jump.performed -= Jumping;
        actions.Player.Sprint.performed -= OnFlyPerformed;
        actions.Player.Sprint.canceled -= OnFlyCanceled;
        actions.Player.Disable();

        StopFlying();    
        MoveInput = 0f;
        isMoving = false;
    }

    private void Update()
    {
        MoveInput = actions.Player.Move.ReadValue<Vector2>().x;
        isMoving = MoveInput != 0;

        knockbackTimer -= Time.deltaTime;
        GroundState();

        jumpBufferCounter -= Time.deltaTime;
        if (jumpBufferCounter > 0f) TryJump();
    }

    private void FixedUpdate()
    {
        Moving();
        HandleFlying(); 
    }

    private void Jumping(InputAction.CallbackContext ctx)
    {
        jumpBufferCounter = jumpBufferTime; 
        TryJump();                        
    }

    private void TryJump()
    {
        if (IsKnockedBack) return;

        bool firstJumpAllowed = coyoteTimeCounter > 0f;
        bool extraJumpAllowed = maxJump < TotalJumps;

        if (maxJump > 0 && (firstJumpAllowed || extraJumpAllowed))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
            maxJump--;
            coyoteTimeCounter = 0f;

            jumpBufferCounter = 0f;                
            if (isFlying) StopFlying();              
            if (dustFX != null) dustFX.Play();       
        }
    }

    private void Moving()
    {
        if (IsKnockedBack) return; 

        rb.linearVelocity = new Vector2(MoveInput * speed, rb.linearVelocityY);
    }

    private void GroundState()
    {
        isGround = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundLayer);

        if (isGround && !wasGround)
        {
            maxJump = TotalJumps;
            HandleLanding();
        }
        if (isGround)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // 
        if (!isGround && coyoteTimeCounter <= 0f && maxJump == TotalJumps)
        {
            maxJump = TotalJumps - 1;
        }

        wasGround = isGround;
    }

    private void HandleLanding()
    {
        canFly = true;
        flyTimer = 0f;
        if (isFlying) StopFlying();
        if (dustFX != null) dustFX.Play();
    }

    public void ApplyKnockback(Vector2 force, float duration)
    {
        StopFlying();
        knockbackTimer = duration;
        rb.linearVelocity = force;
    }

    #region Flying
    private void OnFlyPerformed(InputAction.CallbackContext ctx)
    {
        StartFlying();
    }

    private void OnFlyCanceled(InputAction.CallbackContext ctx)
    {
        StopFlying();
    }

    private void StartFlying()
    {
        if (IsKnockedBack || isGround || !canFly || isFlying) return;

        isFlying = true;
        if (dustFX != null && !dustFX.isPlaying) dustFX.Play();
    }

    private void StopFlying()
    {
        if (!isFlying) return;

        isFlying = false;
        if (dustFX != null && dustFX.isPlaying) dustFX.Stop();
    }

    private void HandleFlying()
    {
        if (!isFlying) return;

        // 
        rb.linearVelocity = new Vector2(rb.linearVelocityX, Mathf.Max(rb.linearVelocityY, flyForce));

        flyTimer += Time.fixedDeltaTime;
        if (flyTimer >= flyDuration)
        {
            canFly = false;
            StopFlying();
        }
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
    }

}