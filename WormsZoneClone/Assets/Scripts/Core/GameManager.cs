using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// GameManager — Game ka main controller
/// Singleton pattern use karta hai
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public float gameTime = 300f;       // Game ka total time (5 min)
    public int maxPlayers = 15;

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;     // Worm spawn points
    public GameObject wormPrefab;       // Worm prefab (Photon Resources folder mein hona chahiye)

    [Header("UI References")]
    public GameHUD gameHUD;
    public GameOverUI gameOverUI;

    // Game state
    private bool gameStarted = false;
    private bool gameOver = false;
    private float timeRemaining;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        timeRemaining = gameTime;
        SpawnLocalPlayer();
        gameStarted = true;
    }

    private void Update()
    {
        if (!gameStarted || gameOver) return;

        // Timer countdown
        timeRemaining -= Time.deltaTime;

        if (gameHUD != null)
            gameHUD.UpdateTimer(timeRemaining);

        if (timeRemaining <= 0)
            EndGame();
    }

    /// <summary>
    /// Player spawn karo
    /// </summary>
    private void SpawnLocalPlayer()
    {
        // Random spawn point
        Vector3 spawnPos = GetRandomSpawnPoint();

        GameObject worm;
        if (PhotonNetwork.IsConnected)
        {
            // Photon se spawn karo
            worm = PhotonNetwork.Instantiate("WormPrefab", spawnPos, Quaternion.identity);
        }
        else
        {
            // Offline mode
            worm = Instantiate(wormPrefab, spawnPos, Quaternion.identity);
        }

        if (worm == null) return;

        // Camera ko worm follow karwao
        CameraFollow cam = FindObjectOfType<CameraFollow>();
        if (cam != null)
            cam.SetTarget(worm.transform);

        // Skin apply karo
        WormBody body = worm.GetComponent<WormBody>();
        WormSkinManager.Instance?.ApplySkin(body);

        // Player name set karo
        if (PhotonNetwork.IsConnected)
        {
            PhotonView pv = worm.GetComponent<PhotonView>();
            pv?.RPC("SetPlayerName", RpcTarget.AllBuffered, PhotonNetwork.NickName);
        }
    }

    private Vector3 GetRandomSpawnPoint()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        }

        // Default random spawn
        return new Vector3(
            Random.Range(-30f, 30f),
            Random.Range(-30f, 30f),
            0
        );
    }

    /// <summary>
    /// Player mar gaya — Game Over show karo
    /// </summary>
    public void OnPlayerDied()
    {
        if (gameOver) return;
        gameOver = true;

        int finalScore = LeaderboardManager.Instance?.GetMyScore() ?? 0;
        int rank = LeaderboardManager.Instance?.GetMyRank() ?? 0;

        if (gameOverUI != null)
            gameOverUI.Show(finalScore, rank);
    }

    /// <summary>
    /// Game time khatam — end karo
    /// </summary>
    private void EndGame()
    {
        gameOver = true;

        int finalScore = LeaderboardManager.Instance?.GetMyScore() ?? 0;
        int rank = LeaderboardManager.Instance?.GetMyRank() ?? 0;

        if (gameOverUI != null)
            gameOverUI.Show(finalScore, rank);
    }

    /// <summary>
    /// Main Menu pe wapas jao
    /// </summary>
    public void ReturnToMenu()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.LeaveRoom();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // Photon Callbacks
    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Agar koi player chala gaya
        Debug.Log($"{otherPlayer.NickName} left the game");
    }
}
