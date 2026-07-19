using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// NetworkManager — Photon PUN 2 connection aur room management
/// Main Menu scene mein ek GameObject pe attach karo
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    [Header("Photon Settings")]
    public string gameVersion = "1.0";
    public byte maxPlayersPerRoom = 15;

    [Header("UI")]
    public LobbyUI lobbyUI;
    public MainMenuUI mainMenuUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Photon se connect karo
        ConnectToPhoton();
    }

    /// <summary>
    /// Photon server se connect karo
    /// </summary>
    public void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Photon se connect ho raha hai...");
        }
    }

    /// <summary>
    /// Random room mein join karo (Quick Play)
    /// </summary>
    public void QuickPlay(string playerName)
    {
        PhotonNetwork.NickName = playerName;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            ConnectToPhoton();
        }
    }

    /// <summary>
    /// Custom room create karo
    /// </summary>
    public void CreateRoom(string roomName, string playerName)
    {
        PhotonNetwork.NickName = playerName;

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = maxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(roomName, options);
    }

    /// <summary>
    /// Room naam se join karo
    /// </summary>
    public void JoinRoom(string roomName, string playerName)
    {
        PhotonNetwork.NickName = playerName;
        PhotonNetwork.JoinRoom(roomName);
    }

    /// <summary>
    /// Offline (single player) mode mein khelo
    /// </summary>
    public void PlayOffline(string playerName)
    {
        PhotonNetwork.NickName = playerName;
        PhotonNetwork.OfflineMode = true;
        SceneManager.LoadScene("GameScene");
    }

    // ======= Photon Callbacks =======

    public override void OnConnectedToMaster()
    {
        Debug.Log("✅ Photon Master Server se connect ho gaya!");
        PhotonNetwork.JoinLobby();

        if (mainMenuUI != null)
            mainMenuUI.OnConnected();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("✅ Lobby mein join ho gaya!");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"❌ Disconnect ho gaya: {cause}");

        if (mainMenuUI != null)
            mainMenuUI.OnDisconnected();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"✅ Room mein join ho gaya: {PhotonNetwork.CurrentRoom.Name}");

        // Game scene load karo
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Random room nahi mila, naya create kar raha hoon...");

        // Naya room create karo
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = maxPlayersPerRoom
        };
        PhotonNetwork.CreateRoom(null, options); // null = random name
    }

    public override void OnCreatedRoom()
    {
        Debug.Log($"✅ Room create ho gaya: {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"❌ Room create nahi hua: {message}");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"🎮 {newPlayer.NickName} room mein aaya!");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"👋 {otherPlayer.NickName} room se gaya!");
    }

    public override void OnRoomListUpdate(System.Collections.Generic.List<RoomInfo> roomList)
    {
        if (lobbyUI != null)
            lobbyUI.UpdateRoomList(roomList);
    }
}
