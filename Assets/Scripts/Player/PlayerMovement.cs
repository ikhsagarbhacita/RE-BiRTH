using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float coyoteTime = 0.15f;


    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundLayer;


    public float MoveInput { get; private set; }
    public float Speed => speed;
    public bool isGround { get; private set; }


    private bool wasGround;
    private float coyoteTimeCounter;
    private InputSystem_Actions actions;
    private Rigidbody2D rb;
    private int maxJump = 2;

    private void Awake()
    {
        actions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();
        
    }

    private void OnEnable()
    {
        actions.Player.Enable();
        actions.Player.Jump.performed += Jumping;
    }
    private void OnDisable()
    {
        actions.Player.Jump.performed -= Jumping;
        actions.Player.Disable();
    }

    private void Update()
    {
        MoveInput = actions.Player.Move.ReadValue<Vector2>().x;
        GroundState();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(MoveInput * speed, rb.linearVelocityY);
    }

    private void Jumping(InputAction.CallbackContext ctx)
    {

        if(coyoteTimeCounter > 0f || maxJump > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
            maxJump--;
            coyoteTimeCounter = 0f;
        }
        
    }

    private void GroundState()
    {
        isGround = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundLayer);

        if(isGround && !wasGround)
        {
            maxJump = 2;
        }
        if (isGround)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        wasGround = isGround;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
    }

}
