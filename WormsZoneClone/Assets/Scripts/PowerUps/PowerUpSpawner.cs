using UnityEngine;

/// <summary>
/// PowerUpSpawner — Map pe random power-ups spawn karta hai
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float spawnInterval = 20f;      // Har 20 second mein spawn
    public int maxPowerUpsOnMap = 5;       // Max power-ups ek saath
    public float mapSize = 40f;            // Map boundaries

    [Header("Power-Up Prefabs")]
    public GameObject speedBoostPrefab;
    public GameObject foodMagnetPrefab;
    public GameObject shieldPrefab;

    private int currentPowerUps = 0;
    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval && currentPowerUps < maxPowerUpsOnMap)
        {
            timer = 0f;
            SpawnRandomPowerUp();
        }
    }

    private void SpawnRandomPowerUp()
    {
        GameObject[] prefabs = { speedBoostPrefab, foodMagnetPrefab, shieldPrefab };

        // Null prefabs filter karo
        System.Collections.Generic.List<GameObject> validPrefabs = new System.Collections.Generic.List<GameObject>();
        foreach (var p in prefabs)
        {
            if (p != null) validPrefabs.Add(p);
        }

        if (validPrefabs.Count == 0) return;

        // Random prefab aur position
        GameObject selected = validPrefabs[Random.Range(0, validPrefabs.Count)];
        Vector3 pos = new Vector3(
            Random.Range(-mapSize, mapSize),
            Random.Range(-mapSize, mapSize),
            0
        );

        GameObject powerUp = Instantiate(selected, pos, Quaternion.identity);
        currentPowerUps++;

        // Destroy hone pe count kam karo
        PowerUpBase pBase = powerUp.GetComponent<PowerUpBase>();
        if (pBase != null)
            StartCoroutine(TrackPowerUp(powerUp, pBase.spawnLifetime));
    }

    private System.Collections.IEnumerator TrackPowerUp(GameObject powerUp, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);

        if (powerUp != null) // Pehle se collect nahi hua
        {
            Destroy(powerUp);
            currentPowerUps = Mathf.Max(0, currentPowerUps - 1);
        }
        else
        {
            currentPowerUps = Mathf.Max(0, currentPowerUps - 1);
        }
    }
}
