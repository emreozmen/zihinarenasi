using System;
using System.Collections;
using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    // Android - Google Play Games
    private const string AndroidLeaderboardTotalScore = "CgkI5Ob3ucYeEAIQAQ";
    private const string AndroidLeaderboardTotalStars = "CgkI5Ob3ucYeEAIQAg";

    // iOS - Game Center
    private const string IOSLeaderboardTotalScore = "com.zihinarenasi.leaderboard.totalscore";
    private const string IOSLeaderboardTotalStars = "com.zihinarenasi.leaderboard.totalstars";

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
#if UNITY_ANDROID
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.ManuallyAuthenticate(_ => { });
#elif UNITY_IOS
        Social.localUser.Authenticate(success =>
        {
            Debug.Log("Game Center giriş: " + success);
        });
#endif
    }

    // ──────────────────────────────────────────
    // Skor Gönder
    // ──────────────────────────────────────────

    public void SubmitTotalScore(int score)
    {
#if UNITY_ANDROID
        if (!PlayGamesPlatform.Instance.IsAuthenticated()) return;
        PlayGamesPlatform.Instance.ReportScore(score, AndroidLeaderboardTotalScore, null);
#elif UNITY_IOS
        Social.ReportScore(score, IOSLeaderboardTotalScore, success =>
        {
            Debug.Log("Toplam puan gönderildi: " + score + " | Başarılı: " + success);
        });
#endif
    }

    public void SubmitTotalStars(int stars)
    {
#if UNITY_ANDROID
        if (!PlayGamesPlatform.Instance.IsAuthenticated()) return;
        PlayGamesPlatform.Instance.ReportScore(stars, AndroidLeaderboardTotalStars, null);
#elif UNITY_IOS
        Social.ReportScore(stars, IOSLeaderboardTotalStars, success =>
        {
            Debug.Log("Toplam yıldız gönderildi: " + stars + " | Başarılı: " + success);
        });
#endif
    }

    // ──────────────────────────────────────────
    // Leaderboard Göster
    // ──────────────────────────────────────────

    public void ShowTotalScoreLeaderboard()
    {
#if UNITY_ANDROID
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            SignInAndroid(() => PlayGamesPlatform.Instance.ShowLeaderboardUI(AndroidLeaderboardTotalScore));
            return;
        }
        PlayGamesPlatform.Instance.ShowLeaderboardUI(AndroidLeaderboardTotalScore);
#elif UNITY_IOS
        Social.ShowLeaderboardUI();
#endif
    }

    public void ShowTotalStarsLeaderboard()
    {
#if UNITY_ANDROID
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            SignInAndroid(() => PlayGamesPlatform.Instance.ShowLeaderboardUI(AndroidLeaderboardTotalStars));
            return;
        }
        PlayGamesPlatform.Instance.ShowLeaderboardUI(AndroidLeaderboardTotalStars);
#elif UNITY_IOS
        Social.ShowLeaderboardUI();
#endif
    }

    public void ShowAllLeaderboards()
    {
#if UNITY_ANDROID
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            SignInAndroid(() => PlayGamesPlatform.Instance.ShowLeaderboardUI());
            return;
        }
        PlayGamesPlatform.Instance.ShowLeaderboardUI();
#elif UNITY_IOS
        Social.ShowLeaderboardUI();
#endif
    }

    // ──────────────────────────────────────────
    // Android Giriş
    // ──────────────────────────────────────────

#if UNITY_ANDROID
    private void SignInAndroid(Action onSuccess)
    {
        PlayGamesPlatform.Instance.ManuallyAuthenticate(status =>
        {
            if (status == SignInStatus.Success)
                StartCoroutine(ShowAfterDelay(onSuccess));
        });
    }

    private IEnumerator ShowAfterDelay(Action action)
    {
        yield return new WaitForSeconds(0.5f);
        action?.Invoke();
    }
#endif

    // ──────────────────────────────────────────
    // Giriş Durumu
    // ──────────────────────────────────────────

    public bool IsSignedIn()
    {
#if UNITY_ANDROID
        return PlayGamesPlatform.Instance.IsAuthenticated();
#elif UNITY_IOS
        return Social.localUser.authenticated;
#else
        return false;
#endif
    }
}
