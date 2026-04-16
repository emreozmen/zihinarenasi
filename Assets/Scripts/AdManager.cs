using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

#if UNITY_ANDROID
    private const string InterstitialId = "ca-app-pub-3794471707334842/4620567249";
    private const string RewardedId     = "ca-app-pub-3794471707334842/5278775892";
    private const string BannerId       = "ca-app-pub-3794471707334842/4323640032";
#elif UNITY_IOS
    private const string InterstitialId = "ca-app-pub-3794471707334842/5797650640";
    private const string RewardedId     = "ca-app-pub-3794471707334842/1802565316";
    private const string BannerId       = "ca-app-pub-3794471707334842/3323075638";
#else
    private const string InterstitialId = "ca-app-pub-3940256099942544/1033173712";
    private const string RewardedId = "ca-app-pub-3940256099942544/5224354917";
    private const string BannerId = "ca-app-pub-3940256099942544/6300978111";
#endif

    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    private BannerView bannerView;
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
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("AdMob başlatıldı.");
            LoadInterstitial();
            LoadRewarded();
            LoadBanner();
        });
    }

    // ──────────────────────────────────────────
    // Banner
    // ──────────────────────────────────────────

    private void LoadBanner()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }

        bannerView = new BannerView(BannerId, AdSize.Banner, AdPosition.Bottom);

        bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner yüklendi.");
            bannerView.Show();
        };

        bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogWarning("Banner yüklenemedi: " + error);
        };

        var request = new AdRequest();
        bannerView.LoadAd(request);
    }

    public void ShowBanner()
    {
        if (bannerView != null)
            bannerView.Show();
    }

    public void HideBanner()
    {
        if (bannerView != null)
            bannerView.Hide();
    }

    // ──────────────────────────────────────────
    // Interstitial
    // ──────────────────────────────────────────

    private void LoadInterstitial()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        var request = new AdRequest();
        InterstitialAd.Load(InterstitialId, request, (ad, error) =>
        {
            if (error != null)
            {
                Debug.LogWarning("Interstitial yüklenemedi: " + error);
                return;
            }
            interstitialAd = ad;
            Debug.Log("Interstitial yüklendi.");
        });
    }

    public void ShowInterstitial(Action onClosed = null)
    {
        if (IAPManager.Instance != null &&
            (IAPManager.Instance.IsNoAdsPurchased() || IAPManager.Instance.IsVIPPurchased()))
        {
            onClosed?.Invoke();
            return;
        }

        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                onClosed?.Invoke();
                LoadInterstitial();
            };
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
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        var request = new AdRequest();
        RewardedAd.Load(RewardedId, request, (ad, error) =>
        {
            if (error != null)
            {
                Debug.LogWarning("Rewarded yüklenemedi: " + error);
                return;
            }
            rewardedAd = ad;
            Debug.Log("Rewarded yüklendi.");
        });
    }

    public void ShowRewarded(Action onRewardEarned)
    {
        this.onRewardEarned = onRewardEarned;

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                LoadRewarded();
            };
            rewardedAd.Show(reward =>
            {
                Debug.Log("Ödül kazanıldı!");
                this.onRewardEarned?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("Rewarded reklam hazır değil, direkt veriliyor.");
            onRewardEarned?.Invoke();
            LoadRewarded();
        }
    }

    // ──────────────────────────────────────────
    // Her 5 Levelda Bir Interstitial
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
    // Temizlik
    // ──────────────────────────────────────────

    private void OnDestroy()
    {
        if (bannerView != null) bannerView.Destroy();
        if (interstitialAd != null) interstitialAd.Destroy();
        if (rewardedAd != null) rewardedAd.Destroy();
    }
}