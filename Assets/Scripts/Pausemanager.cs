using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Pause Butonu")]
    public Button pauseButton;

    [Header("Pause Paneli")]
    public GameObject pausePanel;

    [Header("Pause İçi Butonlar")]
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Button levelSelectButton;

    [Header("Ses Butonu")]
    public Button soundButton;
    public Image soundButtonImage;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    private bool isPaused = false;
    private const string SoundKey = "SoundEnabled";
    private bool isSoundOn;

    // ──────────────────────────────────────────
    // Başlangıç
    // ──────────────────────────────────────────

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        isSoundOn = PlayerPrefs.GetInt(SoundKey, 1) == 1;
        UpdateSoundButton();

        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartWithAd);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (levelSelectButton != null)
            levelSelectButton.onClick.AddListener(GoToLevelSelect);

        if (soundButton != null)
            soundButton.onClick.AddListener(ToggleSound);
    }

    // ──────────────────────────────────────────
    // Pause Aç/Kapat
    // ──────────────────────────────────────────

    public void TogglePause()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (isPaused) Resume();
        else Pause();
    }

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void Resume()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    // ──────────────────────────────────────────
    // Tekrar Başla — Rewarded Reklam
    // ──────────────────────────────────────────

    private void OnRestartWithAd()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        Time.timeScale = 1f;

        if (AdManager.Instance != null)
        {
            AdManager.Instance.ShowRewarded(() =>
            {
                ChapterManager.ReloadCurrentScene();
            });
        }
        else
        {
            ChapterManager.ReloadCurrentScene();
        }
    }

    // ──────────────────────────────────────────
    // Navigasyon
    // ──────────────────────────────────────────

    private void GoToMainMenu()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void GoToLevelSelect()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelSelect");
    }

    // ──────────────────────────────────────────
    // Ses
    // ──────────────────────────────────────────

    private void ToggleSound()
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
    // Güvenlik
    // ──────────────────────────────────────────

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}