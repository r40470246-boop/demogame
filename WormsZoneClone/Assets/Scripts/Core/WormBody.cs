using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// WormBody — Worm ke body segments manage karta hai
/// Head ke peeche smooth snake movement implement karta hai
/// </summary>
public class WormBody : MonoBehaviourPun
{
    [Header("Body Settings")]
    public GameObject bodySegmentPrefab;   // Body segment prefab
    public int initialSegments = 5;        // Shuruat mein kitne segments
    public float segmentSpacing = 0.3f;    // Segments ke beech distance
    public float bodyFollowSpeed = 15f;    // Body kitni tez follow kare

    [Header("Visual")]
    public Color bodyColor = Color.green;  // Default color
    public float segmentSize = 0.4f;       // Segment ka size

    // Body segments ki list
    private List<Transform> segments = new List<Transform>();
    private List<Vector3> positionHistory = new List<Vector3>();  // Position history for smooth follow

    // Score tracking
    private int score = 0;
    public int Score => score;
    public int SegmentCount => segments.Count;

    // Camera reference
    private CameraFollow cameraFollow;

    // Food spawner reference (death pe food spawn ke liye)
    private FoodSpawner foodSpawner;

    private void Start()
    {
        cameraFollow = FindObjectOfType<CameraFollow>();
        foodSpawner = FindObjectOfType<FoodSpawner>();

        // Sirf apne worm ke segments banao
        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            InitializeBody();
        }
    }

    private void InitializeBody()
    {
        // Position history initialize karo
        for (int i = 0; i < 300; i++)
            positionHistory.Add(transform.position);

        // Initial segments banao
        for (int i = 0; i < initialSegments; i++)
        {
            AddSegment();
        }
    }

    private void Update()
    {
        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            UpdatePositionHistory();
            MoveSegments();
        }
    }

    private void UpdatePositionHistory()
    {
        // Har frame head ki position history mein add karo
        positionHistory.Insert(0, transform.position);

        // History ka size limit rakho
        int maxHistory = segments.Count * Mathf.RoundToInt(1f / segmentSpacing) + 50;
        if (positionHistory.Count > maxHistory)
            positionHistory.RemoveAt(positionHistory.Count - 1);
    }

    private void MoveSegments()
    {
        // Har segment ko uske position history ke point pe move karo
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == null) continue;

            // History index calculate karo
            int historyIndex = Mathf.RoundToInt(i * segmentSpacing / Time.deltaTime * 0.016f);
            historyIndex = Mathf.Clamp(historyIndex, 0, positionHistory.Count - 1);

            // Smooth move
            segments[i].position = Vector3.Lerp(
                segments[i].position,
                positionHistory[Mathf.Min(historyIndex + (i * 3), positionHistory.Count - 1)],
                bodyFollowSpeed * Time.deltaTime
            );

            // Rotation bhi smooth karo
            if (i > 0 && segments[i - 1] != null)
            {
                Vector3 dir = segments[i - 1].position - segments[i].position;
                if (dir != Vector3.zero)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    segments[i].rotation = Quaternion.Lerp(
                        segments[i].rotation,
                        Quaternion.Euler(0, 0, angle - 90f),
                        bodyFollowSpeed * Time.deltaTime
                    );
                }
            }
        }
    }

    /// <summary>
    /// Naya body segment add karo (food khane ke baad)
    /// </summary>
    public void AddSegment()
    {
        Vector3 spawnPos = segments.Count > 0
            ? segments[segments.Count - 1].position
            : transform.position - transform.up * segmentSpacing;

        GameObject newSegment;

        if (bodySegmentPrefab != null)
        {
            newSegment = Instantiate(bodySegmentPrefab, spawnPos, transform.rotation);
        }
        else
        {
            // Default segment banao agar prefab nahi hai
            newSegment = new GameObject("Segment_" + segments.Count);
            newSegment.transform.position = spawnPos;

            SpriteRenderer sr = newSegment.AddComponent<SpriteRenderer>();
            sr.color = bodyColor;
            sr.sortingOrder = -segments.Count; // Head ke neeche render ho

            // Circle shape
            sr.sprite = CreateCircleSprite();
            newSegment.transform.localScale = Vector3.one * segmentSize;
        }

        // Head ke child banao (cleanup ke liye)
        newSegment.transform.SetParent(null);
        segments.Add(newSegment.transform);
    }

    /// <summary>
    /// Food khaya — score badao aur body badao
    /// </summary>
    public void OnFoodEaten(int foodValue)
    {
        score += foodValue;

        // Har 5 score pe ek segment add karo
        int segmentsToAdd = foodValue / 5;
        for (int i = 0; i < Mathf.Max(1, segmentsToAdd); i++)
        {
            AddSegment();
        }

        // Camera zoom adjust karo
        if (cameraFollow != null)
            cameraFollow.SetZoomForSize(segments.Count);

        // Leaderboard update karo
        LeaderboardManager.Instance?.UpdateScore(score);
    }

    /// <summary>
    /// Boost se body thodi shrink hogi
    /// </summary>
    public void ShrinkBody(float amount)
    {
        // Shrink timer - accumulated amount se remove karo
        if (segments.Count <= 5) return; // Minimum size

        // Baad mein accumulated amount se segment remove karo
        // Isse smooth shrinking hogi
    }

    /// <summary>
    /// Death pe saare segments food mein convert karo
    /// </summary>
    public void ConvertToFood()
    {
        if (foodSpawner == null) return;

        foreach (Transform seg in segments)
        {
            if (seg != null)
            {
                // Har segment ki jagah food spawn karo
                foodSpawner.SpawnFoodAt(seg.position, 5);
                Destroy(seg.gameObject);
            }
        }

        segments.Clear();
    }

    /// <summary>
    /// Body ka color set karo (skin ke liye)
    /// </summary>
    public void SetColor(Color color)
    {
        bodyColor = color;

        // Head ka color bhi set karo
        SpriteRenderer headSR = GetComponent<SpriteRenderer>();
        if (headSR != null)
            headSR.color = color;

        // Saare segments ka color set karo
        foreach (Transform seg in segments)
        {
            if (seg == null) continue;
            SpriteRenderer sr = seg.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = color;
        }
    }

    private void OnDestroy()
    {
        // Cleanup - saare segments destroy karo
        foreach (Transform seg in segments)
        {
            if (seg != null)
                Destroy(seg.gameObject);
        }
    }

    // Simple circle sprite banana (runtime mein)
    private Sprite CreateCircleSprite()
    {
        Texture2D tex = new Texture2D(64, 64);
        Vector2 center = new Vector2(32, 32);

        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                tex.SetPixel(x, y, dist < 30 ? Color.white : Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }
}
