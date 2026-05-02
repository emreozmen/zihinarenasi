using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordRoundManager : MonoBehaviour
{
    [Header("References")]
    public LetterGenerator letterGenerator;
    public WordDictionary wordDictionary;

    [Header("UI")]
    public TMP_Text lettersText;
    public TMP_InputField wordInputField;
    public TMP_Text feedbackText;
    public TMP_Text timerText;
    public Button submitButton;

    [Header("Zorunlu Harf UI")]
    public TMP_Text requiredLetterText;

    [Header("Minimum Kelime Uzunluğu UI")]
    public TMP_Text minWordLengthText;

    [Header("Hız Bonusu UI")]
    public TMP_Text speedBonusText;

    [Header("Seed Kelime İpucu")]
    public Button seedWordHintButton;
    public TMP_Text seedWordHintButtonText;
    public TMP_Text seedWordRevealText;

    [Header("Boss Level Popup")]
    public GameObject bossPopup;
    public TMP_Text bossPopupText;
    public Button bossPopupReadyButton;

    [Header("Butonlar")]
    public Button hintButton;
    public Button passButton;

    private List<char> currentLetters = new List<char>();
    private float timeRemaining;
    private float totalTime;
    private bool isRoundActive;

    private string selectedSeedWord;
    private WordEntry bestWordEntry;
    private LevelData currentLevelData;
    private bool seedWordRevealed = false;

    // ──────────────────────────────────────────
    // Başlangıç
    // ──────────────────────────────────────────

    private void Start()
    {
        if (bossPopup != null)
            bossPopup.SetActive(false);

        if (requiredLetterText != null)
            requiredLetterText.gameObject.SetActive(false);

        if (minWordLengthText != null)
            minWordLengthText.gameObject.SetActive(false);

        if (speedBonusText != null)
            speedBonusText.gameObject.SetActive(false);

        if (seedWordHintButton != null)
            seedWordHintButton.gameObject.SetActive(false);

        if (seedWordRevealText != null)
            seedWordRevealText.gameObject.SetActive(false);

        if (wordInputField != null)
            wordInputField.onSubmit.AddListener((_) => SubmitWord());
    }

    // ──────────────────────────────────────────
    // Round Başlat
    // ──────────────────────────────────────────

    public void BeginRound(LevelData levelData)
    {
        currentLevelData = levelData;
        seedWordRevealed = false;

        if (letterGenerator == null || wordDictionary == null)
        {
            Debug.LogError("WordRoundManager referansları eksik.");
            return;
        }

        selectedSeedWord = wordDictionary.GetRandomWord(levelData.wordMinLength, levelData.wordMaxLength);

        if (string.IsNullOrEmpty(selectedSeedWord))
        {
            Debug.LogError("Bu level için uygun kelime bulunamadı.");
            return;
        }

        int letterCount = levelData.letterCount > 0 ? levelData.letterCount : 8;

        if (levelData.hasRequiredLetter)
            currentLetters = letterGenerator.GenerateLettersFromWord(selectedSeedWord, letterCount, levelData);
        else
            currentLetters = letterGenerator.GenerateLettersFromWord(selectedSeedWord, letterCount);

        bestWordEntry = WordDatabaseService.Instance.GetBestWord(currentLetters);

        lettersText.text = string.Join("  ", currentLetters);
        wordInputField.text = "";
        feedbackText.text = "";

        UpdateRequiredLetterUI(levelData);
        UpdateMinWordLengthUI(levelData);
        UpdateSpeedBonusUI(levelData);
        UpdateSeedWordHintButton(levelData);

        if (levelData.isBossLevel)
            ShowBossPopup(levelData);
        else
            StartRound(levelData);
    }

    // ──────────────────────────────────────────
    // UI Göstergeleri
    // ──────────────────────────────────────────

    private void UpdateRequiredLetterUI(LevelData levelData)
    {
        if (requiredLetterText == null) return;

        if (!levelData.hasRequiredLetter)
        {
            requiredLetterText.gameObject.SetActive(false);
            return;
        }

        requiredLetterText.gameObject.SetActive(true);
        char requiredChar = currentLetters[levelData.requiredLetterIndex];
        requiredLetterText.text = $"Zorunlu harf: <color=#FFD700><b>{requiredChar}</b></color>";
    }

    private void UpdateMinWordLengthUI(LevelData levelData)
    {
        if (minWordLengthText == null) return;

        if (levelData.minimumWordLength <= 0)
        {
            minWordLengthText.gameObject.SetActive(false);
            return;
        }

        minWordLengthText.gameObject.SetActive(true);
        minWordLengthText.text = $"En az <color=#FF6B6B><b>{levelData.minimumWordLength}</b></color> harfli kelime!";
    }

    private void UpdateSpeedBonusUI(LevelData levelData)
    {
        if (speedBonusText == null) return;

        if (!levelData.hasSpeedBonus)
        {
            speedBonusText.gameObject.SetActive(false);
            return;
        }

        speedBonusText.gameObject.SetActive(true);
        int thresholdPercent = Mathf.RoundToInt(levelData.speedBonusThreshold * 100f);
        speedBonusText.text = $"⚡ Sürenin %{thresholdPercent}'inde bitirirsen bonus puan!";
    }

    private void UpdateSeedWordHintButton(LevelData levelData)
    {
        if (seedWordHintButton == null) return;

        if (!levelData.showSeedWordHint)
        {
            seedWordHintButton.gameObject.SetActive(false);
            if (seedWordRevealText != null) seedWordRevealText.gameObject.SetActive(false);
            return;
        }

        seedWordHintButton.gameObject.SetActive(true);
        seedWordHintButton.interactable = true;

        if (seedWordHintButtonText != null)
            seedWordHintButtonText.text = "Kelimeyi Gör";

        if (seedWordRevealText != null)
            seedWordRevealText.gameObject.SetActive(false);

        seedWordHintButton.onClick.RemoveAllListeners();
        seedWordHintButton.onClick.AddListener(OnSeedWordHintButton);
    }

    // ──────────────────────────────────────────
    // Seed Kelime İpucu Butonu
    // ──────────────────────────────────────────

    private void OnSeedWordHintButton()
    {
        if (!isRoundActive) return;
        if (seedWordRevealed) return;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (seedWordHintButton != null)
            seedWordHintButton.interactable = false;

        if (AdManager.Instance != null)
            AdManager.Instance.ShowRewarded(() => RevealSeedWord());
        else
            RevealSeedWord();
    }

    private void RevealSeedWord()
    {
        seedWordRevealed = true;

        if (seedWordHintButton != null)
            seedWordHintButton.gameObject.SetActive(false);

        if (seedWordRevealText != null)
        {
            seedWordRevealText.gameObject.SetActive(true);
            seedWordRevealText.text = $"İpucu: <color=#FFD700><b>{selectedSeedWord.ToUpper()}</b></color>";
        }
    }

    // ──────────────────────────────────────────
    // Boss Level Popup
    // ──────────────────────────────────────────

    private void ShowBossPopup(LevelData levelData)
    {
        if (bossPopup == null) { StartRound(levelData); return; }

        string bannedOpName = GetBannedOperationName(levelData.bannedOperation);
        string extras = "";

        if (levelData.hasRequiredLetter)
            extras += "\n⚠️ Zorunlu harf kullanmalısın!";
        if (levelData.minimumWordLength > 0)
            extras += $"\n⚠️ En az {levelData.minimumWordLength} harfli kelime!";
        if (levelData.hasSpeedBonus)
            extras += "\n⚡ Hız bonusu aktif!";

        if (bossPopupText != null)
        {
            bossPopupText.text =
                $"Bu bölümde {levelData.letterCount} harf verilecek, " +
                $"süre daha kısa" +
                (levelData.bannedOperation > 0 ? $" ve <color=#E74C3C>{bannedOpName}</color> işlemi yasak!" : "!") +
                extras +
                $"\n\nKendine güven, başarabilirsin!";
        }

        bossPopup.SetActive(true);
        Time.timeScale = 0f;

        if (bossPopupReadyButton != null)
        {
            bossPopupReadyButton.onClick.RemoveAllListeners();
            bossPopupReadyButton.onClick.AddListener(() =>
            {
                bossPopup.SetActive(false);
                Time.timeScale = 1f;
                StartRound(levelData);
            });
        }
    }

    private string GetBannedOperationName(int banned)
    {
        switch (banned)
        {
            case 1: return "toplama (+)";
            case 2: return "çıkarma (-)";
            case 3: return "çarpma (x)";
            case 4: return "bölme (÷)";
            default: return "";
        }
    }

    private void StartRound(LevelData levelData)
    {
        totalTime = levelData.wordRoundDuration;
        timeRemaining = totalTime;
        isRoundActive = true;

        if (hintButton != null) hintButton.interactable = true;
        if (passButton != null) passButton.interactable = true;
    }

    // ──────────────────────────────────────────
    // Update
    // ──────────────────────────────────────────

    private void Update()
    {
        if (!isRoundActive) return;

        timeRemaining -= Time.deltaTime;
        if (timerText != null) timerText.text = Mathf.CeilToInt(timeRemaining).ToString();

        if (timeRemaining <= 0f)
            EndRound();
    }

    // ──────────────────────────────────────────
    // Kelime Gönder
    // ──────────────────────────────────────────

    public void SubmitWord()
    {
        if (!isRoundActive) return;

        string playerWord = wordInputField.text.Trim();

        if (string.IsNullOrEmpty(playerWord))
        {
            feedbackText.text = "Kelime gir.";
            return;
        }

        if (currentLevelData != null && currentLevelData.minimumWordLength > 0)
        {
            if (playerWord.Length < currentLevelData.minimumWordLength)
            {
                feedbackText.text = $"En az <color=#FF6B6B>{currentLevelData.minimumWordLength}</color> harfli kelime girmelisin!";
                if (AudioManager.Instance != null) AudioManager.Instance.PlayIncorrect();
                return;
            }
        }

        if (currentLevelData != null && currentLevelData.hasRequiredLetter)
        {
            char requiredChar = currentLetters[currentLevelData.requiredLetterIndex];
            string upperWord = playerWord.ToUpper(System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));

            if (!upperWord.Contains(requiredChar.ToString()))
            {
                feedbackText.text = $"<color=#FFD700>{requiredChar}</color> harfini kullanmalısın!";
                if (AudioManager.Instance != null) AudioManager.Instance.PlayIncorrect();
                return;
            }
        }

        int score = WordDatabaseService.Instance.CalculateWordScore(playerWord, currentLetters);

        if (score <= 0)
        {
            feedbackText.text = "Geçersiz kelime.";
            if (AudioManager.Instance != null) AudioManager.Instance.PlayIncorrect();
            return;
        }

        int bonusScore = 0;
        if (currentLevelData != null && currentLevelData.hasSpeedBonus)
        {
            float timeUsed = totalTime - timeRemaining;
            float timeRatio = timeUsed / totalTime;

            if (timeRatio <= currentLevelData.speedBonusThreshold)
            {
                bonusScore = 3;
                score += bonusScore;
            }
        }

        string normalized = TurkishAlphabet.Normalize(playerWord);

        if (bonusScore > 0)
            feedbackText.text = $"⚡ Hız bonusu! +{bonusScore} ekstra!\nKelimen: {normalized}\n+{score} puan";
        else
            feedbackText.text =
                "Senin kelimen: " + normalized +
                "\nEn iyi: " + (bestWordEntry != null ? bestWordEntry.word : "-") +
                "\n+" + score + " puan";

        if (AudioManager.Instance != null) AudioManager.Instance.PlayCorrect();

        isRoundActive = false;

        string bestWord = bestWordEntry != null ? bestWordEntry.word : "-";
        GameManager.Instance.OnWordRoundFinished(score, bestWord);
    }

    // ──────────────────────────────────────────
    // İpucu — Önce hak kontrol et, yoksa reklam
    // ──────────────────────────────────────────

    public void OnHintButton()
    {
        if (!isRoundActive) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        int hintCount = PlayerPrefs.GetInt("HintCount", 0);

        if (hintCount > 0)
        {
            // İpucu hakkı var — direkt kullan
            PlayerPrefs.SetInt("HintCount", hintCount - 1);
            PlayerPrefs.Save();

            // UI güncelle
            if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
                GameManager.Instance.uiManager.UpdateHintCount();

            GiveHint();

            if (hintButton != null) hintButton.interactable = false;
        }
        else
        {
            // İpucu hakkı yok — reklam izle
            if (hintButton != null) hintButton.interactable = false;

            if (AdManager.Instance != null)
                AdManager.Instance.ShowRewarded(() => GiveHint());
            else
                GiveHint();
        }
    }

    private void GiveHint()
    {
        if (bestWordEntry == null) { feedbackText.text = "İpucu bulunamadı."; return; }

        string hint = bestWordEntry.word.Substring(0, Mathf.Min(2, bestWordEntry.word.Length));
        feedbackText.text = $"İpucu: En iyi kelime \"{hint.ToUpper()}...\" ile başlıyor. ({bestWordEntry.length} harf)";

        if (currentLevelData != null && currentLevelData.hasRequiredLetter)
        {
            char requiredChar = currentLetters[currentLevelData.requiredLetterIndex];
            feedbackText.text += $"\nZorunlu harf: <color=#FFD700>{requiredChar}</color>";
        }
    }

    // ──────────────────────────────────────────
    // Pas
    // ──────────────────────────────────────────

    public void OnPassButton()
    {
        if (!isRoundActive) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        isRoundActive = false;
        feedbackText.text = "Pas geçildi.";

        if (passButton != null) passButton.interactable = false;

        GameManager.Instance.OnWordRoundPassed();
    }

    // ──────────────────────────────────────────
    // Süre Doldu
    // ──────────────────────────────────────────

    private void EndRound()
    {
        isRoundActive = false;
        feedbackText.text = "Süre doldu!\nEn iyi kelime: " + (bestWordEntry != null ? bestWordEntry.word : "-");

        string bestWord = bestWordEntry != null ? bestWordEntry.word : "-";
        GameManager.Instance.OnWordRoundFinished(0, bestWord);
    }
}