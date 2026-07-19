using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// MainMenuUI — Main Menu screen
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject connectingPanel;

    [Header("Input Fields")]
    public TMP_InputField playerNameInput;

    [Header("Buttons")]
    public Button quickPlayButton;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button offlineButton;
    public Button skinsButton;

    [Header("Status Text")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI versionText;

    [Header("Skin Preview")]
    public Image skinPreviewImage;

    private void Start()
    {
        // Buttons setup
        quickPlayButton?.onClick.AddListener(OnQuickPlay);
        createRoomButton?.onClick.AddListener(OnShowLobby);
        offlineButton?.onClick.AddListener(OnOfflinePlay);
        skinsButton?.onClick.AddListener(OnOpenSkins);

        // Saved name load karo
        playerNameInput.text = PlayerPrefs.GetString("PlayerName", "Player" + Random.Range(100, 999));

        // Version text
        if (versionText != null)
            versionText.text = "v1.0.0";

        // Connecting panel show karo
        ShowConnecting(true);
        statusText.text = "Connecting to server...";

        // Skin preview
        UpdateSkinPreview();
    }

    public void OnConnected()
    {
        ShowConnecting(false);
        statusText.text = "Connected! ✅";
    }

    public void OnDisconnected()
    {
        ShowConnecting(false);
        statusText.text = "❌ Offline Mode";
    }

    private void OnQuickPlay()
    {
        SavePlayerName();
        NetworkManager.Instance?.QuickPlay(playerNameInput.text);
        statusText.text = "Finding game...";
    }

    private void OnShowLobby()
    {
        SavePlayerName();
        SceneManager.LoadScene("Lobby");
    }

    private void OnOfflinePlay()
    {
        SavePlayerName();
        NetworkManager.Instance?.PlayOffline(playerNameInput.text);
    }

    private void OnOpenSkins()
    {
        // Skin selection panel toggle
        // Isko extend karo apni need ke according
        Debug.Log("Skins panel open karo!");
    }

    private void SavePlayerName()
    {
        string name = playerNameInput.text.Trim();
        if (string.IsNullOrEmpty(name))
            name = "Player" + Random.Range(100, 999);

        playerNameInput.text = name;
        PlayerPrefs.SetString("PlayerName", name);
        PlayerPrefs.Save();
    }

    private void ShowConnecting(bool show)
    {
        if (connectingPanel != null) connectingPanel.SetActive(show);
        if (mainPanel != null) mainPanel.SetActive(!show);
    }

    private void UpdateSkinPreview()
    {
        if (skinPreviewImage == null || WormSkinManager.Instance == null) return;

        WormSkinManager.WormSkin skin = WormSkinManager.Instance.GetSelectedSkin();
        skinPreviewImage.color = skin.headColor;
    }
}
