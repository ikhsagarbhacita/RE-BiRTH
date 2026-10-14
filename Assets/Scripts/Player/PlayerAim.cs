using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [Header("Aim")]
    [SerializeField] private Transform aimVisual;
    [SerializeField] private Transform limitShootVisual;
    [SerializeField] private float aimRadius = 2f;
    [SerializeField] private float limitShoot = 10f;

    public Vector2 clampedOffset;
    public Vector2 limitOffset;

    private void Update()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = mouseWorldPos - (Vector2)transform.position;

        clampedOffset = Vector2.ClampMagnitude(direction, aimRadius);
        limitOffset = direction.normalized * limitShoot;

        aimVisual.position = (Vector2)transform.position + clampedOffset;
        limitShootVisual.position = (Vector2)transform.position + limitOffset;
    }

}
