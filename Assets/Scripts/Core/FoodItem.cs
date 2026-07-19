using UnityEngine;
using Photon.Pun;

/// <summary>
/// FoodItem — Ek food particle jo map pe hoti hai
/// Worm se collect hoti hai aur value deti hai
/// </summary>
public class FoodItem : MonoBehaviour
{
    [Header("Food Settings")]
    public int value = 10;                  // Score value
    public float bobSpeed = 2f;            // Up-down animation speed
    public float bobAmount = 0.1f;         // Kitna bob kare

    [Header("Visual")]
    public Color[] possibleColors;         // Random colors
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private bool isCollected = false;

    // Magnetic pull ke liye
    private Transform magnetTarget = null;
    public float magnetSpeed = 8f;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;

        // Random color assign karo
        if (possibleColors != null && possibleColors.Length > 0)
        {
            spriteRenderer.color = possibleColors[Random.Range(0, possibleColors.Length)];
        }

        // Random scale variation
        float randomSize = Random.Range(0.8f, 1.2f);
        transform.localScale *= randomSize;
    }

    private void Update()
    {
        if (isCollected) return;

        // Magnet power-up ke liye
        if (magnetTarget != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                magnetTarget.position,
                magnetSpeed * Time.deltaTime
            );
            return;
        }

        // Smooth bob animation
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotation animation
        transform.Rotate(0, 0, 60f * Time.deltaTime);
    }

    /// <summary>
    /// Food collect karo — value return karo
    /// </summary>
    public int Collect()
    {
        if (isCollected) return 0;

        isCollected = true;

        // Collect animation
        LeanTween.scale(gameObject, Vector3.zero, 0.15f)
            .setEaseInBack()
            .setOnComplete(() => Destroy(gameObject));

        return value;
    }

    /// <summary>
    /// Magnet power-up ke liye target set karo
    /// </summary>
    public void SetMagnetTarget(Transform target)
    {
        magnetTarget = target;
    }
}
