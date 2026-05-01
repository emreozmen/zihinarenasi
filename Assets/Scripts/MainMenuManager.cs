using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Popups")]
    public GameObject settingsPopup;
    public GameObject aboutPopup;
    public GameObject comingSoonPopup;

    [Header("Leaderboard Popup")]
    public GameObject leaderboardPopup;
    public Button totalScoreTabButton;
    public Button totalStarsTabButton;
    public Button leaderboardCloseButton;

    [Header("Tab Renkleri")]
    public Color activeTabColor = new Color(0.20f, 0.60f, 0.86f);
    public Color inactiveTabColor = new Color(0.17f, 0.24f, 0.31f);

    [Header("Mağaza")]
    public ShopManager shopManager;

    [Header("Görevler")]
    public QuestUI questUI;

    [Header("Oyuncu ID")]
    public TMP_Text playerIdText;
    public Button copyPlayerIdButton;

    [Header("Çok Yakında Popup")]
    public TMP_Text comingSoonTitleText;

    [Header("Ses Butonu")]
    public Button soundButton;
    public Image soundButtonImage;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    [Header("Müzik Butonu")]
    public Button musicButton;
    public Image musicButtonImage;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    [Header("Sosyal Medya Linkleri")]
    public string instagramURL = "https://instagram.com/zihinarenasi";
    public string twitterURL = "https://twitter.com/zihinarenasi";
    public string youtubeURL = "https://youtube.com/@zihinarenasi2026";
    public string facebookURL = "https://www.facebook.com/profile.php?id=61573478978251";

    [Header("Destek & Yasal")]
    public string supportEmail = "zihinarenasi@gmail.com";
    public string privacyPolicyURL = "https://emreozmen.github.io/zihinarenasi/privacy";
    public string termsURL = "https://emreozmen.github.io/zihinarenasi/terms";

    private const string SoundKey = "SoundEnabled";
    private const string MusicKey = "MusicEnabled";

    private bool isSoundOn;
    private bool isMusicOn;

    // ──────────────────────────────────────────
    // Başlangıç
    // ──────────────────────────────────────────

    private void Start()
    {
        if (shopManager == null)
            shopManager = ShopManager.Instance;

        if (settingsPopup != null) settingsPopup.SetActive(false);
        if (aboutPopup != null) aboutPopup.SetActive(false);
        if (comingSoonPopup != null) comingSoonPopup.SetActive(false);
        if (leaderboardPopup != null) leaderboardPopup.SetActive(false);

        // Leaderboard tab butonları
        if (totalScoreTabButton != null)
            totalScoreTabButton.onClick.AddListener(() => ShowLeaderboardTab(0));
        if (totalStarsTabButton != null)
            totalStarsTabButton.onClick.AddListener(() => ShowLeaderboardTab(1));
        if (leaderboardCloseButton != null)
            leaderboardCloseButton.onClick.AddListener(CloseLeaderboard);

        // Oyuncu ID
        if (copyPlayerIdButton != null)
            copyPlayerIdButton.onClick.AddListener(CopyPlayerId);

        isSoundOn = PlayerPrefs.GetInt(SoundKey, 1) == 1;
        isMusicOn = PlayerPrefs.GetInt(MusicKey, 1) == 1;

        UpdateSoundButton();
        UpdateMusicButton();

        // Firebase hazır olunca ID'yi göster
        InvokeRepeating("TryUpdatePlayerId", 1f, 1f);
    }

    // ──────────────────────────────────────────
    // Oyuncu ID
    // ──────────────────────────────────────────

    private void TryUpdatePlayerId()
    {
        if (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsReady()) return;

        string playerId = FirebaseManager.Instance.GetPlayerId();
        if (playerIdText != null)
            playerIdText.text = $"ID: {ShortenId(playerId)}";

        CancelInvoke("TryUpdatePlayerId");
    }

    private string ShortenId(string id)
    {
        if (string.IsNullOrEmpty(id)) return "...";
        if (id.Length <= 12) return id;
        return id.Substring(0, 6) + "..." + id.Substring(id.Length - 4);
    }

    private void CopyPlayerId()
    {
        if (FirebaseManager.Instance == null) return;

        string playerId = FirebaseManager.Instance.GetPlayerId();
        GUIUtility.systemCopyBuffer = playerId;

        if (playerIdText != null)
            playerIdText.text = "Kopyalandı!";

        Invoke("RestorePlayerIdText", 2f);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
    }

    private void RestorePlayerIdText()
    {
        if (FirebaseManager.Instance == null) return;
        string playerId = FirebaseManager.Instance.GetPlayerId();
        if (playerIdText != null)
            playerIdText.text = $"ID: {ShortenId(playerId)}";
    }

    // ──────────────────────────────────────────
    // Ana Butonlar
    // ──────────────────────────────────────────

    public void OnPlayButton()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        int currentLevel = ChapterManager.LoadCurrentLevel();
        int chapter = ChapterManager.GetChapter(currentLevel);
        ChapterManager.LoadChapterScene(chapter);
    }

    public void OnChaptersButton()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        SceneManager.LoadScene("LevelSelect");
    }

    public void OnSettingsButton()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (settingsPopup == null) return;
        bool isOpen = settingsPopup.activeSelf;
        settingsPopup.SetActive(!isOpen);
        if (aboutPopup != null) aboutPopup.SetActive(false);
        if (comingSoonPopup != null) comingSoonPopup.SetActive(false);
        if (leaderboardPopup != null) leaderboardPopup.SetActive(false);
    }

    public void OnAboutButton()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (aboutPopup == null) return;
        bool isOpen = aboutPopup.activeSelf;
        aboutPopup.SetActive(!isOpen);
        if (settingsPopup != null) settingsPopup.SetActive(false);
        if (comingSoonPopup != null) comingSoonPopup.SetActive(false);
        if (leaderboardPopup != null) leaderboardPopup.SetActive(false);
    }

    public void CloseAllPopups()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (settingsPopup != null) settingsPopup.SetActive(false);
        if (aboutPopup != null) aboutPopup.SetActive(false);
        if (comingSoonPopup != null) comingSoonPopup.SetActive(false);
        if (leaderboardPopup != null) leaderboardPopup.SetActive(false);
    }

    // ──────────────────────────────────────────
    // Mağaza
    // ──────────────────────────────────────────

    public void OnShopButton()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.OpenShop();
        else if (shopManager != null)
            shopManager.OpenShop();
        else
            Debug.LogWarning("ShopManager bağlı değil!");
    }

    // ──────────────────────────────────────────
    // Görevler
    // ──────────────────────────────────────────

    public void OnQuestsButton()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (questUI != null) questUI.OpenQuests();
        else Debug.LogWarning("QuestUI bağlı değil!");
    }

    // ──────────────────────────────────────────
    // Sıralama
    // ──────────────────────────────────────────

    public void OnLeaderboardButton()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (leaderboardPopup == null)
        {
            if (LeaderboardManager.Instance != null)
                LeaderboardManager.Instance.ShowAllLeaderboards();
            return;
        }

        leaderboardPopup.SetActive(true);
        if (settingsPopup != null) settingsPopup.SetActive(false);
        if (aboutPopup != null) aboutPopup.SetActive(false);
        if (comingSoonPopup != null) comingSoonPopup.SetActive(false);

        SetTabColor(totalScoreTabButton, true);
        SetTabColor(totalStarsTabButton, false);
    }

    public void ShowLeaderboardTab(int index)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        SetTabColor(totalScoreTabButton, index == 0);
        SetTabColor(totalStarsTabButton, index == 1);

        if (index == 0 && LeaderboardManager.Instance != null)
            LeaderboardManager.Instance.ShowTotalScoreLeaderboard();
        else if (index == 1 && LeaderboardManager.Instance != null)
            LeaderboardManager.Instance.ShowTotalStarsLeaderboard();
    }

    public void CloseLeaderboard()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (leaderboardPopup != null) leaderboardPopup.SetActive(false);
    }

    private void SetTabColor(Button button, bool isActive)
    {
        if (button == null) return;
        Image img = button.GetComponent<Image>();
        if (img != null)
            img.color = isActive ? activeTabColor : inactiveTabColor;
    }

    // ──────────────────────────────────────────
    // Çok Yakında
    // ──────────────────────────────────────────

    private void ShowComingSoon(string featureName)
    {
        if (comingSoonPopup == null) return;
        if (settingsPopup != null) settingsPopup.SetActive(false);
        if (aboutPopup != null) aboutPopup.SetActive(false);
        if (leaderboardPopup != null) leaderboardPopup.SetActive(false);

        if (comingSoonTitleText != null)
            comingSoonTitleText.text = $"{featureName}\nÇok Yakında!";

        comingSoonPopup.SetActive(true);
    }

    // ──────────────────────────────────────────
    // Ses
    // ──────────────────────────────────────────

    public void OnSoundButton()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt(SoundKey, isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
        UpdateSoundButton();
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSound(isSoundOn);
    }

    private void UpdateSoundButton()
    {
        if (soundButtonImage == null) return;
        soundButtonImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
    }

    // ──────────────────────────────────────────
    // Müzik
    // ──────────────────────────────────────────

    public void OnMusicButton()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt(MusicKey, isMusicOn ? 1 : 0);
        PlayerPrefs.Save();
        UpdateMusicButton();
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusic(isMusicOn);
    }

    private void UpdateMusicButton()
    {
        if (musicButtonImage == null) return;
        musicButtonImage.sprite = isMusicOn ? musicOnSprite : musicOffSprite;
    }

    // ──────────────────────────────────────────
    // Sosyal Medya
    // ──────────────────────────────────────────

    public void OnInstagramButton() { Application.OpenURL(instagramURL); }
    public void OnTwitterButton() { Application.OpenURL(twitterURL); }
    public void OnYoutubeButton() { Application.OpenURL(youtubeURL); }
    public void OnFacebookButton() { Application.OpenURL(facebookURL); }

    // ──────────────────────────────────────────
    // Destek & Yasal
    // ──────────────────────────────────────────

    public void OnSupportButton()
    {
        string playerId = FirebaseManager.Instance != null
            ? FirebaseManager.Instance.GetPlayerId()
            : "unknown";

        string subject = "Destek Talebi - Zihin Arenasi";
        string body = $"Oyuncu ID: {playerId}%0A%0ASorunum:%0A";

        Application.OpenURL($"mailto:{supportEmail}?subject={subject}&body={body}");
    }

    public void OnPrivacyPolicyButton() { Application.OpenURL(privacyPolicyURL); }
    public void OnTermsButton() { Application.OpenURL(termsURL); }
}