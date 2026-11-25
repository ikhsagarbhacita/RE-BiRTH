using UnityEngine;

public class DeadZone : MonoBehaviour
{
    // DeadZone: area di bawah map, player jatuh ke sini bakal mati
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth player =
            other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            player.Kill();
        }
    }
}