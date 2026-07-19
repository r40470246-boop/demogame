using UnityEngine;

/// <summary>
/// Shield Power-Up — Ek collision se protect karta hai
/// </summary>
public class Shield : PowerUpBase
{
    protected override void Start()
    {
        powerUpName = "Shield";
        powerUpColor = new Color(0.3f, 1f, 0.3f);  // Green
        base.Start();
    }

    public override void Apply(GameObject worm)
    {
        WormHead head = worm.GetComponentInParent<WormHead>();

        if (head != null)
        {
            head.ActivateShield(duration);
            GameHUD.Instance?.ShowNotification("🛡️ SHIELD ACTIVE!", powerUpColor);
        }

        base.Apply(worm);
    }
}
