using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

/// <summary>
/// LobbyUI — Room list, create room, join room
/// </summary>
public class LobbyUI : MonoBehaviour
{
    [Header("Create Room")]
    public TMP_InputField roomNameInput;
    public Button createRoomButton;

    [Header("Room List")]
    public Transform roomListContainer;
    public GameObject roomEntryPrefab;
    public Button refreshButton;

    [Header("Status")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI playerCountText;

    private List<GameObject> roomEntries = new List<GameObject>();

    private void Start()
    {
        createRoomButton?.onClick.AddListener(OnCreateRoom);
        refreshButton?.onClick.AddListener(OnRefresh);
    }

    private void OnCreateRoom()
    {
        string roomName = roomNameInput.text.Trim();
        if (string.IsNullOrEmpty(roomName))
            roomName = "Room_" + Random.Range(1000, 9999);

        string playerName = PlayerPrefs.GetString("PlayerName", "Player");
        NetworkManager.Instance?.CreateRoom(roomName, playerName);
        statusText.text = $"Creating room '{roomName}'...";
    }

    private void OnRefresh()
    {
        statusText.text = "Refreshing...";
        // Photon automatically room list update karta hai
    }

    /// <summary>
    /// Room list update karo (NetworkManager se call hoga)
    /// </summary>
    public void UpdateRoomList(List<RoomInfo> rooms)
    {
        // Purani entries clear
        foreach (var entry in roomEntries)
            Destroy(entry);
        roomEntries.Clear();

        foreach (var room in rooms)
        {
            if (room.RemovedFromList) continue;

            GameObject entry;

            if (roomEntryPrefab != null)
            {
                entry = Instantiate(roomEntryPrefab, roomListContainer);
            }
            else
            {
                entry = CreateDefaultRoomEntry(room);
                entry.transform.SetParent(roomListContainer, false);
            }

            roomEntries.Add(entry);
        }

        if (playerCountText != null)
            playerCountText.text = $"Rooms: {roomEntries.Count}";

        statusText.text = roomEntries.Count == 0 ? "No rooms found. Create one!" : "Select a room to join";
    }

    private GameObject CreateDefaultRoomEntry(RoomInfo room)
    {
        GameObject entry = new GameObject($"RoomEntry_{room.Name}");

        RectTransform rt = entry.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 50);

        Button btn = entry.AddComponent<Button>();
        Image img = entry.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

        TextMeshProUGUI text = new GameObject("Text").AddComponent<TextMeshProUGUI>();
        text.transform.SetParent(entry.transform, false);
        text.text = $"{room.Name}  ({room.PlayerCount}/{room.MaxPlayers})";
        text.fontSize = 16;
        text.color = Color.white;

        string capturedName = room.Name;
        btn.onClick.AddListener(() => JoinRoom(capturedName));

        return entry;
    }

    private void JoinRoom(string roomName)
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Player");
        NetworkManager.Instance?.JoinRoom(roomName, playerName);
        statusText.text = $"Joining '{roomName}'...";
    }
}
