using UnityEngine;
using Photon.Pun;

/// <summary>
/// WormHead — Worm ke head pe collision detection
/// Food eat karna aur dusre worms se collision handle karna
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class WormHead : MonoBehaviourPun
{
    [Header("References")]
    public WormBody wormBody;
    public WormMovement wormMovement;
    public ParticleSystem eatParticles;     // Food eat karne ka effect
    public ParticleSystem deathParticles;  // Death explosion effect

    [Header("Shield")]
    public bool hasShield = false;         // Shield power-up active?
    public GameObject shieldVisual;        // Shield glow object

    private void Awake()
    {
        // Sirf apne worm pe collision check karo
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.25f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Sirf apne worm ka collision process karo
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

        // Food khaaya
        if (other.CompareTag("Food"))
        {
            FoodItem food = other.GetComponent<FoodItem>();
            if (food != null)
            {
                int value = food.Collect();
                wormBody?.OnFoodEaten(value);

                // Eat effect play karo
                if (eatParticles != null)
                    eatParticles.Play();
            }
        }

        // Power-Up mila
        else if (other.CompareTag("PowerUp"))
        {
            PowerUpBase powerUp = other.GetComponent<PowerUpBase>();
            powerUp?.Apply(gameObject);
        }

        // Doosre worm ki BODY se takraya — apni death
        else if (other.CompareTag("WormBody"))
        {
            // Check karo ye apni body toh nahi hai
            WormBody otherBody = other.GetComponentInParent<WormBody>();
            if (otherBody != null && otherBody != wormBody)
            {
                // Shield hai toh survive karo
                if (hasShield)
                {
                    RemoveShield();
                    return;
                }

                Die();
            }
        }

        // Map boundary se bahar gaya
        else if (other.CompareTag("Boundary"))
        {
            Die();
        }
    }

    /// <summary>
    /// Worm mar gaya — effects play karo
    /// </summary>
    private void Die()
    {
        if (wormMovement == null || !wormMovement.IsAlive) return;

        // Death particles
        if (deathParticles != null)
        {
            deathParticles.transform.SetParent(null);
            deathParticles.Play();
            Destroy(deathParticles.gameObject, 2f);
        }

        wormMovement.Die();
    }

    /// <summary>
    /// Shield activate karo
    /// </summary>
    public void ActivateShield(float duration)
    {
        hasShield = true;
        if (shieldVisual != null)
            shieldVisual.SetActive(true);

        Invoke(nameof(RemoveShield), duration);
    }

    private void RemoveShield()
    {
        hasShield = false;
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }
}
