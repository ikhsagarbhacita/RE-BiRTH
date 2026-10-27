using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float cameraSpeed;
    [SerializeField] private Vector3 offset = new Vector3(0, 1, -10f);

    private Vector3 velocity = Vector3.zero;
    private void LateUpdate()
    {
        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position, targetPos, ref velocity, cameraSpeed);

    }
}
