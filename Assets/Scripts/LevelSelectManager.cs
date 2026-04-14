using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    [Header("Level Button Prefab")]
    public GameObject levelButtonPrefab;

    [Header("Chapter Pages")]
    public Transform[] chapterGrids;

    [Header("Grid_9 (Level 1-9)")]
    public Transform[] chapterGrid9s;

    [Header("Grid_10 (Level 10 - Boss)")]
    public Transform[] chapterGrid10s;

    [Header("Chapter UI")]
    public TMP_Text chapterTitleText;
    public TMP_Text chapterStarsText;
    public TMP_Text chapterStatusText;

    [Header("Navigation")]
    public Button prevButton;
    public Button nextButton;

    [Header("Main Menu")]
    public Button mainMenuButton;

    [Header("Bölüm Arka Planları")]
    public Image[] chapterBackgrounds;     // Her bölümün arka plan Image'ı
    public Sprite[] chapterBgSprites;      // Her bölüm için arka plan sprite'ı

    [Header("Bölüm 1 — Ateş/Yıldırım")]
    public Sprite[] chapter1LevelSprites;      // 1-9 arası buton görselleri
    public Sprite chapter1BossSprite;          // 10. level boss butonu

    [Header("Bölüm 2 — Buz/Kış")]
    public Sprite[] chapter2LevelSprites;
    public Sprite chapter2BossSprite;

    [Header("Bölüm 3 — Dağ/Taş")]
    public Sprite[] chapter3LevelSprites;
    public Sprite chapter3BossSprite;

    [Header("Bölüm 4 — Gece Ormanı")]
    public Sprite[] chapter4LevelSprites;
    public Sprite chapter4BossSprite;

    [Header("Bölüm 5 — Karanlık Orman")]
    public Sprite[] chapter5LevelSprites;
    public Sprite chapter5BossSprite;

    private int currentChapterIndex = 0;
    private int totalChapters;

    // ──────────────────────────────────────────
    // Başlangıç
    // ──────────────────────────────────────────

    private void Start()
    {
        totalChapters = chapterGrids.Length;

        for (int c = 0; c < totalChapters; c++)
            BuildChapterGrid(c + 1, c);

        int currentLevel = ChapterManager.LoadCurrentLevel();
        currentChapterIndex = Mathf.Clamp(ChapterManager.GetChapter(currentLevel) - 1, 0, totalChapters - 1);

        ShowChapter(currentChapterIndex);

        if (prevButton != null) prevButton.onClick.AddListener(GoToPreviousChapter);
        if (nextButton != null) nextButton.onClick.AddListener(GoToNextChapter);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    // ──────────────────────────────────────────
    // Grid Oluşturma
    // ──────────────────────────────────────────

    private void BuildChapterGrid(int chapter, int chapterIndex)
    {
        Transform grid9 = chapterGrid9s != null && chapterIndex < chapterGrid9s.Length ? chapterGrid9s[chapterIndex] : null;
        Transform grid10 = chapterGrid10s != null && chapterIndex < chapterGrid10s.Length ? chapterGrid10s[chapterIndex] : null;

        if (grid9 != null)
            foreach (Transform child in grid9) Destroy(child.gameObject);
        if (grid10 != null)
            foreach (Transform child in grid10) Destroy(child.gameObject);

        // Bu bölümün sprite dizisini al
        Sprite[] levelSprites = GetLevelSprites(chapter);
        Sprite bossSprite = GetBossSprite(chapter);

        int startLevel = ChapterManager.GetChapterStartLevel(chapter);
        int endLevel = ChapterManager.GetChapterEndLevel(chapter);
        int buttonIndex = 0;

        for (int level = startLevel; level <= endLevel; level++)
        {
            bool isLastLevel = (buttonIndex == 9);
            Transform targetGrid = isLastLevel ? grid10 : grid9;

            if (targetGrid == null) { buttonIndex++; continue; }

            GameObject buttonObj = Instantiate(levelButtonPrefab, targetGrid);
            LevelButton lb = buttonObj.GetComponent<LevelButton>();

            if (lb != null)
            {
                // Bölüme uygun sprite'ları ata
                if (isLastLevel && bossSprite != null)
                {
                    lb.levelSprites = new Sprite[] { bossSprite };
                }
                else if (levelSprites != null)
                {
                    lb.levelSprites = levelSprites;
                }

                lb.Setup(level);
            }

            buttonIndex++;
        }
    }

    // ──────────────────────────────────────────
    // Bölüme Göre Sprite Getir
    // ──────────────────────────────────────────

    private Sprite[] GetLevelSprites(int chapter)
    {
        switch (chapter)
        {
            case 1: return chapter1LevelSprites;
            case 2: return chapter2LevelSprites;
            case 3: return chapter3LevelSprites;
            case 4: return chapter4LevelSprites;
            case 5: return chapter5LevelSprites;
            default: return chapter1LevelSprites;
        }
    }

    private Sprite GetBossSprite(int chapter)
    {
        switch (chapter)
        {
            case 1: return chapter1BossSprite;
            case 2: return chapter2BossSprite;
            case 3: return chapter3BossSprite;
            case 4: return chapter4BossSprite;
            case 5: return chapter5BossSprite;
            default: return chapter1BossSprite;
        }
    }

    // ──────────────────────────────────────────
    // Sayfa Gösterme
    // ──────────────────────────────────────────

    private void ShowChapter(int index)
    {
        for (int i = 0; i < chapterGrids.Length; i++)
            chapterGrids[i].gameObject.SetActive(i == index);

        int chapter = index + 1;
        bool isUnlocked = ChapterManager.IsChapterUnlocked(chapter);

        if (chapterTitleText != null)
            chapterTitleText.text = "Bölüm " + chapter;

        if (chapterStarsText != null)
        {
            int stars = ChapterManager.GetChapterStars(chapter);
            int maxStars = ChapterManager.LevelsPerChapter * ChapterManager.MaxStarsPerLevel;
            chapterStarsText.text = stars + " / " + maxStars + " yıldız";
        }

        if (chapterStatusText != null)
            chapterStatusText.text = isUnlocked ? "" : "Kilitli";

        if (prevButton != null)
            prevButton.interactable = index > 0;

        if (nextButton != null)
            nextButton.interactable = index < totalChapters - 1;

        // Arka plan sprite'ını değiştir
        UpdateBackground(index);
    }

    // ──────────────────────────────────────────
    // Arka Plan Güncelle
    // ──────────────────────────────────────────

    private void UpdateBackground(int chapterIndex)
    {
        if (chapterBackgrounds == null || chapterBgSprites == null) return;
        if (chapterIndex >= chapterBackgrounds.Length) return;
        if (chapterIndex >= chapterBgSprites.Length) return;

        // Tüm arka planları kapat
        foreach (var bg in chapterBackgrounds)
            if (bg != null) bg.gameObject.SetActive(false);

        // Seçili bölümün arka planını aç
        if (chapterBackgrounds[chapterIndex] != null)
        {
            chapterBackgrounds[chapterIndex].gameObject.SetActive(true);
            if (chapterBgSprites[chapterIndex] != null)
                chapterBackgrounds[chapterIndex].sprite = chapterBgSprites[chapterIndex];
        }
    }

    // ──────────────────────────────────────────
    // Navigasyon
    // ──────────────────────────────────────────

    public void GoToPreviousChapter()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (currentChapterIndex > 0)
        {
            currentChapterIndex--;
            ShowChapter(currentChapterIndex);
        }
    }

    public void GoToNextChapter()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (currentChapterIndex < totalChapters - 1)
        {
            currentChapterIndex++;
            ShowChapter(currentChapterIndex);
        }
    }

    public void GoToMainMenu()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}