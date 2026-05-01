using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdManager Instance;

    // ──────────────────────────────────────────
    // Game ID'ler
    // ──────────────────────────────────────────

#if UNITY_ANDROID
    private const string GameId = "6101063";
    private const string InterstitialId = "Interstitial_Android";
    private const string RewardedId = "Rewarded_Android";
    private const string BannerId = "Banner_Android";
#elif UNITY_IOS
    private const string GameId         = "6101062";
    private const string InterstitialId = "Interstitial_iOS";
    private const string RewardedId     = "Rewarded_iOS";
    private const string BannerId       = "Banner_iOS";
#else
    private const string GameId         = "6101063";
    private const string InterstitialId = "Interstitial_Android";
    private const string RewardedId     = "Rewarded_Android";
    private const string BannerId       = "Banner_Android";
#endif

    private bool isInitialized = false;
    private bool interstitialReady = false;
    private bool rewardedReady = false;

    private Action onInterstitialClosed;
    private Action onRewardEarned;

    private BannerLoadOptions bannerLoadOptions;

    // ──────────────────────────────────────────
    // Başlangıç
    // ──────────────────────────────────────────

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
            return;
        }
    }

    private void Start()
    {
        // true = test modu
        Advertisement.Initialize(GameId, false, this);
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (isInitialized)
            ShowBanner();
    }

    // ──────────────────────────────────────────
    // Initialization Callbacks
    // ──────────────────────────────────────────

    public void OnInitializationComplete()
    {
        isInitialized = true;
        Debug.Log("Unity Ads başlatıldı.");
        LoadInterstitial();
        LoadRewarded();
        LoadBanner();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads başlatılamadı: {error} - {message}");
    }

    // ──────────────────────────────────────────
    // Banner
    // ──────────────────────────────────────────

    private void LoadBanner()
    {
        bannerLoadOptions = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        Advertisement.Banner.Load(BannerId, bannerLoadOptions);
    }

    private void OnBannerLoaded()
    {
        Debug.Log("Banner yüklendi.");
        ShowBanner();
    }

    private void OnBannerError(string message)
    {
        Debug.LogWarning("Banner yüklenemedi: " + message);
    }

    public void ShowBanner()
    {
        if (!isInitialized) return;

        var showOptions = new BannerOptions
        {
            showCallback = () => Debug.Log("Banner gösterildi."),
            hideCallback = () => Debug.Log("Banner gizlendi.")
        };
        Advertisement.Banner.Show(BannerId, showOptions);
    }

    public void HideBanner()
    {
        Advertisement.Banner.Hide();
    }

    // ──────────────────────────────────────────
    // Interstitial
    // ──────────────────────────────────────────

    private void LoadInterstitial()
    {
        interstitialReady = false;
        Advertisement.Load(InterstitialId, this);
    }

    public void ShowInterstitial(Action onClosed = null)
    {
        if (IAPManager.Instance != null &&
            (IAPManager.Instance.IsNoAdsPurchased() || IAPManager.Instance.IsVIPPurchased()))
        {
            onClosed?.Invoke();
            return;
        }

        if (!isInitialized || !interstitialReady)
        {
            onClosed?.Invoke();
            LoadInterstitial();
            return;
        }

        onInterstitialClosed = onClosed;
        Advertisement.Show(InterstitialId, this);
    }

    // ──────────────────────────────────────────
    // Rewarded
    // ──────────────────────────────────────────

    private void LoadRewarded()
    {
        rewardedReady = false;
        Advertisement.Load(RewardedId, this);
    }

    public void ShowRewarded(Action onReward)
    {
        if (!isInitialized || !rewardedReady)
        {
            Debug.LogWarning("Rewarded hazır değil, direkt veriliyor.");
            onReward?.Invoke();
            LoadRewarded();
            return;
        }

        onRewardEarned = onReward;
        Advertisement.Show(RewardedId, this);
    }

    // ──────────────────────────────────────────
    // Her 3 Levelda Bir Interstitial
    // ──────────────────────────────────────────

    public void OnLevelCompleted(int globalLevel, Action onClosed = null)
    {
        if (IAPManager.Instance != null &&
            (IAPManager.Instance.IsNoAdsPurchased() || IAPManager.Instance.IsVIPPurchased()))
        {
            onClosed?.Invoke();
            return;
        }

        if (globalLevel % 3 == 0)
            ShowInterstitial(onClosed);
        else
            onClosed?.Invoke();
    }

    // ──────────────────────────────────────────
    // IUnityAdsLoadListener
    // ──────────────────────────────────────────

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Reklam yüklendi: " + placementId);

        if (placementId == InterstitialId)
            interstitialReady = true;
        else if (placementId == RewardedId)
            rewardedReady = true;
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"Reklam yüklenemedi: {placementId} | {error} | {message}");
    }

    // ──────────────────────────────────────────
    // IUnityAdsShowListener
    // ──────────────────────────────────────────

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"Reklam tamamlandı: {placementId} | {showCompletionState}");

        if (placementId == InterstitialId)
        {
            interstitialReady = false;
            onInterstitialClosed?.Invoke();
            onInterstitialClosed = null;
            LoadInterstitial();
        }
        else if (placementId == RewardedId)
        {
            rewardedReady = false;

            if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
            {
                Debug.Log("Ödül kazanıldı!");
                onRewardEarned?.Invoke();
            }

            onRewardEarned = null;
            LoadRewarded();
        }
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning($"Reklam gösterilemedi: {placementId} | {error} | {message}");

        if (placementId == InterstitialId)
        {
            onInterstitialClosed?.Invoke();
            onInterstitialClosed = null;
            LoadInterstitial();
        }
        else if (placementId == RewardedId)
        {
            onRewardEarned?.Invoke();
            onRewardEarned = null;
            LoadRewarded();
        }
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log("Reklam başladı: " + placementId);
        HideBanner();
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("Reklama tıklandı: " + placementId);
    }
}