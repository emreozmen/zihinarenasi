using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public static readonly List<QuestDefinition> AllQuests = new List<QuestDefinition>
    {
        new QuestDefinition("complete_3_levels",  "6 tur tamamla",                   6,  1),
        new QuestDefinition("earn_10_stars",      "10 yıldız kazan",                   10, 1),
        new QuestDefinition("word_score_8",       "Kelime turunda 8+ puan al",          1,  2),
        new QuestDefinition("math_perfect",       "Matematik turunda tam isabet yap",   1,  2),
        new QuestDefinition("no_pass_level",      "Pas kullanmadan bir level geç",      1,  2),
    };

    public List<QuestProgress> TodayQuests { get; private set; } = new List<QuestProgress>();
    public int CurrentStreak { get; private set; } = 0;
    public bool IsLoaded { get; private set; } = false;

    private const string QuestDateKey = "QuestDate";
    private const string QuestDataKey = "QuestData";
    private const string StreakKey = "QuestStreak";
    private const string LastCompleteKey = "QuestLastComplete";
    private const string NoAdsUntilKey = "NoAdsUntil";

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
        LoadQuests();
    }

    public void OnFirebaseReady() { }

    // ──────────────────────────────────────────
    // Görevleri Yükle
    // ──────────────────────────────────────────

    private void LoadQuests()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string savedDate = PlayerPrefs.GetString(QuestDateKey, "");

        if (savedDate == today)
            LoadFromPrefs();
        else
            GenerateNewQuests(today);

        CurrentStreak = PlayerPrefs.GetInt(StreakKey, 0);
        IsLoaded = true;

        Debug.Log($"Görevler yüklendi. Bugün: {today} | Streak: {CurrentStreak}");
    }

    // ──────────────────────────────────────────
    // Yeni Görev Oluştur
    // ──────────────────────────────────────────

    private void GenerateNewQuests(string today)
    {
        List<QuestDefinition> shuffled = new List<QuestDefinition>(AllQuests);
        Shuffle(shuffled);

        TodayQuests.Clear();
        for (int i = 0; i < Mathf.Min(5, shuffled.Count); i++)
            TodayQuests.Add(new QuestProgress(shuffled[i]));

        PlayerPrefs.SetString(QuestDateKey, today);
        SaveToPrefs();

        Debug.Log("Yeni görevler oluşturuldu.");
    }

    // ──────────────────────────────────────────
    // PlayerPrefs Kayıt/Yükle
    // ──────────────────────────────────────────

    private void SaveToPrefs()
    {
        string data = "";
        foreach (var q in TodayQuests)
            data += $"{q.Definition.Id},{q.Current},{(q.IsCompleted ? 1 : 0)},{(q.IsRewarded ? 1 : 0)}|";

        PlayerPrefs.SetString(QuestDataKey, data);
        PlayerPrefs.Save();
    }

    private void LoadFromPrefs()
    {
        TodayQuests.Clear();
        string data = PlayerPrefs.GetString(QuestDataKey, "");

        if (string.IsNullOrEmpty(data)) return;

        string[] entries = data.Split('|');
        foreach (string entry in entries)
        {
            if (string.IsNullOrEmpty(entry)) continue;

            string[] parts = entry.Split(',');
            if (parts.Length < 4) continue;

            string id = parts[0];
            int current = int.TryParse(parts[1], out int c) ? c : 0;
            bool completed = parts[2] == "1";
            bool rewarded = parts[3] == "1";

            QuestDefinition def = AllQuests.Find(q => q.Id == id);
            if (def == null) continue;

            TodayQuests.Add(new QuestProgress(def)
            {
                Current = current,
                IsCompleted = completed,
                IsRewarded = rewarded
            });
        }
    }

    // ──────────────────────────────────────────
    // İlerleme Güncelle
    // ──────────────────────────────────────────

    public void UpdateProgress(string questId, int amount = 1)
    {

        Debug.Log($"UpdateProgress: {questId} | TodayQuests sayısı: {TodayQuests.Count}");
        foreach (var q in TodayQuests)
            Debug.Log($"  -> {q.Definition.Id} | Current: {q.Current}");

        if (!IsLoaded) return;

        bool anyCompleted = false;

        foreach (var quest in TodayQuests)
        {
            if (quest.Definition.Id != questId) continue;
            if (quest.IsCompleted) continue;

            quest.Current = Mathf.Min(quest.Current + amount, quest.Definition.Target);

            if (quest.Current >= quest.Definition.Target)
            {
                quest.IsCompleted = true;
                anyCompleted = true;
                Debug.Log($"Görev tamamlandı: {quest.Definition.Title}");

                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.ShowQuestComplete(quest.Definition.Title);
            }
        }

        SaveToPrefs();

        if (anyCompleted)
            CheckAllQuestsCompleted();
    }

    // ──────────────────────────────────────────
    // Ödül Al
    // ──────────────────────────────────────────

    public void ClaimQuestReward(QuestProgress quest)
    {
        if (!quest.IsCompleted || quest.IsRewarded) return;

        quest.IsRewarded = true;

        int currentHints = PlayerPrefs.GetInt("HintCount", 0);
        PlayerPrefs.SetInt("HintCount", currentHints + quest.Definition.HintReward);
        PlayerPrefs.Save();

        SaveToPrefs();

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.ShowReward(quest.Definition.HintReward);

        Debug.Log($"Ödül alındı: +{quest.Definition.HintReward} ipucu");
    }

    // ──────────────────────────────────────────
    // Tüm Görevler Tamamlandı mı?
    // ──────────────────────────────────────────

    private void CheckAllQuestsCompleted()
    {
        foreach (var quest in TodayQuests)
            if (!quest.IsCompleted) return;

        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string lastComplete = PlayerPrefs.GetString(LastCompleteKey, "");

        if (lastComplete == today) return;

        PlayerPrefs.SetString(LastCompleteKey, today);

        string yesterday = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
        CurrentStreak = lastComplete == yesterday ? CurrentStreak + 1 : 1;

        PlayerPrefs.SetInt(StreakKey, CurrentStreak);
        PlayerPrefs.Save();

        Debug.Log($"Tüm görevler tamamlandı! Streak: {CurrentStreak}");

        if (CurrentStreak >= 7)
        {
            GiveWeeklyReward();
            CurrentStreak = 0;
            PlayerPrefs.SetInt(StreakKey, 0);
            PlayerPrefs.Save();
        }
    }

    // ──────────────────────────────────────────
    // Haftalık Ödül
    // ──────────────────────────────────────────

    private void GiveWeeklyReward()
    {
        int currentHints = PlayerPrefs.GetInt("HintCount", 0);
        PlayerPrefs.SetInt("HintCount", currentHints + 10);

        DateTime noAdsUntil = DateTime.UtcNow.AddHours(12);
        PlayerPrefs.SetString(NoAdsUntilKey, noAdsUntil.ToString("o"));
        PlayerPrefs.Save();

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.ShowWeeklyReward();

        Debug.Log("Haftalık ödül: 10 ipucu + 12 saat reklamsız!");
    }

    // ──────────────────────────────────────────
    // Reklamsız mı?
    // ──────────────────────────────────────────

    public bool IsNoAdsActive()
    {
        string noAdsUntilStr = PlayerPrefs.GetString(NoAdsUntilKey, "");
        if (string.IsNullOrEmpty(noAdsUntilStr)) return false;

        if (DateTime.TryParse(noAdsUntilStr, out DateTime noAdsUntil))
            return DateTime.UtcNow < noAdsUntil;

        return false;
    }

    // ──────────────────────────────────────────
    // Yardımcılar
    // ──────────────────────────────────────────

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }
}

[System.Serializable]
public class QuestDefinition
{
    public string Id;
    public string Title;
    public int Target;
    public int HintReward;

    public QuestDefinition(string id, string title, int target, int hintReward)
    {
        Id = id;
        Title = title;
        Target = target;
        HintReward = hintReward;
    }
}

[System.Serializable]
public class QuestProgress
{
    public QuestDefinition Definition;
    public int Current;
    public bool IsCompleted;
    public bool IsRewarded;

    public QuestProgress(QuestDefinition definition)
    {
        Definition = definition;
        Current = 0;
        IsCompleted = false;
        IsRewarded = false;
    }
}