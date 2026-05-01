using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Auth;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    private bool isInitialized = false;

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
        InitializeFirebase();
    }

    // ──────────────────────────────────────────
    // Firebase Başlat
    // ──────────────────────────────────────────

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                SignInAnonymously();
            }
            else
            {
                Debug.LogError("Firebase başlatılamadı: " + task.Result);
            }
        });
    }

    // ──────────────────────────────────────────
    // Anonim Giriş
    // ──────────────────────────────────────────

    private void SignInAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Firebase anonim giriş başarısız: " + task.Exception);
                return;
            }

            currentUser = task.Result.User;
            isInitialized = true;
            Debug.Log("Firebase giriş başarılı. UID: " + currentUser.UserId);

            LoadPlayerData();
        });
    }

    // ──────────────────────────────────────────
    // Veri Kaydet
    // ──────────────────────────────────────────

    public void SavePlayerData()
    {
        if (!isInitialized || currentUser == null) return;

        int totalStars = ChapterManager.GetTotalStars();
        int totalScore = PlayerPrefs.GetInt("TotalScore", 0);
        int hintCount = PlayerPrefs.GetInt("HintCount", 0);
        int lives = PlayerPrefs.GetInt("LivesCount", LivesManager.MaxLives);
        int currentLevel = ChapterManager.LoadCurrentLevel();

        Dictionary<string, int> levelStars = new Dictionary<string, int>();
        for (int i = 1; i <= ChapterManager.TotalChapters * ChapterManager.LevelsPerChapter; i++)
        {
            int stars = ChapterManager.GetStars(i);
            if (stars > 0)
                levelStars["level_" + i] = stars;
        }

        var data = new Dictionary<string, object>
        {
            { "totalStars",   totalStars },
            { "totalScore",   totalScore },
            { "hintCount",    hintCount },
            { "lives",        lives },
            { "currentLevel", currentLevel },
            { "levelStars",   levelStars },
            { "lastSaved",    DateTime.UtcNow.ToString("o") },
            { "platform",     Application.platform.ToString() },
            { "appVersion",   Application.version }
        };

        db.Collection("players").Document(currentUser.UserId)
            .SetAsync(data)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError("Veri kaydedilemedi: " + task.Exception);
                else
                    Debug.Log("Veriler kaydedildi.");
            });
    }

    // ──────────────────────────────────────────
    // Satın Alma Logu
    // ──────────────────────────────────────────

    public void LogPurchase(string productId)
    {
        if (!isInitialized || currentUser == null) return;

        var data = new Dictionary<string, object>
        {
            { "productId", productId },
            { "timestamp", DateTime.UtcNow.ToString("o") },
            { "platform",  Application.platform.ToString() },
            { "appVersion", Application.version }
        };

        db.Collection("players").Document(currentUser.UserId)
            .Collection("purchases").AddAsync(data)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError("Satın alma logu kaydedilemedi: " + task.Exception);
                else
                    Debug.Log("Satın alma logu kaydedildi: " + productId);
            });
    }

    // ──────────────────────────────────────────
    // Veri Yükle
    // ──────────────────────────────────────────

    public void LoadPlayerData()
    {
        if (!isInitialized || currentUser == null) return;

        db.Collection("players").Document(currentUser.UserId)
            .GetSnapshotAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Veri yüklenemedi: " + task.Exception);
                    return;
                }

                DocumentSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    Debug.Log("Yeni oyuncu — bulutta veri yok.");
                    return;
                }

                int localLevel = ChapterManager.LoadCurrentLevel();
                int cloudLevel = snapshot.ContainsField("currentLevel")
                    ? snapshot.GetValue<int>("currentLevel") : 1;

                if (cloudLevel > localLevel)
                {
                    Debug.Log("Bulut verisi daha güncel, yükleniyor...");
                    RestoreFromCloud(snapshot);
                }
                else
                {
                    Debug.Log("Yerel veri daha güncel, bulut güncelleniyor...");
                    SavePlayerData();
                }
            });
    }

    // ──────────────────────────────────────────
    // Buluttan Geri Yükle
    // ──────────────────────────────────────────

    private void RestoreFromCloud(DocumentSnapshot snapshot)
    {
        if (snapshot.ContainsField("totalScore"))
            PlayerPrefs.SetInt("TotalScore", snapshot.GetValue<int>("totalScore"));

        if (snapshot.ContainsField("hintCount"))
            PlayerPrefs.SetInt("HintCount", snapshot.GetValue<int>("hintCount"));

        if (snapshot.ContainsField("lives"))
            PlayerPrefs.SetInt("LivesCount", snapshot.GetValue<int>("lives"));

        if (snapshot.ContainsField("currentLevel"))
            ChapterManager.SaveCurrentLevel(snapshot.GetValue<int>("currentLevel"));

        if (snapshot.ContainsField("levelStars"))
        {
            var levelStars = snapshot.GetValue<Dictionary<string, object>>("levelStars");
            foreach (var kvp in levelStars)
            {
                int level = int.Parse(kvp.Key.Replace("level_", ""));
                int stars = Convert.ToInt32(kvp.Value);
                ChapterManager.SaveStars(level, stars);
            }
        }

        PlayerPrefs.Save();
        Debug.Log("Bulut verisi geri yüklendi.");
    }

    // ──────────────────────────────────────────
    // Yardımcılar
    // ──────────────────────────────────────────

    public string GetPlayerId() => currentUser != null ? currentUser.UserId : "unknown";
    public bool IsReady() => isInitialized;
}