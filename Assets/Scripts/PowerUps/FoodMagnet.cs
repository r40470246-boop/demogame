using UnityEngine;

/// <summary>
/// FoodMagnet Power-Up — Aas paas ka khaana automatic attract hota hai
/// </summary>
public class FoodMagnet : PowerUpBase
{
    [Header("Magnet Settings")]
    public float magnetRadius = 8f;     // Kitni dur se food attract hoga
    public float magnetForce = 10f;    // Kitni tez attract hoga

    protected override void Start()
    {
        powerUpName = "Food Magnet";
        powerUpColor = new Color(0f, 0.8f, 1f);  // Cyan
        base.Start();
    }

    public override void Apply(GameObject worm)
    {
        // Magnet component add karo worm pe
        MagnetEffect magnet = worm.AddComponent<MagnetEffect>();
        magnet.magnetRadius = magnetRadius;
        magnet.magnetForce = magnetForce;
        magnet.duration = duration;

        GameHUD.Instance?.ShowNotification("🧲 FOOD MAGNET!", powerUpColor);

        base.Apply(worm);
    }
}

/// <summary>
/// Magnet effect component — worm pe temporarily attach hota hai
/// </summary>
public class MagnetEffect : MonoBehaviour
{
    public float magnetRadius = 8f;
    public float magnetForce = 10f;
    public float duration = 5f;

    private float timer;

    private void Start()
    {
        timer = duration;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            Destroy(this);
            return;
        }

        // Aas paas ke saare food attract karo
        Collider2D[] nearbyFood = Physics2D.OverlapCircleAll(transform.position, magnetRadius);

        foreach (Collider2D col in nearbyFood)
        {
            if (col.CompareTag("Food"))
            {
                FoodItem food = col.GetComponent<FoodItem>();
                food?.SetMagnetTarget(transform);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }
}
