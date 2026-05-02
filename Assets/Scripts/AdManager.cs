using System;
using UnityEngine;
using GoogleMobileAds.Api;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

#if UNITY_ANDROID
    private const string BannerId       = "ca-app-pub-3794471707334842/4323640032";
    private const string InterstitialId = "ca-app-pub-3794471707334842/4620567249";
    private const string RewardedId     = "ca-app-pub-3794471707334842/5278775892";
#elif UNITY_IOS
    private const string BannerId       = "ca-app-pub-3794471707334842/3323075638";
    private const string InterstitialId = "ca-app-pub-3794471707334842/5797650640";
    private const string RewardedId     = "ca-app-pub-3794471707334842/1802565316";
#else
    private const string BannerId       = "ca-app-pub-3794471707334842/4323640032";
    private const string InterstitialId = "ca-app-pub-3794471707334842/4620567249";
    private const string RewardedId     = "ca-app-pub-3794471707334842/5278775892";
#endif

    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;

    private Action onInterstitialClosed;
    private Action onRewardEarned;

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
        MobileAds.Initialize(_ =>
        {
            Debug.Log("AdMob başlatıldı.");
            LoadInterstitial();
            LoadRewarded();
            LoadBanner();
        });
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
        ShowBanner();
    }

    // ──────────────────────────────────────────
    // Banner
    // ──────────────────────────────────────────

    private void LoadBanner()
    {
        bannerView?.Destroy();
        bannerView = new BannerView(BannerId, AdSize.Banner, AdPosition.Bottom);
        bannerView.LoadAd(new AdRequest());
    }

    public void ShowBanner()
    {
        if (IsAdsRemoved()) { HideBanner(); return; }
        bannerView?.Show();
    }

    public void HideBanner()
    {
        bannerView?.Hide();
    }

    // ──────────────────────────────────────────
    // Interstitial
    // ──────────────────────────────────────────

    private void LoadInterstitial()
    {
        InterstitialAd.Load(InterstitialId, new AdRequest(), (ad, error) =>
        {
            if (error != null) { Debug.LogWarning("Interstitial yüklenemedi: " + error); return; }
            interstitialAd = ad;
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                onInterstitialClosed?.Invoke();
                onInterstitialClosed = null;
                LoadInterstitial();
            };
            interstitialAd.OnAdFullScreenContentFailed += _ =>
            {
                onInterstitialClosed?.Invoke();
                onInterstitialClosed = null;
                LoadInterstitial();
            };
            Debug.Log("Interstitial yüklendi.");
        });
    }

    public void ShowInterstitial(Action onClosed = null)
    {
        if (IsAdsRemoved()) { onClosed?.Invoke(); return; }

        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            onInterstitialClosed = onClosed;
            interstitialAd.Show();
        }
        else
        {
            onClosed?.Invoke();
            LoadInterstitial();
        }
    }

    // ──────────────────────────────────────────
    // Rewarded
    // ──────────────────────────────────────────

    private void LoadRewarded()
    {
        RewardedAd.Load(RewardedId, new AdRequest(), (ad, error) =>
        {
            if (error != null) { Debug.LogWarning("Rewarded yüklenemedi: " + error); return; }
            rewardedAd = ad;
            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                onRewardEarned = null;
                LoadRewarded();
            };
            rewardedAd.OnAdFullScreenContentFailed += _ =>
            {
                onRewardEarned?.Invoke();
                onRewardEarned = null;
                LoadRewarded();
            };
            Debug.Log("Rewarded yüklendi.");
        });
    }

    public void ShowRewarded(Action onReward)
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            onRewardEarned = onReward;
            rewardedAd.Show(_ =>
            {
                onRewardEarned?.Invoke();
                onRewardEarned = null;
            });
        }
        else
        {
            Debug.LogWarning("Rewarded hazır değil, direkt veriliyor.");
            onReward?.Invoke();
            LoadRewarded();
        }
    }

    // ──────────────────────────────────────────
    // Her 3 Levelda Bir Interstitial
    // ──────────────────────────────────────────

    public void OnLevelCompleted(int globalLevel, Action onClosed = null)
    {
        if (IsAdsRemoved()) { onClosed?.Invoke(); return; }

        if (globalLevel % 3 == 0)
            ShowInterstitial(onClosed);
        else
            onClosed?.Invoke();
    }

    // ──────────────────────────────────────────
    // Yardımcı
    // ──────────────────────────────────────────

    private bool IsAdsRemoved()
    {
        return IAPManager.Instance != null &&
               (IAPManager.Instance.IsNoAdsPurchased() || IAPManager.Instance.IsVIPPurchased());
    }
}
