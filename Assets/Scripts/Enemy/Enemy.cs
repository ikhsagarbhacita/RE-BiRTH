using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected SpriteRenderer sr;          
    protected Transform player;           
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Collider2D[] colliders;


    [Header("Genaral Info")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float idleDuration = 1.5f;
    protected float idleTimer;
    protected bool canMove = true;


    [Header("Health & Damage")]          
    [SerializeField] protected int maxHealth = 3;
    [SerializeField] protected int contactDamage = 1;     // damage kalau badan menyentuh player (0 = nonaktif)
    [SerializeField] protected float hurtStunDuration = 0.2f;
    protected int currentHealth;
    protected float stunTimer;
    protected bool IsStunned => stunTimer > 0f;
    private Coroutine flashRoutine;
    private Color originalColor = Color.white;

    public event System.Action<int, int> OnHealthChanged; // (current, max) -> untuk health bar boss
    public event System.Action OnDied;


    [Header("Death Details")]
    [SerializeField] protected float deathImpactSpeed = 5f;
    [SerializeField] protected float deathRotationSpeed = 150f;
    protected int deathRotationDirection = 1;
    protected bool isDead;


    [Header("Collision Detector")]
    [SerializeField] protected float groundCheckDistance = 1.1f;
    [SerializeField] protected float wallCheckDistance = 0.7f;
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected float playerDetectionDistance = 15f;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected Transform groundCheck;
    protected bool isPlayerDetected;
    protected bool isGrounded;
    protected bool isWalled;
    protected bool isGroundInfrontDetected;

    protected float isFacingDirection = -1f;
    protected bool isFacingRight = false;

    private readonly HashSet<string> animParams = new HashSet<string>();
    private float findPlayerTimer;

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        colliders = GetComponentsInChildren<Collider2D>();

        currentHealth = maxHealth;
        if (sr != null) originalColor = sr.color;

        // No spam warning kalau parameternya belum dibuat
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            foreach (var p in anim.parameters)
                animParams.Add(p.name);
        }
    }

    protected virtual void Start()
    {
        if (sr != null && sr.flipX == true && !isFacingRight)
        {
            sr.flipX = false;
            Flip();
        }
    }

    protected virtual void Update()
    {
        HandleCollisions();
        HandleAnimator();

        idleTimer -= Time.deltaTime; // Decrease the value
        stunTimer -= Time.deltaTime;

        if (isDead)
            HandleDeathRotation();
    }

    // Return true kalau player ada.
    protected bool HasPlayer()
    {
        // Player sedang mati (menunggu respawn) -> musuh anggap tidak ada target
        if (PlayerManager.instance != null && !PlayerManager.instance.IsPlayerAlive)
            return false;

        if (player != null) return true;

        findPlayerTimer -= Time.deltaTime;
        if (findPlayerTimer > 0f) return false;

        findPlayerTimer = 0.5f;

        if (PlayerManager.instance != null && PlayerManager.instance.PlayerTransform != null)
        {
            player = PlayerManager.instance.PlayerTransform;
        }
        else
        {
            PlayerHealth p = FindFirstObjectByType<PlayerHealth>();
            if (p != null) player = p.transform;
        }

        return player != null;
    }

    protected void SetVelocityX(float x)
    {
        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
    }

    protected void Trigger(string triggerName)
    {
        if (animParams.Contains(triggerName)) anim.SetTrigger(triggerName);
    }

    // Enemy AI Patrol (Basic)
    protected void PatrolMove()
    {
        if (!isGrounded) return;

        if (idleTimer > 0f)
        {
            SetVelocityX(0);
            return;
        }

        if (isWalled || !isGroundInfrontDetected)
        {
            idleTimer = idleDuration;
            SetVelocityX(0);
            Flip();
            return;
        }

        SetVelocityX(moveSpeed * isFacingDirection);
    }

    // Damage
    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        stunTimer = hurtStunDuration;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(HitFlash());

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator HitFlash()
    {
        if (sr != null) sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (sr != null) sr.color = originalColor;
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead || contactDamage <= 0) return;

        PlayerHealth p = collision.collider.GetComponentInParent<PlayerHealth>();
        if (p != null)
            p.TakeDamage(contactDamage, transform.position.x);
    }

    public virtual void Die()
    {
        if (isDead) return;

        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        Trigger("hit");
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, deathImpactSpeed);
        isDead = true;

        if (Random.Range(0, 100) < 50)
            deathRotationDirection = deathRotationDirection * -1;

        OnDied?.Invoke();
        Destroy(gameObject, 10);
    }


    private void HandleDeathRotation()
    {
        transform.Rotate(0, 0, (deathRotationSpeed * deathRotationDirection) * Time.deltaTime);
    }

    protected virtual void HandleFlip(float xValue)
    {
        if (xValue < transform.position.x && isFacingRight || xValue > transform.position.x && !isFacingRight)
            Flip();
    }

    protected virtual void Flip()
    {
        isFacingDirection *= -1; // Change the facing direction multiplier
        transform.Rotate(0f, 180f, 0f); // Rotate the player 180 degrees around the Y-axis
        isFacingRight = !isFacingRight;
    }

    [ContextMenu("Change Facing Direction")]
    public void FlipDefaultFacingDirection()
    {
        if (sr != null) sr.flipX = !sr.flipX;
    }

    protected virtual void HandleAnimator()
    {
        if (anim != null && animParams.Contains("xVelocity"))
            anim.SetFloat("xVelocity", rb.linearVelocity.x);
    }

    protected virtual void HandleCollisions()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer); // 
        isGroundInfrontDetected = groundCheck != null && Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer); // Check if the player is grounded by casting a ray downwards
        isWalled = Physics2D.Raycast(transform.position, Vector2.right * isFacingDirection, wallCheckDistance, groundLayer); // Check if the player is against a wall by casting a ray in the direction the player is facing
        isPlayerDetected = Physics2D.Raycast(transform.position, Vector2.right * isFacingDirection, playerDetectionDistance, playerLayer); // 
    }

    // Draw a line in the Scene View to visualize the ground check distance
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        if (groundCheck != null)
            Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * isFacingDirection), transform.position.y));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (playerDetectionDistance * isFacingDirection), transform.position.y));
    }
}
