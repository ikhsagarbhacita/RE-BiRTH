using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [Header("Aim")]
    [SerializeField] private Transform aimVisual;
    [SerializeField] private float aimRadius = 2f;

    public Vector2 clampedOffset;

    private void Update()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = mouseWorldPos - (Vector2)transform.position;

        clampedOffset = Vector2.ClampMagnitude(direction, aimRadius);

        aimVisual.position = (Vector2)transform.position + clampedOffset;
    }

}
