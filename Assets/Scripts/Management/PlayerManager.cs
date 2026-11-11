using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static event Action OnPlayerRespawn;
    public static event Action OnPlayerDeath;

    public static PlayerManager instance;

    public enum DeathBehavior { RespawnAtCheckpoint, RestartLevel }

    [Header("Player")]
    [SerializeField] private PlayerHealth player;
    [SerializeField] private Transform respawnPoint; 

    [Header("Death")]
    [SerializeField] private DeathBehavior onDeath = DeathBehavior.RespawnAtCheckpoint;
    [SerializeField] private float respawnDelay = 2f;

    private Vector3 respawnPosition;

    public PlayerHealth Health => player;
    public Transform PlayerTransform => player != null ? player.transform : null;
    public bool IsPlayerAlive => player != null && !player.IsDead;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (player == null)
            player = FindFirstObjectByType<PlayerHealth>();

        if (player != null)
            respawnPosition = respawnPoint != null ? respawnPoint.position : player.transform.position;
    }

    private void OnEnable()
    {
        if (player != null) player.OnDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        if (player != null) player.OnDied -= HandlePlayerDied;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    // Dipanggil Checkpoint
    public void UpdateRespawnPosition(Transform newRespawnPoint) => respawnPosition = newRespawnPoint.position;

    private void HandlePlayerDied()
    {
        OnPlayerDeath?.Invoke();
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (onDeath == DeathBehavior.RestartLevel)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            yield break;
        }

        player.Respawn(respawnPosition);

        if (CameraMovement.instance != null)
            CameraMovement.instance.Snap();

        OnPlayerRespawn?.Invoke();
    }
}
