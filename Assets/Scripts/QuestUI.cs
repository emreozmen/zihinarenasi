using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [Header("Popup")]
    public GameObject questPopup;
    public Button closeButton;

    [Header("Streak")]
    public TMP_Text streakText;
    public TMP_Text noAdsTimerText;

    [Header("Görev Kartları (5 adet)")]
    public QuestCard[] questCards;

    [Header("Haftalık Ödül")]
    public GameObject weeklyRewardPanel;
    public TMP_Text weeklyRewardText;

    private void Start()
    {
        if (questPopup != null)
            questPopup.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseQuests);
    }

    private void Update()
    {
        UpdateNoAdsTimer();
    }

    // ──────────────────────────────────────────
    // Popup Aç/Kapat
    // ──────────────────────────────────────────

    public void OpenQuests()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (questPopup != null)
            questPopup.SetActive(true);

        if (QuestManager.Instance == null || !QuestManager.Instance.IsLoaded)
        {
            if (streakText != null) streakText.text = "Yükleniyor...";
            return;
        }

        RefreshUI();
    }

    public void CloseQuests()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (questPopup != null) questPopup.SetActive(false);
    }

    // ──────────────────────────────────────────
    // UI Güncelle
    // ──────────────────────────────────────────

    public void RefreshUI()
    {
        if (QuestManager.Instance == null) return;

        // Streak
        if (streakText != null)
        {
            int streak = QuestManager.Instance.CurrentStreak;
            streakText.text = $"🔥 {streak} / 7 günlük seri";
        }

        // Görev kartları
        var quests = QuestManager.Instance.TodayQuests;
        for (int i = 0; i < questCards.Length; i++)
        {
            if (questCards[i] == null) continue;

            if (i < quests.Count)
            {
                questCards[i].gameObject.SetActive(true);
                questCards[i].Setup(quests[i], this);
            }
            else
            {
                questCards[i].gameObject.SetActive(false);
            }
        }

        // Haftalık ödül paneli
        if (weeklyRewardPanel != null)
        {
            bool noAdsActive = QuestManager.Instance.IsNoAdsActive();
            weeklyRewardPanel.SetActive(noAdsActive);

            if (noAdsActive && weeklyRewardText != null)
                weeklyRewardText.text = "⚡ Reklamsız mod aktif!";
        }
    }

    // ──────────────────────────────────────────
    // Ödül Al
    // ──────────────────────────────────────────

    public void ClaimReward(QuestProgress quest)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        QuestManager.Instance.ClaimQuestReward(quest);
        RefreshUI();
    }

    // ──────────────────────────────────────────
    // Reklamsız Sayacı
    // ──────────────────────────────────────────

    private void UpdateNoAdsTimer()
    {
        if (noAdsTimerText == null) return;
        if (QuestManager.Instance == null) return;

        if (QuestManager.Instance.IsNoAdsActive())
        {
            string noAdsUntilStr = PlayerPrefs.GetString("NoAdsUntil", "");
            if (DateTime.TryParse(noAdsUntilStr, out DateTime noAdsUntil))
            {
                TimeSpan remaining = noAdsUntil - DateTime.UtcNow;
                noAdsTimerText.gameObject.SetActive(true);
                noAdsTimerText.text = $"Reklamsız: {remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
            }
        }
        else
        {
            if (noAdsTimerText != null)
                noAdsTimerText.gameObject.SetActive(false);
        }
    }
}