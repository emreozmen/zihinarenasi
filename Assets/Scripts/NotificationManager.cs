using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    // ──────────────────────────────────────────
    // Oyun İçi Bildirim UI
    // ──────────────────────────────────────────

    [Header("Oyun İçi Bildirim")]
    public GameObject notificationPanel;
    public TMP_Text notificationText;
    public Image notificationIcon;

    [Header("İkonlar")]
    public Sprite questIcon;
    public Sprite rewardIcon;

    [Header("Animasyon Ayarları")]
    public float slideInDuration = 0.3f;
    public float holdDuration = 2.5f;
    public float slideOutDuration = 0.3f;

    private RectTransform panelRect;
    private Coroutine currentNotif;
    private Vector2 shownPos;
    private Vector2 hiddenPos;

    // ──────────────────────────────────────────
    // Bildirim ID'leri
    // ──────────────────────────────────────────

    private const string LivesChannelId = "lives_channel";
    private const string DailyChannelId = "daily_channel";
    private const int LivesNotifId = 1001;
    private const int DailyNotifId = 1002;

    // ──────────────────────────────────────────
    // Başlangıç
    // ──────────────────────────────────────────

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (notificationPanel != null)
        {
            panelRect = notificationPanel.GetComponent<RectTransform>();
            shownPos = panelRect.anchoredPosition;
            hiddenPos = shownPos + new Vector2(panelRect.sizeDelta.x + 50f, 0f);
            panelRect.anchoredPosition = hiddenPos;
            notificationPanel.SetActive(false);
        }

        SetupNotificationChannels();
        ScheduleDailyNotification();
    }

    // ──────────────────────────────────────────
    // Kanal Kurulum
    // ──────────────────────────────────────────

    private void SetupNotificationChannels()
    {
#if UNITY_ANDROID
        var livesChannel = new AndroidNotificationChannel
        {
            Id          = LivesChannelId,
            Name        = "Can Bildirimleri",
            Description = "Can yenilendiğinde bildirim gönderir.",
            Importance  = Importance.Default
        };
        AndroidNotificationCenter.RegisterNotificationChannel(livesChannel);

        var dailyChannel = new AndroidNotificationChannel
        {
            Id          = DailyChannelId,
            Name        = "Günlük Bildirimler",
            Description = "Günlük oynamaya davet bildirimi.",
            Importance  = Importance.Default
        };
        AndroidNotificationCenter.RegisterNotificationChannel(dailyChannel);
#endif
    }

    // ──────────────────────────────────────────
    // Can Bildirimi
    // ──────────────────────────────────────────

    public void ScheduleLivesNotification(int minutesUntilFull)
    {
#if UNITY_ANDROID
        // Eski bildirimi iptal et
        AndroidNotificationCenter.CancelNotification(LivesNotifId);

        var notification = new AndroidNotification
        {
            Title     = "Canların Yenilendi!",
            Text      = "Zihin Arenası seni bekliyor. Oynamaya hazır mısın?",
            FireTime  = DateTime.Now.AddMinutes(minutesUntilFull),
            SmallIcon = "icon_small",
            LargeIcon = "icon_large"
        };

        AndroidNotificationCenter.SendNotificationWithExplicitID(notification, LivesChannelId, LivesNotifId);
        Debug.Log($"Can bildirimi planlandı: {minutesUntilFull} dakika sonra");

#elif UNITY_IOS
        iOSNotificationCenter.RemoveScheduledNotification("lives_notification");

        var request = new iOSNotificationTimeIntervalTrigger
        {
            TimeInterval = TimeSpan.FromMinutes(minutesUntilFull),
            Repeats      = false
        };

        var notification = new iOSNotification
        {
            Identifier       = "lives_notification",
            Title            = "Canların Yenilendi!",
            Body             = "Zihin Arenası seni bekliyor. Oynamaya hazır mısın?",
            Trigger          = request,
            ShowInForeground = false
        };

        iOSNotificationCenter.ScheduleNotification(notification);
#endif
    }

    // ──────────────────────────────────────────
    // Günlük Bildirim
    // ──────────────────────────────────────────

    public void ScheduleDailyNotification()
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelNotification(DailyNotifId);

        DateTime now    = DateTime.Now;
        DateTime target = new DateTime(now.Year, now.Month, now.Day, 19, 0, 0);

        if (now > target)
            target = target.AddDays(1);

        string[] messages = {
            "Bugün Zihin Arenası'nda yerini aldın mı?",
            "Günlük görevlerin seni bekliyor!",
            "Kelime ve matematik meydan okuması hazır!",
            "Yıldızlarını toplamaya devam et!",
            "Beynini çalıştırma vakti!"
        };

        string msg = messages[UnityEngine.Random.Range(0, messages.Length)];

        var notification = new AndroidNotification
        {
            Title          = "Zihin Arenası",
            Text           = msg,
            FireTime       = target,
            SmallIcon      = "icon_small",
            LargeIcon      = "icon_large",
            RepeatInterval = TimeSpan.FromDays(1)
        };

        AndroidNotificationCenter.SendNotificationWithExplicitID(notification, DailyChannelId, DailyNotifId);
        Debug.Log($"Günlük bildirim planlandı: {target}");

