using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    private const string LeaderboardTotalScore = "CgkI5Ob3ucYeEAIQAQ";
    private const string LeaderboardTotalStars = "CgkI5Ob3ucYeEAIQAg";

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
        InitializePlayGames();
    }

    // ──────────────────────────────────────────
    // Başlatma
    // ──────────────────────────────────────────

    private void InitializePlayGames()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Activate();
        SignIn();
#endif
    }

    private void SignIn()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.ManuallyAuthenticate(success =>
        {
            if (success == SignInStatus.Success)
                Debug.Log("Google Play Games: Giriş başarılı!");
            else
                Debug.LogWarning("Google Play Games: Giriş başarısız — " + success);
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
        Social.ReportScore(score, LeaderboardTotalScore, success =>
        {
            Debug.Log("Toplam puan gönderildi: " + score + " | Başarılı: " + success);
        });
#endif
    }

    public void SubmitTotalStars(int stars)
    {
#if UNITY_ANDROID
        if (!PlayGamesPlatform.Instance.IsAuthenticated()) return;
        Social.ReportScore(stars, LeaderboardTotalStars, success =>
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
        if (!PlayGamesPlatform.Instance.IsAuthenticated()) { SignIn(); return; }
        PlayGamesPlatform.Instance.ShowLeaderboardUI(LeaderboardTotalScore);
#endif
    }

    public void ShowTotalStarsLeaderboard()
    {
#if UNITY_ANDROID
        if (!PlayGamesPlatform.Instance.IsAuthenticated()) { SignIn(); return; }
        PlayGamesPlatform.Instance.ShowLeaderboardUI(LeaderboardTotalStars);
#endif
    }

    public void ShowAllLeaderboards()
    {
#if UNITY_ANDROID
        if (!PlayGamesPlatform.Instance.IsAuthenticated()) { SignIn(); return; }
        PlayGamesPlatform.Instance.ShowLeaderboardUI();
#endif
    }

    // ──────────────────────────────────────────
    // Giriş Durumu
    // ──────────────────────────────────────────

    public bool IsSignedIn()
    {
#if UNITY_ANDROID
        return PlayGamesPlatform.Instance.IsAuthenticated();
#else
        return false;
#endif
    }
}