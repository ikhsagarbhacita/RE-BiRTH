using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerVFX : MonoBehaviour
{
    [Header("Flying VFX")]
    [SerializeField] private TrailRenderer flightTrail;
    private PlayerMovement movement;
    private PlayerHealth health;
    private SpriteRenderer sprite;
    private bool wasFlying;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<PlayerHealth>();
        sprite = GetComponent<SpriteRenderer>();

        if (flightTrail != null && sprite != null)
        {
            flightTrail.sortingLayerID = sprite.sortingLayerID;
            flightTrail.sortingOrder = sprite.sortingOrder - 1;
        }
    }

    private void OnEnable()
    {
        ResetVFX();

        if (health != null)
            health.OnDied += ResetVFX;

        PlayerManager.OnPlayerRespawn += ResetVFX;
    }

    private void LateUpdate()
    {
        bool currentlyFlying =
            movement.isActiveAndEnabled &&
            movement.isFlying &&
            (health == null || !health.IsDead);

        if (currentlyFlying && !wasFlying)
        {
            StartFlightVFX();
        }
        else if (!currentlyFlying && wasFlying)
        {
            StopFlightVFX();
        }

        wasFlying = currentlyFlying;
    }

    private void StartFlightVFX()
    {
        if (flightTrail == null) return;

        flightTrail.Clear();
        flightTrail.emitting = true;
    }

    private void StopFlightVFX()
    {
        if (flightTrail == null) return;

        flightTrail.emitting = false;
    }

    private void ResetVFX()
    {
        wasFlying = false;

        if (flightTrail == null) return;

        flightTrail.emitting = false;
        flightTrail.Clear();
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDied -= ResetVFX;

        PlayerManager.OnPlayerRespawn -= ResetVFX;

        ResetVFX();
    }
}