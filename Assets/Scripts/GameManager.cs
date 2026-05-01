using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Managers")]
    public WordRoundManager wordRoundManager;
    public MathRoundManager mathRoundManager;
    public UIManager uiManager;

    public int CurrentLevel { get; private set; } = 1;
    public LevelData CurrentLevelData { get; private set; }

    private int totalScore;
    private int wordRoundScore;
    private int mathRoundScore;

    private int finalMathTarget;
    private int finalMathValue;
    private int finalMathDifference;

    private bool wordRoundPassed = false;
    private bool mathRoundPassed = false;
    private bool levelFinished = false;

    private string bestWordFromRound = "-";

    private void Awake()
    {
        Instance = this;
        CurrentLevel = ChapterManager.LoadCurrentLevel();
        CurrentLevelData = LevelDatabase.GetLevelData(CurrentLevel);
    }

    private void Start()
    {
        uiManager.HideAllPanels();
        uiManager.UpdateTotalScore(0);
        uiManager.UpdateLevelText(CurrentLevel);
        StartCoroutine(InitGame());
    }

    // ──────────────────────────────────────────
    // Oyun Başlangıcı
    // ──────────────────────────────────────────

    private IEnumerator InitGame()
    {
        yield return null;

        if (LivesManager.Instance != null && !LivesManager.Instance.IsUnlimitedLives())
        {
            if (!LivesManager.Instance.HasLives())
            {
                if (NoLivesPopup.Instance != null)
                    NoLivesPopup.Instance.Show(null);
                yield break;
            }
        }

        StartWordRound();
    }

    // ──────────────────────────────────────────
    // Tur Başlatma
    // ──────────────────────────────────────────

    public void StartWordRound()
    {
        levelFinished = false;
        wordRoundPassed = false;
        mathRoundPassed = false;
        wordRoundScore = 0;
        mathRoundScore = 0;
        totalScore = 0;
        finalMathTarget = 0;
        finalMathValue = 0;
        finalMathDifference = 999;
        bestWordFromRound = "-";

        uiManager.ShowWordPanel();
        uiManager.UpdateLevelText(CurrentLevel);
        wordRoundManager.BeginRound(CurrentLevelData);
    }

    public void StartMathRound()
    {
        uiManager.ShowMathPanel();
        uiManager.UpdateLevelText(CurrentLevel);
        mathRoundManager.BeginRound(CurrentLevelData);
    }

    // ──────────────────────────────────────────
    // Kelime Turu
    // ──────────────────────────────────────────

    public void OnWordRoundFinished(int score, string bestWord)
    {
        wordRoundScore = score;
        totalScore += score;
        bestWordFromRound = bestWord;
        uiManager.UpdateTotalScore(totalScore);
        StartMathRound();
    }

    public void OnWordRoundPassed()
    {
        wordRoundPassed = true;
        wordRoundScore = 0;
        bestWordFromRound = "-";
        uiManager.UpdateTotalScore(totalScore);
        StartMathRound();
    }

    // ──────────────────────────────────────────
    // Matematik Turu
    // ──────────────────────────────────────────

    public void OnMathRoundFinished(int score, int target, int finalValue, int difference)
    {
        mathRoundScore = score;
        totalScore += score;
        finalMathTarget = target;
        finalMathValue = finalValue;
        finalMathDifference = difference;

        uiManager.UpdateTotalScore(totalScore);
        FinishLevel();
    }

    public void OnMathRoundPassed()
    {
        mathRoundPassed = true;
        mathRoundScore = 0;
        finalMathTarget = 0;
        finalMathValue = 0;
        finalMathDifference = 999;

        FinishLevel();
    }

    // ──────────────────────────────────────────
    // Level Bitişi
    // ──────────────────────────────────────────

    private void FinishLevel()
    {
        if (levelFinished) return;
        levelFinished = true;

        int stars = 0;
        bool levelPassed = false;

        if (wordRoundPassed || mathRoundPassed)
        {
            stars = 0;
            levelPassed = false;
        }
        else
        {
            levelPassed = wordRoundScore > 0 && finalMathDifference <= 20 && totalScore >= 10;
            stars = levelPassed ? CalculateStars(totalScore) : 0;

            if (QuestManager.Instance != null && levelPassed)
            {
                QuestManager.Instance.UpdateProgress("complete_3_levels");
                QuestManager.Instance.UpdateProgress("earn_10_stars", stars);

                if (wordRoundScore >= 8)
                    QuestManager.Instance.UpdateProgress("word_score_8");

                if (finalMathDifference == 0)
                    QuestManager.Instance.UpdateProgress("math_perfect");

                if (!wordRoundPassed && !mathRoundPassed)
                    QuestManager.Instance.UpdateProgress("no_pass_level");
            }
        }

        // Can sistemi
        if (!levelPassed)
        {
            if (LivesManager.Instance != null && !LivesManager.Instance.IsUnlimitedLives())
            {
                LivesManager.Instance.LoseLife();
                if (uiManager != null) uiManager.UpdateLivesDisplay();
            }
        }

        ChapterManager.SaveStars(CurrentLevel, stars);

        // Firebase'e kaydet
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsReady())
            FirebaseManager.Instance.SavePlayerData();

        if (AudioManager.Instance != null)
        {
            if (levelPassed) AudioManager.Instance.PlayLevelComplete();
            else AudioManager.Instance.PlayIncorrect();
        }

        if (AdManager.Instance != null)
        {
            AdManager.Instance.OnLevelCompleted(CurrentLevel, () =>
            {
                var (nextLevel, isNewChapter, newChapterUnlocked) = ChapterManager.GetNextStep(CurrentLevel);
                uiManager.ShowResults(totalScore, wordRoundScore, mathRoundScore,
                    finalMathTarget, finalMathValue, finalMathDifference,
                    stars, levelPassed, isNewChapter, newChapterUnlocked, bestWordFromRound);
            });
        }
        else
        {
            var (nextLevel, isNewChapter, newChapterUnlocked) = ChapterManager.GetNextStep(CurrentLevel);
            uiManager.ShowResults(totalScore, wordRoundScore, mathRoundScore,
                finalMathTarget, finalMathValue, finalMathDifference,
                stars, levelPassed, isNewChapter, newChapterUnlocked, bestWordFromRound);
        }
    }

    int CalculateStars(int total)
    {
        if (total >= 18) return 3;
        if (total >= 14) return 2;
        if (total >= 10) return 1;
        return 0;
    }

    // ──────────────────────────────────────────
    // Navigasyon
    // ──────────────────────────────────────────

    public void AdvanceLevel()
    {
        int nextLevel = CurrentLevel + 1;
        int currentChapter = ChapterManager.GetChapter(CurrentLevel);
        int nextChapter = ChapterManager.GetChapter(nextLevel);

        ChapterManager.SaveCurrentLevel(nextLevel);

        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsReady())
            FirebaseManager.Instance.SavePlayerData();

        if (nextChapter != currentChapter)
            ChapterManager.LoadChapterScene(nextChapter);
        else
            ChapterManager.ReloadCurrentScene();
    }

    public void RetryLevel()
    {
        if (LivesManager.Instance != null && !LivesManager.Instance.IsUnlimitedLives())
        {
            if (!LivesManager.Instance.HasLives())
            {
                if (NoLivesPopup.Instance != null)
                    NoLivesPopup.Instance.Show(null);
                return;
            }
        }

        ChapterManager.ReloadCurrentScene();
    }

    public void GoToLevelSelect()
    {
        ChapterManager.LoadScene("LevelSelect");
    }

    public int GetTotalStars() => ChapterManager.GetTotalStars();

    public bool CanUnlockNextChapter() => ChapterManager.IsChapterUnlocked(
        ChapterManager.GetChapter(CurrentLevel) + 1
    );
}