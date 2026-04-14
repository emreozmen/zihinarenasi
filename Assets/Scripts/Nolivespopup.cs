using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoLivesPopup : MonoBehaviour
{
    public static NoLivesPopup Instance;

    [Header("Popup")]
    public GameObject popupPanel;

    [Header("Metinler")]
    public TMP_Text titleText;
    public TMP_Text timerText;
    public TMP_Text livesText;

    [Header("Butonlar")]
    public Button watchAdButton;
    public Button shopButton;
    public Button closeButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
            if (canvas != null)
                DontDestroyOnLoad(canvas.gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (popupPanel != null) popupPanel.SetActive(false);

        if (watchAdButton != null)
            watchAdButton.onClick.AddListener(OnWatchAd);

        if (shopButton != null)
            shopButton.onClick.AddListener(OnShop);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnClose);
    }

    private void Update()
    {
        if (popupPanel != null && popupPanel.activeSelf)
            UpdateTimer();
    }

    // ──────────────────────────────────────────
    // Aç
    // ──────────────────────────────────────────

    public void Show(Action onLivesAvailable = null)
    {
        if (popupPanel != null)
        {
            Canvas canvas = popupPanel.GetComponentInParent<Canvas>();
            if (canvas != null) canvas.sortingOrder = 300;
            popupPanel.SetActive(true);
        }

        if (titleText != null)
            titleText.text = "Canın Bitti!";

        UpdateTimer();
    }

    // ──────────────────────────────────────────
    // Zamanlayıcı
    // ──────────────────────────────────────────

    private void UpdateTimer()
    {
        if (LivesManager.Instance == null) return;

        int lives = LivesManager.Instance.GetLives();

        if (livesText != null)
            livesText.text = $"Can: {lives}/{LivesManager.MaxLives}";

        TimeSpan remaining = LivesManager.Instance.GetTimeUntilNextLife();

        if (timerText != null)
        {
            timerText.text = remaining > TimeSpan.Zero
                ? $"Sonraki can: {remaining.Minutes:00}:{remaining.Seconds:00}"
                : "Can yükleniyor...";
        }
    }

    // ──────────────────────────────────────────
    // Reklam İzle
    // ──────────────────────────────────────────

    private void OnWatchAd()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (AdManager.Instance != null)
        {
            AdManager.Instance.ShowRewarded(() =>
            {
                if (LivesManager.Instance != null)
                    LivesManager.Instance.AddLife(1);

                // Panelleri gizle
                UIManager uiManager = FindObjectOfType<UIManager>();
                if (uiManager != null) uiManager.HideAllPanels();

                Hide();
                ChapterManager.ReloadCurrentScene();
            });
        }
        else
        {
            if (LivesManager.Instance != null)
                LivesManager.Instance.AddLife(1);

            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null) uiManager.HideAllPanels();

            Hide();
            ChapterManager.ReloadCurrentScene();
        }
    }

    // ──────────────────────────────────────────
    // Mağaza
    // ──────────────────────────────────────────

    private void OnShop()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        Hide();

        if (ShopManager.Instance != null)
            ShopManager.Instance.OpenShop();
    }

    // ──────────────────────────────────────────
    // Kapat — Ana Menüye Git
    // ──────────────────────────────────────────

    private void OnClose()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        popupPanel.SetActive(false);
        ChapterManager.LoadScene("MainMenu");
    }

    // ──────────────────────────────────────────
    // Kapat
    // ──────────────────────────────────────────

    public void Hide()
    {
        if (popupPanel != null) popupPanel.SetActive(false);

        if (popupPanel != null)
        {
            Canvas canvas = popupPanel.GetComponentInParent<Canvas>();
            if (canvas != null) canvas.sortingOrder = 0;
        }
    }
}