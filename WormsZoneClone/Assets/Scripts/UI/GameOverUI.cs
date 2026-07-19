using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GameOverUI — Game over screen: score, rank, retry, menu buttons
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI messageText;

    [Header("Buttons")]
    public Button playAgainButton;
    public Button mainMenuButton;

    [Header("Animation")]
    public Animator panelAnimator;

    private void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        playAgainButton?.onClick.AddListener(OnPlayAgain);
        mainMenuButton?.onClick.AddListener(OnMainMenu);
    }

    /// <summary>
    /// Game over screen show karo
    /// </summary>
    public void Show(int finalScore, int rank)
    {
        if (panel != null)
            panel.SetActive(true);

        // Score aur rank set karo
        if (finalScoreText != null)
            finalScoreText.text = $"Score: {finalScore}";

        if (rankText != null)
            rankText.text = $"Rank #{rank}";

        // Performance based message
        string message = "";
        if (rank == 1)
        {
            titleText.text = "🏆 CHAMPION!";
            message = "Waah bhai, tu number 1 hai!";
        }
        else if (rank <= 3)
        {
            titleText.text = "🎉 GREAT GAME!";
            message = "Top 3 mein aaya! Ekdum solid!";
        }
        else if (rank <= 5)
        {
            titleText.text = "💪 WELL PLAYED!";
            message = "Top 5! Aur thodi practice kar!";
        }
        else
        {
            titleText.text = "☠️ GAME OVER";
            message = "Koi nahi bhai, agli baar pakka!";
        }

        if (messageText != null)
            messageText.text = message;

        // Animation play karo
        if (panelAnimator != null)
            panelAnimator.SetTrigger("Show");
    }

    private void OnPlayAgain()
    {
        // Same scene reload
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    private void OnMainMenu()
    {
        GameManager.Instance?.ReturnToMenu();
    }
}
