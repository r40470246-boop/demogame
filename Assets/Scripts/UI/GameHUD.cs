using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GameHUD — In-game UI: score, leaderboard, timer, notifications
/// GameScene mein Canvas pe attach karo
/// </summary>
public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance;

    [Header("Score & Timer")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI sizeText;

    [Header("Leaderboard")]
    public Transform leaderboardContainer;
    public GameObject leaderboardEntryPrefab;
    private List<GameObject> leaderboardEntries = new List<GameObject>();

    [Header("Notification")]
    public TextMeshProUGUI notificationText;
    private Coroutine notificationCoroutine;

    [Header("Boost Button")]
    public Button boostButton;
    public Image boostButtonFill;    // Cooldown fill

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Boost button listener
        if (boostButton != null)
        {
            boostButton.onClick.AddListener(OnBoostPressed);
        }

        if (notificationText != null)
            notificationText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Score update karo
    /// </summary>
    public void UpdateScore(int score, int segments)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        if (sizeText != null)
            sizeText.text = $"Size: {segments}";
    }

    /// <summary>
    /// Timer update karo
    /// </summary>
    public void UpdateTimer(float timeRemaining)
    {
        if (timerText == null) return;

        int minutes = (int)(timeRemaining / 60);
        int seconds = (int)(timeRemaining % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";

        // Warning color jab time kam ho
        if (timeRemaining < 30)
            timerText.color = Color.red;
        else if (timeRemaining < 60)
            timerText.color = Color.yellow;
        else
            timerText.color = Color.white;
    }

    /// <summary>
    /// Leaderboard update karo
    /// </summary>
    public void UpdateLeaderboard(List<LeaderboardManager.PlayerScore> scores)
    {
        // Purani entries clear karo
        foreach (var entry in leaderboardEntries)
            Destroy(entry);
        leaderboardEntries.Clear();

        if (leaderboardContainer == null) return;

        // Top 5 show karo
        int maxEntries = Mathf.Min(5, scores.Count);

        for (int i = 0; i < maxEntries; i++)
        {
            var score = scores[i];
            GameObject entry;

            if (leaderboardEntryPrefab != null)
            {
                entry = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            }
            else
            {
                // Runtime mein entry create karo
                entry = CreateDefaultEntry(score, i + 1);
                entry.transform.SetParent(leaderboardContainer, false);
            }

            leaderboardEntries.Add(entry);

            // Text set karo
            TextMeshProUGUI entryText = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (entryText != null)
            {
                string medal = i == 0 ? "🥇" : i == 1 ? "🥈" : i == 2 ? "🥉" : $"{i + 1}.";
                entryText.text = $"{medal} {score.playerName} - {score.score}";

                // Local player ko highlight karo
                bool isLocalPlayer = !Photon.Pun.PhotonNetwork.IsConnected
                    ? i == 0
                    : score.photonActorNumber == Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;

                entryText.color = isLocalPlayer ? Color.yellow : Color.white;
            }
        }

        // Mera rank update karo
        int myRank = LeaderboardManager.Instance?.GetMyRank() ?? 1;
        if (rankText != null)
            rankText.text = $"Rank #{myRank}";
    }

    private GameObject CreateDefaultEntry(LeaderboardManager.PlayerScore score, int rank)
    {
        GameObject entry = new GameObject($"Entry_{rank}");
        RectTransform rt = entry.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 30);

        TextMeshProUGUI text = entry.AddComponent<TextMeshProUGUI>();
        text.fontSize = 14;
        text.color = Color.white;
        text.text = $"{rank}. {score.playerName} - {score.score}";

        return entry;
    }

    /// <summary>
    /// Power-up notification dikhao
    /// </summary>
    public void ShowNotification(string message, Color color)
    {
        if (notificationText == null) return;

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message, color));
    }

    private System.Collections.IEnumerator ShowNotificationCoroutine(string message, Color color)
    {
        notificationText.gameObject.SetActive(true);
        notificationText.text = message;
        notificationText.color = color;

        // Fade in
        float t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            notificationText.alpha = Mathf.Lerp(0, 1, t / 0.3f);
            yield return null;
        }

        // Wait
        yield return new WaitForSeconds(2f);

        // Fade out
        t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            notificationText.alpha = Mathf.Lerp(1, 0, t / 0.5f);
            yield return null;
        }

        notificationText.gameObject.SetActive(false);
    }

    private void OnBoostPressed()
    {
        WormMovement myWorm = FindObjectOfType<WormMovement>();

        if (myWorm != null && (myWorm.GetComponent<Photon.Pun.PhotonView>()?.IsMine ?? true))
        {
            myWorm.StartBoost();
        }
    }
}
