using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int levelNumber;
    public int chapterNumber;
    public int levelInChapter;

    // Kelime Turu
    public int wordMinLength;
    public int wordMaxLength;
    public float wordRoundDuration;
    public int letterCount;
    public bool hasRequiredLetter;
    public int requiredLetterIndex;
    public int minimumWordLength;

    // Matematik Turu
    public float mathRoundDuration;
    public int mathTargetMin;
    public int mathTargetMax;
    public int bigNumberCount;
    public int bannedOperation;
    public int maxOperationSteps;

    // Hız Bonusu
    public bool hasSpeedBonus;
    public float speedBonusThreshold;

    // Boss Level
    public bool isBossLevel;

    // Seed kelimeyi göster (Bölüm 1)
    public bool showSeedWordHint;
}

public static class LevelDatabase
{
    public static LevelData GetLevelData(int globalLevel)
    {
        globalLevel = Mathf.Max(1, globalLevel);

        int chapter = ChapterManager.GetChapter(globalLevel);
        int levelInChapter = ChapterManager.GetLevelInChapter(globalLevel);
        bool isBossLevel = levelInChapter == 10;

        float chapterProgress = (levelInChapter - 1) / (float)(ChapterManager.LevelsPerChapter - 1);

        LevelData data = new LevelData
        {
            levelNumber = globalLevel,
            chapterNumber = chapter,
            levelInChapter = levelInChapter,
            isBossLevel = isBossLevel,
            bannedOperation = 0,
            hasRequiredLetter = false,
            requiredLetterIndex = 0,
            minimumWordLength = 0,
            maxOperationSteps = 0,
            hasSpeedBonus = false,
            speedBonusThreshold = 0.5f,
            showSeedWordHint = false
        };

        if (isBossLevel)
        {
            SetBossLevelData(data, chapter);
            return data;
        }

        switch (chapter)
        {
            // ─────────────────────────────────────────
            // BÖLÜM 1 — Normal + Seed kelime ipucu
            // ─────────────────────────────────────────
            case 1:
                data.letterCount = 8;
                data.wordMinLength = 3;
                data.wordMaxLength = Mathf.RoundToInt(Mathf.Lerp(4f, 6f, chapterProgress));
                data.wordRoundDuration = Mathf.Lerp(50f, 40f, chapterProgress);
                data.mathRoundDuration = Mathf.Lerp(60f, 50f, chapterProgress);
                data.mathTargetMin = 100;
                data.mathTargetMax = Mathf.RoundToInt(Mathf.Lerp(200f, 350f, chapterProgress));
                data.bigNumberCount = 2;
                data.showSeedWordHint = true;   // Seed kelime ipucu aktif!
                break;

            // ─────────────────────────────────────────
            // BÖLÜM 2 — Zorunlu Harf
            // ─────────────────────────────────────────
            case 2:
                data.letterCount = 8;
                data.wordMinLength = 4;
                data.wordMaxLength = Mathf.RoundToInt(Mathf.Lerp(6f, 7f, chapterProgress));
                data.wordRoundDuration = Mathf.Lerp(32f, 27f, chapterProgress);
                data.mathRoundDuration = Mathf.Lerp(42f, 38f, chapterProgress);
                data.mathTargetMin = 200;
                data.mathTargetMax = Mathf.RoundToInt(Mathf.Lerp(550f, 700f, chapterProgress));
                data.bigNumberCount = 2;
                data.hasRequiredLetter = true;
                break;

            // ─────────────────────────────────────────
            // BÖLÜM 3 — Minimum Kelime Uzunluğu
            // ─────────────────────────────────────────
            case 3:
                data.letterCount = 8;
                data.wordMinLength = 5;
                data.wordMaxLength = Mathf.RoundToInt(Mathf.Lerp(7f, 8f, chapterProgress));
                data.wordRoundDuration = Mathf.Lerp(27f, 23f, chapterProgress);
                data.mathRoundDuration = Mathf.Lerp(38f, 34f, chapterProgress);
                data.mathTargetMin = 300;
                data.mathTargetMax = Mathf.RoundToInt(Mathf.Lerp(700f, 850f, chapterProgress));
                data.bigNumberCount = Mathf.RoundToInt(Mathf.Lerp(2f, 3f, chapterProgress));
                data.minimumWordLength = Mathf.RoundToInt(Mathf.Lerp(4f, 5f, chapterProgress));
                break;

            // ─────────────────────────────────────────
            // BÖLÜM 4 — Adım Sınırı
            // ─────────────────────────────────────────
            case 4:
                data.letterCount = 8;
                data.wordMinLength = 5;
                data.wordMaxLength = Mathf.RoundToInt(Mathf.Lerp(8f, 9f, chapterProgress));
                data.wordRoundDuration = Mathf.Lerp(23f, 20f, chapterProgress);
                data.mathRoundDuration = Mathf.Lerp(34f, 30f, chapterProgress);
                data.mathTargetMin = 400;
                data.mathTargetMax = Mathf.RoundToInt(Mathf.Lerp(850f, 950f, chapterProgress));
                data.bigNumberCount = 3;
                data.maxOperationSteps = Mathf.RoundToInt(Mathf.Lerp(4f, 3f, chapterProgress));
                break;

            // ─────────────────────────────────────────
            // BÖLÜM 5+ — Hız Bonusu
            // ─────────────────────────────────────────
            default:
                data.letterCount = 8;
                data.wordMinLength = 6;
                data.wordMaxLength = 9;
                data.wordRoundDuration = Mathf.Max(17f, 20f - (chapter - 5) * 1f);
                data.mathRoundDuration = Mathf.Max(25f, 30f - (chapter - 5) * 1f);
                data.mathTargetMin = 500;
                data.mathTargetMax = 999;
                data.bigNumberCount = 3;
                data.hasSpeedBonus = true;
                data.speedBonusThreshold = 0.5f;
                break;
        }

        return data;
    }

