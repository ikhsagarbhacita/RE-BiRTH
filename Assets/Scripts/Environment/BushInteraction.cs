using System.Collections;
using UnityEngine;

public class BushInteraction : MonoBehaviour
{
    [SerializeField] private Transform bushVisual;
    [SerializeField] private ParticleSystem leafParticles;
    [SerializeField] private float shakeDuration = 0.4f;
    [SerializeField] private float shakeStrength = 8f;
    [SerializeField] private float shakeFrequency = 25f;

    private Quaternion originalRotation;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        originalRotation = bushVisual.localRotation;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() == null)
            return;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(Shake());

        if (leafParticles != null)
            leafParticles.Emit(8);
    }

    private IEnumerator Shake()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float damping = 1f -
                Mathf.Clamp01(elapsed / shakeDuration);

            float angle =
                Mathf.Sin(elapsed * shakeFrequency)
                * shakeStrength * damping;

            bushVisual.localRotation =
                originalRotation *
                Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        bushVisual.localRotation = originalRotation;
        shakeRoutine = null;
    }
}