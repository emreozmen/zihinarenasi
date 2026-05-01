using System;
using UnityEngine;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance;

    public const int MaxLives = 5;  // Yenileme ile dolan maksimum
    public const int AbsoluteMax = 99; // Satın alınan canlarla ulaşılabilecek maksimum

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

        // Zamanlayıcıyı sadece MaxLives veya altına düşünce başlat
        if (current <= MaxLives && !PlayerPrefs.HasKey(LastLostTimeKey))
            PlayerPrefs.SetString(LastLostTimeKey, DateTime.UtcNow.ToString("o"));

        PlayerPrefs.Save();
        Debug.Log($"Can kaybedildi. Kalan: {newCount}");

        if (newCount == 0)
            ScheduleFullLivesNotification();
    }

    // ──────────────────────────────────────────
    // Can Ekle (Reklam veya Normal)
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

    // ──────────────────────────────────────────
    // Can Ekle (Satın Alma) — MaxLives'ın üstüne çıkabilir
    // ──────────────────────────────────────────

    public void AddPurchasedLives(int amount = 5)
    {
        int current = GetLives();
        int newCount = Mathf.Min(current + amount, AbsoluteMax);
        PlayerPrefs.SetInt(LivesKey, newCount);

        // Satın alınan canlar MaxLives'ın üstündeyse zamanlayıcıyı sil
        if (newCount > MaxLives)
            PlayerPrefs.DeleteKey(LastLostTimeKey);

        PlayerPrefs.Save();
        Debug.Log($"Satın alınan can eklendi. Toplam: {newCount}");

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
    // Kademeli Yenileme — Sadece MaxLives altında çalışır
    // ──────────────────────────────────────────

    public void RechargeIfNeeded()
    {
        int current = PlayerPrefs.GetInt(LivesKey, MaxLives);

        // MaxLives veya üstündeyse yenileme yapma
        if (current >= MaxLives) return;

        string lastLostStr = PlayerPrefs.GetString(LastLostTimeKey, "");
        if (string.IsNullOrEmpty(lastLostStr)) return;

        if (!DateTime.TryParse(lastLostStr, out DateTime lastLost)) return;

        double minutesPassed = (DateTime.UtcNow - lastLost).TotalMinutes;

        int livesToAdd = 0;
        double usedMinutes = 0;

        for (int i = current; i < MaxLives; i++)
        {
            int needed = RechargeMinutes[i];
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

        // MaxLives veya üstündeyse yenileme yok
        if (current >= MaxLives) return TimeSpan.Zero;

        string lastLostStr = PlayerPrefs.GetString(LastLostTimeKey, "");
        if (string.IsNullOrEmpty(lastLostStr)) return TimeSpan.Zero;

        if (!DateTime.TryParse(lastLostStr, out DateTime lastLost)) return TimeSpan.Zero;

        double minutesPassed = (DateTime.UtcNow - lastLost).TotalMinutes;

        if (current < 0 || current >= RechargeMinutes.Length) return TimeSpan.Zero;

        int needed = RechargeMinutes[current];
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
        for (int i = current; i < MaxLives; i++)
            total += RechargeMinutes[i];

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
    // Sınırsız Can
    // ──────────────────────────────────────────

    public bool IsUnlimitedLives() => false;
}