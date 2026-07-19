using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// LeaderboardManager — Real-time leaderboard manage karta hai
/// Saare players ke scores track karta hai
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    [Header("UI Reference")]
    public GameHUD gameHUD;

    // Players aur unke scores
    private Dictionary<int, PlayerScore> playerScores = new Dictionary<int, PlayerScore>();

    [System.Serializable]
    public class PlayerScore
    {
        public string playerName;
        public int score;
        public int photonActorNumber;
        public bool isAlive;
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
        // Saare current players register karo
        if (PhotonNetwork.IsConnected)
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                RegisterPlayer(player.ActorNumber, player.NickName);
            }
        }
        else
        {
            // Offline mode mein sirf local player
            RegisterPlayer(0, "You");

            // Fake AI players add karo
            string[] aiNames = { "SnakeKing", "WormMaster", "SlitherPro", "CoilBoss", "ZoneDominator" };
            for (int i = 1; i <= 5; i++)
            {
                RegisterPlayer(i, aiNames[i - 1]);
                UpdateScore(Random.Range(50, 500), i);
            }
        }
    }

    /// <summary>
    /// Player ko leaderboard mein register karo
    /// </summary>
    public void RegisterPlayer(int actorNumber, string playerName)
    {
        if (!playerScores.ContainsKey(actorNumber))
        {
            playerScores[actorNumber] = new PlayerScore
            {
                playerName = playerName,
                score = 0,
                photonActorNumber = actorNumber,
                isAlive = true
            };
        }
    }

    /// <summary>
    /// Local player ka score update karo
    /// </summary>
    public void UpdateScore(int newScore, int actorNumber = -1)
    {
        // Default: local player
        if (actorNumber == -1)
        {
            actorNumber = PhotonNetwork.IsConnected ? PhotonNetwork.LocalPlayer.ActorNumber : 0;
        }

        if (playerScores.ContainsKey(actorNumber))
        {
            playerScores[actorNumber].score = newScore;
        }

        // Network pe broadcast karo
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(
                new ExitGames.Client.Photon.Hashtable { { "score", newScore } }
            );
        }

        // HUD update karo
        RefreshLeaderboard();
    }

    /// <summary>
    /// Leaderboard refresh karo
    /// </summary>
    private void RefreshLeaderboard()
    {
        // Sort by score (descending)
        List<PlayerScore> sorted = new List<PlayerScore>(playerScores.Values);
        sorted.Sort((a, b) => b.score.CompareTo(a.score));

        // Top 5 HUD pe dikhao
        if (gameHUD != null)
        {
            gameHUD.UpdateLeaderboard(sorted);
        }
    }

    /// <summary>
    /// Mere score ka rank kya hai?
    /// </summary>
    public int GetMyRank()
    {
        int myActorNumber = PhotonNetwork.IsConnected ? PhotonNetwork.LocalPlayer.ActorNumber : 0;

        List<PlayerScore> sorted = new List<PlayerScore>(playerScores.Values);
        sorted.Sort((a, b) => b.score.CompareTo(a.score));

        for (int i = 0; i < sorted.Count; i++)
        {
            if (sorted[i].photonActorNumber == myActorNumber)
                return i + 1;
        }

        return sorted.Count;
    }

    /// <summary>
    /// Mera current score
    /// </summary>
    public int GetMyScore()
    {
        int myActorNumber = PhotonNetwork.IsConnected ? PhotonNetwork.LocalPlayer.ActorNumber : 0;

        if (playerScores.ContainsKey(myActorNumber))
            return playerScores[myActorNumber].score;

        return 0;
    }

    /// <summary>
    /// Player mar gaya — leaderboard update karo
    /// </summary>
    public void OnPlayerDied(int actorNumber)
    {
        if (playerScores.ContainsKey(actorNumber))
            playerScores[actorNumber].isAlive = false;

        RefreshLeaderboard();
    }
}
