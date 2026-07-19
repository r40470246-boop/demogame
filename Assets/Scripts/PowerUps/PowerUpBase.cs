using UnityEngine;

/// <summary>
/// PowerUpBase — Saare power-ups ki base class
/// Isse inherit karke naye power-ups banao
/// </summary>
public abstract class PowerUpBase : MonoBehaviour
{
    [Header("Power-Up Settings")]
    public string powerUpName = "Power-Up";
    public float duration = 5f;            // Kitni der tak active rahe
    public float spawnLifetime = 15f;      // Map pe kitni der tak rahe
    public Color powerUpColor = Color.cyan;

    [Header("Visual")]
    public ParticleSystem collectEffect;
    public SpriteRenderer iconRenderer;

    private float spawnTime;

    protected virtual void Start()
    {
        spawnTime = Time.time;

        // Auto destroy after lifetime
        Destroy(gameObject, spawnLifetime);

        // Floating animation
        StartCoroutine(FloatAnimation());
    }

    private System.Collections.IEnumerator FloatAnimation()
    {
        Vector3 startPos = transform.position;
        float t = 0;

        while (true)
        {
            t += Time.deltaTime;
            transform.position = startPos + Vector3.up * Mathf.Sin(t * 2f) * 0.2f;
            transform.Rotate(0, 0, 90f * Time.deltaTime);
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("WormHead"))
        {
            Apply(other.gameObject);
        }
    }

    /// <summary>
    /// Power-up apply karo — child classes override karein
    /// </summary>
    public virtual void Apply(GameObject worm)
    {
        // Collect effect
        if (collectEffect != null)
        {
            collectEffect.transform.SetParent(null);
            collectEffect.Play();
            Destroy(collectEffect.gameObject, 2f);
        }

        // Screen shake ya flash effect
        CameraShake.Instance?.Shake(0.2f, 0.1f);

        Destroy(gameObject);
    }
}

// =============================
// Camera shake utility
// =============================
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(DoShake(duration, magnitude));
    }

    private System.Collections.IEnumerator DoShake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
