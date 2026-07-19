using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// FoodSpawner — Map pe food spawn karta hai
/// Photon pe sirf Master Client food spawn karta hai (sync ke liye)
/// </summary>
public class FoodSpawner : MonoBehaviourPun
{
    public static FoodSpawner Instance;

    [Header("Food Settings")]
    public GameObject foodPrefab;           // Food prefab
    public int maxFoodOnMap = 200;          // Map pe max food
    public float spawnInterval = 0.5f;     // Kitni baar spawn karo
    public float mapSize = 45f;            // Map boundaries

    [Header("Food Types")]
    public FoodType[] foodTypes;           // Different types of food

    private List<GameObject> activeFoods = new List<GameObject>();
    private float spawnTimer = 0f;

    [System.Serializable]
    public class FoodType
    {
        public string name;
        public int value = 10;
        public Color color = Color.yellow;
        public float spawnWeight = 1f;    // Kitni baar spawn ho
        public float size = 0.3f;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Default food types setup (agar inspector mein nahi set kiya)
        if (foodTypes == null || foodTypes.Length == 0)
        {
            foodTypes = new FoodType[]
            {
                new FoodType { name = "Small", value = 5, color = new Color(1f, 0.8f, 0f), spawnWeight = 5f, size = 0.25f },
                new FoodType { name = "Medium", value = 15, color = new Color(0f, 1f, 0.5f), spawnWeight = 3f, size = 0.35f },
                new FoodType { name = "Large", value = 30, color = new Color(1f, 0.3f, 0.8f), spawnWeight = 1f, size = 0.5f },
                new FoodType { name = "Rainbow", value = 50, color = new Color(0.5f, 0f, 1f), spawnWeight = 0.3f, size = 0.6f }
            };
        }

        // Initial food spawn (sirf Master Client kare)
        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected)
        {
            for (int i = 0; i < maxFoodOnMap / 2; i++)
            {
                SpawnRandomFood();
            }
        }
    }

    private void Update()
    {
        // Sirf Master Client food spawn kare (baaki sab receive karenge)
        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected) return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval && activeFoods.Count < maxFoodOnMap)
        {
            spawnTimer = 0f;
            SpawnRandomFood();
        }

        // Null references clean karo
        activeFoods.RemoveAll(f => f == null);
    }

    private void SpawnRandomFood()
    {
        // Map pe random position
        Vector2 randomPos = new Vector2(
            Random.Range(-mapSize, mapSize),
            Random.Range(-mapSize, mapSize)
        );

        SpawnFoodAt(randomPos, GetRandomFoodValue());
    }

    /// <summary>
    /// Specific position pe food spawn karo (death ke baad)
    /// </summary>
    public void SpawnFoodAt(Vector3 position, int value)
    {
        if (foodPrefab == null)
        {
            // Runtime mein food create karo
            CreateFoodAtPosition(position, value);
            return;
        }

        GameObject food;
        if (PhotonNetwork.IsConnected)
        {
            food = PhotonNetwork.Instantiate(foodPrefab.name, position, Quaternion.identity);
        }
        else
        {
            food = Instantiate(foodPrefab, position, Quaternion.identity);
        }

        FoodItem foodItem = food.GetComponent<FoodItem>();
        if (foodItem != null)
            foodItem.value = value;

        activeFoods.Add(food);
    }

    private void CreateFoodAtPosition(Vector3 pos, int value)
    {
        // Agar prefab nahi hai toh runtime mein banao
        GameObject food = new GameObject("Food");
        food.transform.position = pos;

        SpriteRenderer sr = food.AddComponent<SpriteRenderer>();
        sr.color = GetColorForValue(value);
        sr.sortingOrder = 1;

        // Circle shape
        Texture2D tex = new Texture2D(32, 32);
        Vector2 center = new Vector2(16, 16);
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                tex.SetPixel(x, y, dist < 14 ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));

        float size = Mathf.Lerp(0.2f, 0.6f, value / 50f);
        food.transform.localScale = Vector3.one * size;

        CircleCollider2D col = food.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        food.tag = "Food";

        FoodItem foodItem = food.AddComponent<FoodItem>();
        foodItem.value = value;

        activeFoods.Add(food);
    }

    private Color GetColorForValue(int value)
    {
        if (value <= 10) return new Color(1f, 0.9f, 0f);      // Yellow - small
        if (value <= 25) return new Color(0f, 1f, 0.5f);      // Green - medium
        if (value <= 40) return new Color(1f, 0.3f, 0.8f);    // Pink - large
        return new Color(0.5f, 0f, 1f);                        // Purple - rainbow
    }

    private int GetRandomFoodValue()
    {
        // Weighted random food type select karo
        float totalWeight = 0;
        foreach (var type in foodTypes)
            totalWeight += type.spawnWeight;

        float random = Random.Range(0, totalWeight);
        float cumulative = 0;

        foreach (var type in foodTypes)
        {
            cumulative += type.spawnWeight;
            if (random <= cumulative)
                return type.value;
        }

        return foodTypes[0].value;
    }
}
