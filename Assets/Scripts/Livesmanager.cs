using System;
using UnityEngine;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance;

    public const int MaxLives = 5;

    // 1. can: 5 dk, 2-5. canlar: 15 dk
    private static readonly int[] RechargeMinutes = { 5, 15, 15, 15, 15 };

    private const string LivesKey = "LivesCount";
    private const string LastLostTimeKey = "LastLostTime";

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
        RechargeIfNeeded();
    }

    private void Update()
    {
        if (Time.frameCount % 1800 == 0)
            RechargeIfNeeded();
    }

    // ──────────────────────────────────────────
    // Can Al
    // ──────────────────────────────────────────

    public int GetLives()
    {
        RechargeIfNeeded();
        return PlayerPrefs.GetInt(LivesKey, MaxLives);
    }

    public bool HasLives() => GetLives() > 0;

    // ──────────────────────────────────────────
    // Can Kaybet
    // ──────────────────────────────────────────

    public void LoseLife()
    {
        int current = GetLives();
        if (current <= 0) return;

        int newCount = current - 1;
        PlayerPrefs.SetInt(LivesKey, newCount);

        // Zamanlayıcıyı başlat — sadece MaxLives'dan ilk düşüşte
        if (!PlayerPrefs.HasKey(LastLostTimeKey))
            PlayerPrefs.SetString(LastLostTimeKey, DateTime.UtcNow.ToString("o"));

        PlayerPrefs.Save();
        Debug.Log($"Can kaybedildi. Kalan: {newCount}");

        if (newCount == 0)
            ScheduleFullLivesNotification();
    }

    // ──────────────────────────────────────────
    // Can Ekle
    // ──────────────────────────────────────────

    public void AddLife(int amount = 1)
    {
        int current = GetLives();
        int newCount = Mathf.Min(current + amount, MaxLives);
        PlayerPrefs.SetInt(LivesKey, newCount);

        if (newCount >= MaxLives)
            PlayerPrefs.DeleteKey(LastLostTimeKey);

        PlayerPrefs.Save();
        Debug.Log($"Can eklendi. Toplam: {newCount}");

        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null) uiManager.UpdateLivesDisplay();

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.CancelLivesNotification();
    }

    public void SetFullLives()
    {
        PlayerPrefs.SetInt(LivesKey, MaxLives);
        PlayerPrefs.DeleteKey(LastLostTimeKey);
        PlayerPrefs.Save();

        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null) uiManager.UpdateLivesDisplay();

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.CancelLivesNotification();
    }

    // ──────────────────────────────────────────
    // Kademeli Yenileme
    // ──────────────────────────────────────────

    public void RechargeIfNeeded()
    {
        int current = PlayerPrefs.GetInt(LivesKey, MaxLives);
        if (current >= MaxLives) return;

        string lastLostStr = PlayerPrefs.GetString(LastLostTimeKey, "");
        if (string.IsNullOrEmpty(lastLostStr)) return;

        if (!DateTime.TryParse(lastLostStr, out DateTime lastLost)) return;

        double minutesPassed = (DateTime.UtcNow - lastLost).TotalMinutes;

        // Kaç can doldu hesapla
        int livesToAdd = 0;
        double usedMinutes = 0;

        for (int i = 0; i < MaxLives - current; i++)
        {
            // i=0: ilk eksik can (MaxLives - current - 1 indeksindeki)
            int idx = MaxLives - current - 1 - i;
            if (idx < 0) break;
            int needed = RechargeMinutes[idx];

            if (minutesPassed >= usedMinutes + needed)
            {
                usedMinutes += needed;
                livesToAdd++;
            }
            else break;
        }

        if (livesToAdd <= 0) return;

        int newCount = Mathf.Min(current + livesToAdd, MaxLives);
        PlayerPrefs.SetInt(LivesKey, newCount);

        if (newCount >= MaxLives)
            PlayerPrefs.DeleteKey(LastLostTimeKey);
        else
        {
            DateTime updated = lastLost.AddMinutes(usedMinutes);
            PlayerPrefs.SetString(LastLostTimeKey, updated.ToString("o"));
        }

        PlayerPrefs.Save();
        Debug.Log($"Can yenilendi. Toplam: {newCount}");
    }

    // ──────────────────────────────────────────
    // Sonraki Cana Kalan Süre
    // ──────────────────────────────────────────

    public TimeSpan GetTimeUntilNextLife()
    {
        int current = PlayerPrefs.GetInt(LivesKey, MaxLives);
        if (current >= MaxLives) return TimeSpan.Zero;

        string lastLostStr = PlayerPrefs.GetString(LastLostTimeKey, "");
        if (string.IsNullOrEmpty(lastLostStr)) return TimeSpan.Zero;

        if (!DateTime.TryParse(lastLostStr, out DateTime lastLost)) return TimeSpan.Zero;

        double minutesPassed = (DateTime.UtcNow - lastLost).TotalMinutes;

        // Bir sonraki can için indeks
        int idx = MaxLives - current - 1;
        if (idx < 0 || idx >= RechargeMinutes.Length) return TimeSpan.Zero;

        int needed = RechargeMinutes[idx];
        double remaining = needed - minutesPassed;

        return remaining > 0 ? TimeSpan.FromMinutes(remaining) : TimeSpan.Zero;
    }

    // ──────────────────────────────────────────
    // Tüm Canlar Dolana Kadar Kalan Dakika
    // ──────────────────────────────────────────

    public int GetTotalMinutesUntilFull()
    {
        int current = GetLives();
        if (current >= MaxLives) return 0;

        int total = 0;
        for (int i = 0; i < MaxLives - current; i++)
        {
            int idx = MaxLives - current - 1 - i;
            if (idx >= 0 && idx < RechargeMinutes.Length)
                total += RechargeMinutes[idx];
        }
        return total;
    }

    // ──────────────────────────────────────────
    // Bildirim
    // ──────────────────────────────────────────

    private void ScheduleFullLivesNotification()
    {
        if (NotificationManager.Instance == null) return;
        NotificationManager.Instance.ScheduleLivesNotification(GetTotalMinutesUntilFull());
    }

    // ──────────────────────────────────────────
    // Sınırsız Can — Kimse sınırsız cana sahip değil
    // ──────────────────────────────────────────

    public bool IsUnlimitedLives() => false;
}