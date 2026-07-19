using UnityEngine;

/// <summary>
/// SpeedBoost Power-Up — Worm ki speed temporarily badh jaati hai
/// </summary>
public class SpeedBoost : PowerUpBase
{
    [Header("Speed Boost Settings")]
    public float speedAmount = 4f;         // Kitni speed increase ho

    protected override void Start()
    {
        powerUpName = "Speed Boost";
        powerUpColor = new Color(1f, 0.7f, 0f);  // Orange
        base.Start();
    }

    public override void Apply(GameObject worm)
    {
        WormMovement movement = worm.GetComponentInParent<WormMovement>();

        if (movement != null)
        {
            movement.ApplySpeedBoost(speedAmount, duration);

            // Notification
            GameHUD.Instance?.ShowNotification("⚡ SPEED BOOST!", powerUpColor);

            Debug.Log($"Speed boost applied for {duration} seconds!");
        }

        base.Apply(worm);
    }
}
