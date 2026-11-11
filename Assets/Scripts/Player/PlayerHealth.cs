using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibleDuration = 1f;

    [Header("Knockback")]
    [SerializeField] private Vector2 knockbackForce = new Vector2(6f, 8f);
    [SerializeField] private float knockbackDuration = 0.3f;

    [Header("VFX")]
    [SerializeField] private GameObject deathVFX;
    [SerializeField] private float respawnInvincibleDuration = 1.5f;

    public event Action<int, int> OnHealthChanged; // (current, max) -> untuk HUD
    public event Action OnDied;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead { get; private set; }

    private PlayerMovement movement;
    private PlayerShooting shooting;
    private PlayerAim aim;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private bool isInvincible;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        shooting = GetComponent<PlayerShooting>();
        aim = GetComponent<PlayerAim>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        CurrentHealth = maxHealth;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth); 
    }

    public void TakeDamage(int damage, float sourceX)
    {
        if (IsDead || isInvincible) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CameraMovement.instance != null)
            CameraMovement.instance.Shake(0.15f, 0.15f);

        if (CurrentHealth <= 0)
        {
            Die();
            return;
        }

        float dir = transform.position.x < sourceX ? -1f : 1f;
        movement.ApplyKnockback(new Vector2(knockbackForce.x * dir, knockbackForce.y), knockbackDuration);

        StartCoroutine(InvincibleRoutine(invincibleDuration));
    }

    public void Heal(int amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private IEnumerator InvincibleRoutine(float duration)
    {
        isInvincible = true;

        float t = 0f;
        while (t < duration)
        {
            sprite.enabled = !sprite.enabled; // efek berkedip
            yield return new WaitForSeconds(0.1f);
            t += 0.1f;
        }

        sprite.enabled = true;
        isInvincible = false;
    }

    private void Die()
    {
        IsDead = true;
        isInvincible = false;
        StopAllCoroutines();

        if (deathVFX != null) Instantiate(deathVFX, transform.position, Quaternion.identity);

        sprite.enabled = false;
        SetControl(false);
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;   

        OnDied?.Invoke();
    }

    public void Respawn(Vector3 position)
    {
        transform.position = position;

        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;

        IsDead = false;
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        sprite.enabled = true;
        SetControl(true);

        StartCoroutine(InvincibleRoutine(respawnInvincibleDuration));
    }

    private void SetControl(bool value)
    {
        movement.enabled = value;
        if (shooting != null) shooting.enabled = value;
        if (aim != null) aim.enabled = value;
    }
}