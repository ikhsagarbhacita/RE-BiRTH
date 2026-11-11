using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float cameraSpeed;
    [SerializeField] private Vector3 offset = new Vector3(0, 1, -10f);

    public static CameraMovement instance;

    private Vector3 velocity = Vector3.zero;
    private Vector3 basePosition;  // Posisi kamera tanpa getaran
    private float shakeTimer;
    private float shakeMagnitude;

    private void Awake()
    {
        instance = this;
        basePosition = transform.position;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + offset;
        basePosition = Vector3.SmoothDamp(basePosition, targetPos, ref velocity, cameraSpeed);

        Vector3 shake = Vector3.zero;
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            shake = (Vector3)(Random.insideUnitCircle * shakeMagnitude);
        }

        transform.position = basePosition + shake;
    }

    // Shake Screen ketika kena damage, serangan boss, dll
    public void Shake(float duration, float magnitude)
    {
        shakeTimer = duration;
        shakeMagnitude = magnitude;
    }

    // Move ke player (dipanggil saat respawn)
    public void Snap()
    {
        if (target == null) return;
        basePosition = target.position + offset;
        velocity = Vector3.zero;
        transform.position = basePosition;
    }
}