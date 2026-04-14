using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject wordPanel;
    public GameObject mathPanel;
    public GameObject resultsPanel;

    [Header("Top HUD")]
    public TMP_Text totalScoreText;
    public TMP_Text levelText;

    [Header("İpucu Sayaçları")]
    public TMP_Text wordHintCountText;
    public TMP_Text mathHintCountText;

    [Header("Can Göstergesi")]
    public TMP_Text livesText;

    [Header("Results")]
    public TMP_Text finalScoreText;
    public TMP_Text summaryText;
    public TMP_Text resultTitleText;
    public TMP_Text chapterStarsText;
    public TMP_Text bestWordText;

    [Header("Sonuç Görselleri")]
    public GameObject successImage;
    public GameObject failImage;

    [Header("Sonraki Buton Görselleri")]
    public Image nextLevelButtonImage;
    public Sprite nextLevelSprite;
    public Sprite nextChapterSprite;

    [Header("Buttons")]
    public Button nextLevelButton;
    public Button retryButton;
    public Button levelSelectButton;
    public Button mainMenuButton;

    [Header("Yıldız Gerekli Popup")]
    public GameObject starsNeededPopup;
    public TMP_Text starsNeededText;
    public Button starsNeededLevelSelectButton;

    [Header("Animasyon")]
    public ResultsAnimator resultsAnimator;
    public ChapterUnlockAnimator chapterUnlockAnimator;

    private int pendingNextChapter = 0;

    // ──────────────────────────────────────────
    // Başlangıç
    // ──────────────────────────────────────────

    private void Start()
    {
        if (starsNeededPopup != null)
            starsNeededPopup.SetActive(false);

        if (starsNeededLevelSelectButton != null)
        {
            starsNeededLevelSelectButton.onClick.RemoveAllListeners();
            starsNeededLevelSelectButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                GameManager.Instance.GoToLevelSelect();
            });
        }

        UpdateHintCount();
        UpdateLivesDisplay();
    }

    // ──────────────────────────────────────────
    // Tüm Panelleri Gizle
    // ──────────────────────────────────────────

    public void HideAllPanels()
    {
        if (wordPanel != null) wordPanel.SetActive(false);
        if (mathPanel != null) mathPanel.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);
    }

    // ──────────────────────────────────────────
    // Panel Gösterme
    // ──────────────────────────────────────────

    public void ShowWordPanel()
    {
        if (wordPanel != null) wordPanel.SetActive(true);
        if (mathPanel != null) mathPanel.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);
        UpdateHintCount();
        UpdateLivesDisplay();
    }

    public void ShowMathPanel()
    {
        if (wordPanel != null) wordPanel.SetActive(false);
        if (mathPanel != null) mathPanel.SetActive(true);
        if (resultsPanel != null) resultsPanel.SetActive(false);
        UpdateHintCount();
        UpdateLivesDisplay();
    }

    public void ShowResults(
        int totalScore, int wordScore, int mathScore,
        int target, int value, int diff, int stars, bool passed,
        bool isNewChapter = false, bool newChapterUnlocked = false,
        string bestWord = "-")
    {
        if (wordPanel != null) wordPanel.SetActive(false);
        if (mathPanel != null) mathPanel.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(true);

        if (successImage != null) successImage.SetActive(false);
        if (failImage != null) failImage.SetActive(false);

        if (summaryText != null)
        {
            summaryText.text =
                "Hedef: " + target +
                "\nSenin Sonucun: " + value +
                "\nFark: " + diff +
                "\n\nKelime Turu: +" + wordScore +
                "\nİşlem Turu: +" + mathScore;
        }

        if (bestWordText != null)
        {
            bestWordText.text = bestWord != "-"
                ? $"En uzun kelime: {bestWord.ToUpper()}"
                : "";
        }

        if (chapterStarsText != null)
        {
            int currentChapter = ChapterManager.GetChapter(GameManager.Instance.CurrentLevel);
            int chapterStars = ChapterManager.GetChapterStars(currentChapter);
            int maxStars = ChapterManager.LevelsPerChapter * ChapterManager.MaxStarsPerLevel;
            chapterStarsText.text = $"Bölüm {currentChapter}: {chapterStars} / {maxStars} yıldız";
        }

        if (levelSelectButton != null)
        {
            levelSelectButton.gameObject.SetActive(true);
            levelSelectButton.onClick.RemoveAllListeners();
            levelSelectButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                GameManager.Instance.GoToLevelSelect();
            });
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(true);
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                ChapterManager.LoadScene("MainMenu");
            });
        }

        UpdateLivesDisplay();

        if (!passed)
            SetRetryMode();
        else if (isNewChapter && newChapterUnlocked)
            SetNewChapterUnlockedMode();
        else if (isNewChapter && !newChapterUnlocked)
            SetChapterLockedMode();
        else
            SetNextLevelMode();

        if (resultsAnimator != null)
            resultsAnimator.PlayResultsAnimation(totalScore, stars);
        else if (finalScoreText != null)
            finalScoreText.text = "Toplam Puan: " + totalScore;
    }

    // ──────────────────────────────────────────
    // Buton Modları
    // ──────────────────────────────────────────

    private void SetNextLevelMode()
    {
        if (successImage != null) successImage.SetActive(true);
        if (failImage != null) failImage.SetActive(false);

        if (nextLevelButtonImage != null && nextLevelSprite != null)
            nextLevelButtonImage.sprite = nextLevelSprite;

        SetTitleText("Level Tamamlandı!", new Color(0.15f, 0.68f, 0.38f));

        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(true);
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(() => GameManager.Instance.AdvanceLevel());
            SetButtonText(nextLevelButton, "Sonraki Level");
        }

        if (retryButton != null) retryButton.gameObject.SetActive(false);
    }

    private void SetNewChapterUnlockedMode()
    {
        pendingNextChapter = ChapterManager.GetChapter(GameManager.Instance.CurrentLevel) + 1;

        if (successImage != null) successImage.SetActive(true);
        if (failImage != null) failImage.SetActive(false);

        if (nextLevelButtonImage != null && nextChapterSprite != null)
            nextLevelButtonImage.sprite = nextChapterSprite;

        SetTitleText($"Bölüm {pendingNextChapter} Açıldı!", new Color(0.15f, 0.68f, 0.38f));

        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(true);
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(OnNextChapterButton);
            SetButtonText(nextLevelButton, $"Bölüm {pendingNextChapter}'e Geç");
        }

        if (retryButton != null) retryButton.gameObject.SetActive(false);
    }

    private void OnNextChapterButton()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (chapterUnlockAnimator != null)
        {
            chapterUnlockAnimator.PlayUnlockAnimation(pendingNextChapter, () =>
            {
                GameManager.Instance.AdvanceLevel();
            });
        }
        else
        {
            GameManager.Instance.AdvanceLevel();
        }
    }

    private void SetChapterLockedMode()
    {
        int currentChapter = ChapterManager.GetChapter(GameManager.Instance.CurrentLevel);
        int chapterStars = ChapterManager.GetChapterStars(currentChapter);
        int needed = ChapterManager.StarsRequiredToUnlock;

        if (successImage != null) successImage.SetActive(true);
        if (failImage != null) failImage.SetActive(false);

        SetTitleText("Bölüm Sonu", new Color(0.20f, 0.40f, 0.80f));

        if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(false);
        if (retryButton != null) retryButton.gameObject.SetActive(false);

        ShowStarsNeededPopup(needed, chapterStars);
    }

    private void SetRetryMode()
    {
        if (successImage != null) successImage.SetActive(false);
        if (failImage != null) failImage.SetActive(true);

        SetTitleText("Tekrar Dene", new Color(0.83f, 0.18f, 0.18f));

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(true);
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                GameManager.Instance.RetryLevel();
            });
        }

        if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(false);
    }

    // ──────────────────────────────────────────
    // Yıldız Gerekli Popup
    // ──────────────────────────────────────────

    private void ShowStarsNeededPopup(int needed, int current)
    {
        if (starsNeededPopup == null) return;

        if (starsNeededText != null)
        {
            starsNeededText.text =
                $"Sonraki bölüme geçmek için\n" +
                $"{needed} yıldız gerekli.\n\n" +
                $"Şu an: {current} yıldız\n\n" +
                $"Eksik yıldızları tamamlamak için\nbölüme geri dön!";
        }

        starsNeededPopup.SetActive(true);
    }

    public void CloseStarsNeededPopup()
    {
        if (starsNeededPopup != null)
            starsNeededPopup.SetActive(false);
    }

    // ──────────────────────────────────────────
    // İpucu Sayacı
    // ──────────────────────────────────────────

    public void UpdateHintCount()
    {
        int hints = PlayerPrefs.GetInt("HintCount", 0);
        string text = $"({hints})";

        if (wordHintCountText != null) wordHintCountText.text = text;
        if (mathHintCountText != null) mathHintCountText.text = text;
    }

    // ──────────────────────────────────────────
    // Can Göstergesi
    // ──────────────────────────────────────────

    public void UpdateLivesDisplay()
    {
        if (livesText == null) return;
        if (LivesManager.Instance == null) return;

        int lives = LivesManager.Instance.IsUnlimitedLives()
            ? LivesManager.MaxLives
            : LivesManager.Instance.GetLives();

        livesText.text = $"{lives}/{LivesManager.MaxLives}";
    }

    // ──────────────────────────────────────────
    // Yardımcılar
    // ──────────────────────────────────────────

    private void SetTitleText(string text, Color color)
    {
        if (resultTitleText == null) return;
        resultTitleText.text = text;
        resultTitleText.color = color;
    }

    private void SetButtonText(Button button, string text)
    {
        if (button == null) return;
        TMP_Text t = button.GetComponentInChildren<TMP_Text>();
        if (t != null) t.text = text;
    }

    // ──────────────────────────────────────────
    // HUD Güncelleme
    // ──────────────────────────────────────────

    public void UpdateTotalScore(int score)
    {
        if (totalScoreText != null)
            totalScoreText.text = "Puan: " + score;
    }

    public void UpdateLevelText(int globalLevel)
    {
        if (levelText == null) return;
        int chapter = ChapterManager.GetChapter(globalLevel);
        int levelInChapter = ChapterManager.GetLevelInChapter(globalLevel);
        levelText.text = $"Bölüm {chapter}  •  {levelInChapter}";
    }
}