#elif UNITY_IOS
        iOSNotificationCenter.RemoveScheduledNotification("daily_notification");

        var trigger = new iOSNotificationCalendarTrigger
        {
            Hour    = 19,
            Minute  = 0,
            Second  = 0,
            Repeats = true
        };

        string[] messages = {
            "Bugün Zihin Arenası'nda yerini aldın mı?",
            "Günlük görevlerin seni bekliyor!",
            "Kelime ve matematik meydan okuması hazır!"
        };

        string msg = messages[UnityEngine.Random.Range(0, messages.Length)];

        var notification = new iOSNotification
        {
            Identifier       = "daily_notification",
            Title            = "Zihin Arenası",
            Body             = msg,
            Trigger          = trigger,
            ShowInForeground = false
        };

        iOSNotificationCenter.ScheduleNotification(notification);
#endif
    }

    // ──────────────────────────────────────────
    // Bildirimi İptal Et
    // ──────────────────────────────────────────

    public void CancelLivesNotification()
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelNotification(LivesNotifId);
#elif UNITY_IOS
        iOSNotificationCenter.RemoveScheduledNotification("lives_notification");
#endif
    }

    // ──────────────────────────────────────────
    // Oyun İçi Bildirim
    // ──────────────────────────────────────────

    public void ShowQuestComplete(string title)
    {
        Show($"Görev Tamamlandı!\n{title}", questIcon);
    }

    public void ShowReward(int hintCount)
    {
        Show($"+{hintCount} İpucu Kazandın!", rewardIcon);
    }

    public void ShowWeeklyReward()
    {
        Show("Haftalık Ödül!\n+10 İpucu & 12 Saat Reklamsız!", rewardIcon);
    }

    public void Show(string message, Sprite icon = null)
    {
        if (notificationPanel == null) return;

        if (currentNotif != null)
            StopCoroutine(currentNotif);

        currentNotif = StartCoroutine(ShowRoutine(message, icon));
    }

    // ──────────────────────────────────────────
    // Animasyon
    // ──────────────────────────────────────────

    private IEnumerator ShowRoutine(string message, Sprite icon)
    {
        if (notificationText != null) notificationText.text = message;
        if (notificationIcon != null)
        {
            notificationIcon.gameObject.SetActive(icon != null);
            if (icon != null) notificationIcon.sprite = icon;
        }

        notificationPanel.SetActive(true);
        panelRect.anchoredPosition = hiddenPos;

        float elapsed = 0f;
        while (elapsed < slideInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideInDuration);
            float e = 1f - Mathf.Pow(1f - t, 3f);
            panelRect.anchoredPosition = Vector2.Lerp(hiddenPos, shownPos, e);
            yield return null;
        }
        panelRect.anchoredPosition = shownPos;

        yield return new WaitForSecondsRealtime(holdDuration);

        elapsed = 0f;
        while (elapsed < slideOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideOutDuration);
            float e = t * t;
            panelRect.anchoredPosition = Vector2.Lerp(shownPos, hiddenPos, e);
            yield return null;
        }

        panelRect.anchoredPosition = hiddenPos;
        notificationPanel.SetActive(false);
        currentNotif = null;
    }
}