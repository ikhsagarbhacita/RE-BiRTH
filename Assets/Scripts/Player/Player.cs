using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Component References")]
    public float speed;
    public float jumpforce;
    private Rigidbody2D rb;
    private float InputX;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isGround = false;
    bool jump = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        InputX = Input.GetAxis("Horizontal");

        if (InputX != 0)
        {
            anim.SetBool("isRunning", true);
            anim.SetFloat("animSpeed", speed / 5f);
        }
        else
        {
            anim.SetBool("isRunning", false);
        }

        if (InputX > 0)
        {
            spriteRenderer.flipX = false; // Menghadap kanan
        }
        else if (InputX < 0)
        {
            spriteRenderer.flipX = true; // Menghadap kiri
        }

        if (isGround)
        {
            if (Input.GetButtonDown("Jump"))
            {
                jump = true;

            }
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(InputX * speed, rb.linearVelocity.y); 

        if (jump)
        {
            Jump();
            jump = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGround = true;
            anim.SetBool("isJumping", false);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGround = false;
        }
    }

    void Jump()
    {
        rb.AddForce(Vector2.up * jumpforce, ForceMode2D.Impulse);
        anim.SetBool("isJumping", true);
    }
}
