using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text levelNumberText;
    public GameObject lockIcon;
    public GameObject starsContainer;
    public Image[] starImages;
    public Button button;
    public Image circleImage;

    [Header("Level Görselleri (1-10 sırayla)")]
    public Sprite[] levelSprites;

    [Header("Boyut")]
    public Vector2 buttonSize = new Vector2(125f, 125f);

    [Header("Colors")]
    public Color unlockedColor = new Color(1f, 1f, 1f, 1f);
    public Color completedColor = new Color(0.85f, 1f, 0.85f, 1f);
    public Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    public Color starFilledColor = new Color(1.00f, 0.85f, 0.20f);
    public Color starEmptyColor = new Color(0.70f, 0.70f, 0.70f);

    private int globalLevel;

    // ──────────────────────────────────────────
    // Kurulum
    // ──────────────────────────────────────────

    public void Setup(int globalLevelNumber)
    {
        globalLevel = globalLevelNumber;

        bool isUnlocked = ChapterManager.IsLevelUnlocked(globalLevel);
        int stars = ChapterManager.GetStars(globalLevel);
        bool isCompleted = stars > 0;

        int levelInChapter = ChapterManager.GetLevelInChapter(globalLevel);

        // Level numarası metni
        if (levelNumberText != null)
            levelNumberText.text = levelInChapter.ToString();

        // Boyutu zorla
        RectTransform rt = circleImage != null
            ? circleImage.GetComponent<RectTransform>()
            : GetComponent<RectTransform>();

        if (rt != null)
            rt.sizeDelta = buttonSize;

        // Görsel ata
        if (circleImage != null)
        {
            Sprite selectedSprite = GetSpriteForLevel(levelInChapter);

            if (selectedSprite != null)
            {
                circleImage.sprite = selectedSprite;
                circleImage.color = isUnlocked ? Color.white : lockedColor;
            }
            else
            {
                ApplyColor(isUnlocked, isCompleted);
            }
        }

        // Kilit ikonu
        if (lockIcon != null)
            lockIcon.SetActive(!isUnlocked);

        // Yıldızlar
        if (starsContainer != null)
            starsContainer.SetActive(isUnlocked);

        if (isUnlocked)
            UpdateStars(stars);

        // Buton
        button.interactable = isUnlocked;
        button.onClick.RemoveAllListeners();

        if (isUnlocked)
            button.onClick.AddListener(OnClicked);
    }

    // ──────────────────────────────────────────
    // Sprite Seç
    // ──────────────────────────────────────────

    private Sprite GetSpriteForLevel(int levelInChapter)
    {
        if (levelSprites == null || levelSprites.Length == 0) return null;

        // Boss level (10) için son sprite — eğer sadece 1 sprite varsa onu kullan
        if (levelSprites.Length == 1)
            return levelSprites[0];

        // Normal levellar için index bazlı seç
        int index = Mathf.Clamp(levelInChapter - 1, 0, levelSprites.Length - 1);
        return levelSprites[index];
    }

    // ──────────────────────────────────────────
    // Renk Uygula (görsel yoksa)
    // ──────────────────────────────────────────

    private void ApplyColor(bool isUnlocked, bool isCompleted)
    {
        if (circleImage == null) return;

        if (!isUnlocked)
            circleImage.color = lockedColor;
        else if (isCompleted)
            circleImage.color = completedColor;
        else
            circleImage.color = unlockedColor;
    }

    // ──────────────────────────────────────────
    // Tıklama
    // ──────────────────────────────────────────

    private void OnClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        ChapterManager.SaveCurrentLevel(globalLevel);
        int chapter = ChapterManager.GetChapter(globalLevel);
        ChapterManager.LoadChapterScene(chapter);
    }

    // ──────────────────────────────────────────
    // Yıldız Güncelleme
    // ──────────────────────────────────────────

    private void UpdateStars(int earnedStars)
    {
        if (starImages == null) return;

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;
            starImages[i].color = (i < earnedStars) ? starFilledColor : starEmptyColor;
        }
    }
}