    // ──────────────────────────────────────────
    // Boss Level
    // ──────────────────────────────────────────

    private static void SetBossLevelData(LevelData data, int chapter)
    {
        switch (chapter)
        {
            case 1:
                data.letterCount = 6;
                data.wordMinLength = 4;
                data.wordMaxLength = 7;
                data.wordRoundDuration = 25f;
                data.mathRoundDuration = 35f;
                data.mathTargetMin = 400;
                data.mathTargetMax = 700;
                data.bigNumberCount = 2;
                break;
            case 2:
                data.letterCount = 6;
                data.wordMinLength = 5;
                data.wordMaxLength = 8;
                data.wordRoundDuration = 22f;
                data.mathRoundDuration = 32f;
                data.mathTargetMin = 500;
                data.mathTargetMax = 800;
                data.bigNumberCount = 3;
                data.hasRequiredLetter = true;
                break;
            case 3:
                data.letterCount = 6;
                data.wordMinLength = 5;
                data.wordMaxLength = 8;
                data.wordRoundDuration = 20f;
                data.mathRoundDuration = 30f;
                data.mathTargetMin = 600;
                data.mathTargetMax = 900;
                data.bigNumberCount = 3;
                data.minimumWordLength = 5;
                break;
            case 4:
                data.letterCount = 6;
                data.wordMinLength = 6;
                data.wordMaxLength = 9;
                data.wordRoundDuration = 18f;
                data.mathRoundDuration = 28f;
                data.mathTargetMin = 700;
                data.mathTargetMax = 950;
                data.bigNumberCount = 3;
                data.maxOperationSteps = 3;
                break;
            default:
                data.letterCount = 6;
                data.wordMinLength = 6;
                data.wordMaxLength = 9;
                data.wordRoundDuration = 16f;
                data.mathRoundDuration = 25f;
                data.mathTargetMin = 700;
                data.mathTargetMax = 999;
                data.bigNumberCount = 3;
                data.hasSpeedBonus = true;
                data.speedBonusThreshold = 0.4f;
                break;
        }

        data.bannedOperation = 0;
    }
}

// ──────────────────────────────────────────
// Akıllı Yasak İşlem Seçici
// ──────────────────────────────────────────

public static class BannedOperationSelector
{
    public static int SelectBannedOperation(List<int> numbers, int target)
    {
        List<int> candidates = new List<int> { 1, 2, 3, 4 };
        Shuffle(candidates);

        foreach (int op in candidates)
        {
            if (CanReachTarget(numbers, target, op))
                return op;
        }

        return 0;
    }

    private static bool CanReachTarget(List<int> numbers, int target, int bannedOp)
    {
        if (numbers.Count == 1)
            return numbers[0] == target;

        for (int i = 0; i < numbers.Count; i++)
        {
            for (int j = 0; j < numbers.Count; j++)
            {
                if (i == j) continue;

                int a = numbers[i];
                int b = numbers[j];

                List<int> remaining = new List<int>();
                for (int k = 0; k < numbers.Count; k++)
                    if (k != i && k != j) remaining.Add(numbers[k]);

                for (int op = 1; op <= 4; op++)
                {
                    if (op == bannedOp) continue;

                    int result;
                    if (!TryCalculate(a, b, op, out result)) continue;

                    List<int> next = new List<int>(remaining) { result };
                    if (CanReachTarget(next, target, bannedOp))
                        return true;
                }
            }
        }

        return false;
    }

    private static bool TryCalculate(int a, int b, int op, out int result)
    {
        result = 0;
        switch (op)
        {
            case 1: result = a + b; return true;
            case 2: result = a - b; return result > 0;
            case 3: result = a * b; return true;
            case 4:
                if (b == 0 || a % b != 0) return false;
                result = a / b; return true;
        }
        return false;
    }

    private static void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            int temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }
}