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

    [Header("Mağaza")]
    public ShopManager shopManager;

    [Header("Görevler")]
    public QuestUI questUI;

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

        isSoundOn = PlayerPrefs.GetInt(SoundKey, 1) == 1;
        isMusicOn = PlayerPrefs.GetInt(MusicKey, 1) == 1;

        UpdateSoundButton();
        UpdateMusicButton();
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
    }

    public void OnAboutButton()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (aboutPopup == null) return;
        bool isOpen = aboutPopup.activeSelf;
        aboutPopup.SetActive(!isOpen);
        if (settingsPopup != null) settingsPopup.SetActive(false);
        if (comingSoonPopup != null) comingSoonPopup.SetActive(false);
    }

    public void CloseAllPopups()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (settingsPopup != null) settingsPopup.SetActive(false);
        if (aboutPopup != null) aboutPopup.SetActive(false);
        if (comingSoonPopup != null) comingSoonPopup.SetActive(false);
    }

    // ──────────────────────────────────────────
    // Mağaza
    // ──────────────────────────────────────────

    public void OnShopButton()
    {
        Debug.Log("OnShopButton çağrıldı!");
        Debug.Log("ShopManager.Instance: " + (ShopManager.Instance != null ? "VAR" : "NULL"));
        Debug.Log("shopManager: " + (shopManager != null ? "VAR" : "NULL"));

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
        if (LeaderboardManager.Instance != null)
            LeaderboardManager.Instance.ShowAllLeaderboards();
        else
            Debug.LogWarning("LeaderboardManager bağlı değil!");
    }

    // ──────────────────────────────────────────
    // Çok Yakında
    // ──────────────────────────────────────────

    private void ShowComingSoon(string featureName)
    {
        if (comingSoonPopup == null) return;
        if (settingsPopup != null) settingsPopup.SetActive(false);
        if (aboutPopup != null) aboutPopup.SetActive(false);

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

        if (isSoundOn && soundOnSprite != null)
            soundButtonImage.sprite = soundOnSprite;
        else if (!isSoundOn && soundOffSprite != null)
            soundButtonImage.sprite = soundOffSprite;
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

        if (isMusicOn && musicOnSprite != null)
            musicButtonImage.sprite = musicOnSprite;
        else if (!isMusicOn && musicOffSprite != null)
            musicButtonImage.sprite = musicOffSprite;
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
        Application.OpenURL("mailto:" + supportEmail + "?subject=Destek Talebi - Zihin Arenasi");
    }

    public void OnPrivacyPolicyButton() { Application.OpenURL(privacyPolicyURL); }
    public void OnTermsButton() { Application.OpenURL(termsURL); }
}