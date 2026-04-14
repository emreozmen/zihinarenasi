using UnityEngine;
using UnityEngine.SceneManagement;

public static class ChapterManager
{
    public const int LevelsPerChapter = 10;
    public const int StarsRequiredToUnlock = 21;
    public const int MaxStarsPerLevel = 3;
    public const int TotalChapters = 5;

    // ──────────────────────────────────────────
    // Temel Hesaplamalar
    // ──────────────────────────────────────────

    public static int GetChapter(int level)
    {
        return Mathf.CeilToInt((float)level / LevelsPerChapter);
    }

    public static int GetLevelInChapter(int level)
    {
        int mod = level % LevelsPerChapter;
        return mod == 0 ? LevelsPerChapter : mod;
    }

    public static int GetGlobalLevel(int chapter, int levelInChapter)
    {
        return (chapter - 1) * LevelsPerChapter + levelInChapter;
    }

    public static int GetChapterStartLevel(int chapter)
    {
        return (chapter - 1) * LevelsPerChapter + 1;
    }

    public static int GetChapterEndLevel(int chapter)
    {
        return chapter * LevelsPerChapter;
    }

    // ──────────────────────────────────────────
    // Yıldız Kaydetme / Okuma
    // ──────────────────────────────────────────

    public static void SaveStars(int globalLevel, int stars)
    {
        string key = "LevelStars_" + globalLevel;
        int existing = PlayerPrefs.GetInt(key, 0);

        if (stars > existing)
        {
            PlayerPrefs.SetInt(key, stars);
            PlayerPrefs.Save();
        }
    }

    public static int GetStars(int globalLevel)
    {
        return PlayerPrefs.GetInt("LevelStars_" + globalLevel, 0);
    }

    public static int GetChapterStars(int chapter)
    {
        int total = 0;
        int start = GetChapterStartLevel(chapter);
        int end = GetChapterEndLevel(chapter);

        for (int i = start; i <= end; i++)
            total += GetStars(i);

        return total;
    }

    public static int GetTotalStars()
    {
        int total = 0;
        for (int c = 1; c <= TotalChapters; c++)
            total += GetChapterStars(c);
        return total;
    }

    // ──────────────────────────────────────────
    // Level / Bölüm Kilit Kontrolü
    // ──────────────────────────────────────────

    public static bool IsLevelUnlocked(int globalLevel)
    {
        if (globalLevel <= 1) return true;

        int chapter = GetChapter(globalLevel);
        int levelInChapter = GetLevelInChapter(globalLevel);

        if (levelInChapter > 1)
        {
            int previousLevel = globalLevel - 1;
            return GetStars(previousLevel) > 0;
        }

        return IsChapterUnlocked(chapter);
    }

    public static bool IsChapterUnlocked(int chapter)
    {
        if (chapter <= 1) return true;

        int previousChapter = chapter - 1;
        int lastLevelOfPreviousChapter = GetChapterEndLevel(previousChapter);

        if (GetStars(lastLevelOfPreviousChapter) == 0) return false;

        return GetChapterStars(previousChapter) >= StarsRequiredToUnlock;
    }

    // ──────────────────────────────────────────
    // Level İlerleme
    // ──────────────────────────────────────────

    public static (int nextLevel, bool isNewChapter, bool newChapterUnlocked) GetNextStep(int currentGlobalLevel)
    {
        int nextLevel = currentGlobalLevel + 1;
        int currentChapter = GetChapter(currentGlobalLevel);
        int nextChapter = GetChapter(nextLevel);

        bool isNewChapter = nextChapter > currentChapter;
        bool newChapterUnlocked = isNewChapter && IsChapterUnlocked(nextChapter);

        return (nextLevel, isNewChapter, newChapterUnlocked);
    }

    // ──────────────────────────────────────────
    // Kayıt / Yükleme
    // ──────────────────────────────────────────

    private const string CurrentLevelKey = "CurrentLevel";

    public static void SaveCurrentLevel(int globalLevel)
    {
        PlayerPrefs.SetInt(CurrentLevelKey, globalLevel);
        PlayerPrefs.Save();
    }

    public static int LoadCurrentLevel()
    {
        return PlayerPrefs.GetInt(CurrentLevelKey, 1);
    }

    // ──────────────────────────────────────────
    // Sahne Yönetimi — SceneLoader ile smooth geçiş
    // ──────────────────────────────────────────

    public static void LoadChapterScene(int chapter)
    {
        string sceneName = "Chapter_" + chapter;

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    public static void ReloadCurrentScene()
    {
        int buildIndex = SceneManager.GetActiveScene().buildIndex;

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene(buildIndex);
        else
            SceneManager.LoadScene(buildIndex);
    }

    public static void LoadScene(string sceneName)
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    public static int GetTotalScore()
    {
        return PlayerPrefs.GetInt("TotalScore", 0);
    }
}