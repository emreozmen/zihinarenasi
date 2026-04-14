using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Popup")]
    public GameObject shopPopup;

    [Header("Tab Panelleri")]
    public GameObject noAdsPanel;
    public GameObject hintPanel;
    public GameObject vipPanel;
    public GameObject supportPanel;
    public GameObject livesPanel;

    [Header("Tab Butonları")]
    public Button noAdsTabButton;
    public Button hintTabButton;
    public Button vipTabButton;
    public Button supportTabButton;
    public Button livesTabButton;

    [Header("Tab Renkleri")]
    public Color activeTabColor = new Color(0.20f, 0.60f, 0.86f);
    public Color inactiveTabColor = new Color(0.17f, 0.24f, 0.31f);

    [Header("Satın Alma Butonları")]
    public Button buyNoAdsButton;
    public Button buyHintButton;
    public Button buyVIPButton;
    public Button buySupportButton;
    public Button buyLivesButton;

    [Header("Kapat Butonu")]
    public Button closeButton;

    [Header("Durum Metinleri")]
    public TMP_Text noAdsStatusText;
    public TMP_Text hintCountText;
    public TMP_Text vipStatusText;
    public TMP_Text livesCountText;

    [Header("Geri Yükle (iOS)")]
    public Button restoreButton;

    // ──────────────────────────────────────────
    // Başlangıç
    // ──────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ShopPopup Canvas'ını da koru
            if (shopPopup != null)
                DontDestroyOnLoad(shopPopup.transform.root.gameObject);
        }
        else
        {
            // Zaten bir instance var — bu objeyi yok et
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (shopPopup != null) shopPopup.SetActive(false);

        if (noAdsTabButton != null) noAdsTabButton.onClick.AddListener(() => ShowTab(0));
        if (hintTabButton != null) hintTabButton.onClick.AddListener(() => ShowTab(1));
        if (vipTabButton != null) vipTabButton.onClick.AddListener(() => ShowTab(2));
        if (supportTabButton != null) supportTabButton.onClick.AddListener(() => ShowTab(3));
        if (livesTabButton != null) livesTabButton.onClick.AddListener(() => ShowTab(4));

        if (buyNoAdsButton != null) buyNoAdsButton.onClick.AddListener(OnBuyNoAds);
        if (buyHintButton != null) buyHintButton.onClick.AddListener(OnBuyHint);
        if (buyVIPButton != null) buyVIPButton.onClick.AddListener(OnBuyVIP);
        if (buySupportButton != null) buySupportButton.onClick.AddListener(OnBuySupport);
        if (buyLivesButton != null) buyLivesButton.onClick.AddListener(OnBuyLives);

        if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
        if (restoreButton != null) restoreButton.onClick.AddListener(OnRestore);
    }

    // ──────────────────────────────────────────
    // Mağaza Aç/Kapat
    // ──────────────────────────────────────────

    public void OpenShop()
    {
        if (shopPopup != null)
        {
            // Canvas sort order'ı garantiye al
            Canvas canvas = shopPopup.GetComponentInParent<Canvas>();
            if (canvas != null) canvas.sortingOrder = 200;

            shopPopup.SetActive(true);
        }
        ShowTab(0);
        UpdatePurchaseStatus();
    }

    public void CloseShop()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (shopPopup != null) shopPopup.SetActive(false);

        // Sort order'ı sıfırla
        Canvas canvas = shopPopup.GetComponentInParent<Canvas>();
        if (canvas != null) canvas.sortingOrder = 0;
    }

    // ──────────────────────────────────────────
    // Tab Sistemi
    // ──────────────────────────────────────────

    public void ShowTab(int index)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (noAdsPanel != null) noAdsPanel.SetActive(false);
        if (hintPanel != null) hintPanel.SetActive(false);
        if (vipPanel != null) vipPanel.SetActive(false);
        if (supportPanel != null) supportPanel.SetActive(false);
        if (livesPanel != null) livesPanel.SetActive(false);

        SetTabColor(noAdsTabButton, false);
        SetTabColor(hintTabButton, false);
        SetTabColor(vipTabButton, false);
        SetTabColor(supportTabButton, false);
        SetTabColor(livesTabButton, false);

        switch (index)
        {
            case 0:
                if (noAdsPanel != null) noAdsPanel.SetActive(true);
                SetTabColor(noAdsTabButton, true);
                break;
            case 1:
                if (hintPanel != null) hintPanel.SetActive(true);
                SetTabColor(hintTabButton, true);
                break;
            case 2:
                if (vipPanel != null) vipPanel.SetActive(true);
                SetTabColor(vipTabButton, true);
                break;
            case 3:
                if (supportPanel != null) supportPanel.SetActive(true);
                SetTabColor(supportTabButton, true);
                break;
            case 4:
                if (livesPanel != null) livesPanel.SetActive(true);
                SetTabColor(livesTabButton, true);
                break;
        }

        UpdatePurchaseStatus();
    }

    private void SetTabColor(Button button, bool isActive)
    {
        if (button == null) return;
        Image img = button.GetComponent<Image>();
        if (img != null)
            img.color = isActive ? activeTabColor : inactiveTabColor;
    }

    // ──────────────────────────────────────────
    // Satın Alma Durumu
    // ──────────────────────────────────────────

    private void UpdatePurchaseStatus()
    {
        if (IAPManager.Instance == null) return;

        bool noAdsPurchased = IAPManager.Instance.IsNoAdsPurchased();
        if (noAdsStatusText != null)
            noAdsStatusText.text = noAdsPurchased ? "Aktif ✓" : "99₺";
        if (buyNoAdsButton != null)
            buyNoAdsButton.interactable = !noAdsPurchased;

        if (hintCountText != null)
        {
            int count = IAPManager.Instance.GetHintCount();
            hintCountText.text = $"Mevcut İpucu: {count}";
        }

        bool vipPurchased = IAPManager.Instance.IsVIPPurchased();
        if (vipStatusText != null)
            vipStatusText.text = vipPurchased ? "Aktif ✓" : "149₺";
        if (buyVIPButton != null)
            buyVIPButton.interactable = !vipPurchased;

        if (livesCountText != null)
        {
            int lives = LivesManager.Instance != null
                ? LivesManager.Instance.GetLives()
                : LivesManager.MaxLives;
            livesCountText.text = $"Mevcut Can: {lives}/{LivesManager.MaxLives}";
        }

        if (buyLivesButton != null)
            buyLivesButton.interactable = true;
    }

    // ──────────────────────────────────────────
    // Satın Alma
    // ──────────────────────────────────────────

    private void OnBuyNoAds()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (IAPManager.Instance != null) IAPManager.Instance.BuyNoAds();
    }

    private void OnBuyHint()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (IAPManager.Instance != null) IAPManager.Instance.BuyHintPack();
    }

    private void OnBuyVIP()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (IAPManager.Instance != null) IAPManager.Instance.BuyVIP();
    }

    private void OnBuySupport()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (IAPManager.Instance != null) IAPManager.Instance.BuySupport();
    }

    private void OnBuyLives()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (IAPManager.Instance != null) IAPManager.Instance.BuyLivesPack();
    }

    private void OnRestore()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (IAPManager.Instance != null) IAPManager.Instance.RestorePurchases();
    }
}