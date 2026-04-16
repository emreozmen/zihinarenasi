using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MathRoundManager : MonoBehaviour
{
    public TMP_Text targetValueText;
    public TMP_Text timerText;
    public TMP_Text feedbackText;
    public TMP_Text lastOperationText;

    [Header("Number Tiles")]
    public List<NumberTile> numberTiles;

    [Header("İşlem Butonları")]
    public UnityEngine.UI.Button addButton;
    public UnityEngine.UI.Button subtractButton;
    public UnityEngine.UI.Button multiplyButton;
    public UnityEngine.UI.Button divideButton;

    [Header("Diğer Butonlar")]
    public UnityEngine.UI.Button hintButton;
    public UnityEngine.UI.Button passButton;

    [Header("Adım Sınırı UI")]
    public TMP_Text stepLimitText;

    [Header("Hız Bonusu UI")]
    public TMP_Text speedBonusText;

    private float timeRemaining;
    private float totalTime;
    private bool isRoundActive;

    private NumberTile firstSelected;
    private NumberTile secondSelected;
    private MathOperation currentOperation = MathOperation.None;

    private int target;
    private List<int> activeNumbers = new List<int>();
    private int bannedOperation = 0;
    private int maxOperationSteps = 0;
    private int currentSteps = 0;
    private bool hasSpeedBonus = false;
    private float speedBonusThreshold = 0.5f;

    public enum MathOperation { None, Add, Subtract, Multiply, Divide }

    // ──────────────────────────────────────────
    // Başlangıç
    // ──────────────────────────────────────────

    public void BeginRound(LevelData levelData)
    {
        Time.timeScale = 1f;
        target = Random.Range(levelData.mathTargetMin, levelData.mathTargetMax + 1);
        targetValueText.text = target.ToString();

        activeNumbers = GenerateStartNumbers(levelData.bigNumberCount);

        if (levelData.isBossLevel)
            bannedOperation = BannedOperationSelector.SelectBannedOperation(new List<int>(activeNumbers), target);
        else
            bannedOperation = 0;

        maxOperationSteps = levelData.maxOperationSteps;
        currentSteps = 0;
        hasSpeedBonus = levelData.hasSpeedBonus;
        speedBonusThreshold = levelData.speedBonusThreshold;

        feedbackText.text = "2 sayı ve işlem seç.";
        lastOperationText.text = "";

        totalTime = levelData.mathRoundDuration;
        timeRemaining = totalTime;
        isRoundActive = true;

        firstSelected = null;
        secondSelected = null;
        currentOperation = MathOperation.None;

        if (hintButton != null) hintButton.interactable = true;
        if (passButton != null) passButton.interactable = true;

        ApplyBannedOperation(bannedOperation);
        UpdateStepLimitUI();
        UpdateSpeedBonusUI(levelData);

        levelData.bannedOperation = bannedOperation;

        RefreshNumberTiles();
    }

    // ──────────────────────────────────────────
    // UI Göstergeleri
    // ──────────────────────────────────────────

    private void UpdateStepLimitUI()
    {
        if (stepLimitText == null) return;

        if (maxOperationSteps <= 0)
        {
            stepLimitText.gameObject.SetActive(false);
            return;
        }

        int remaining = maxOperationSteps - currentSteps;
        stepLimitText.gameObject.SetActive(true);
        stepLimitText.text = $"Kalan işlem: <b>{remaining}</b>";
        stepLimitText.color = remaining <= 1
            ? new Color(0.9f, 0.2f, 0.2f)
            : Color.white;
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

    // ──────────────────────────────────────────
    // Yasak İşlem
    // ──────────────────────────────────────────

    private void ApplyBannedOperation(int banned)
    {
        if (addButton != null) addButton.interactable = true;
        if (subtractButton != null) subtractButton.interactable = true;
        if (multiplyButton != null) multiplyButton.interactable = true;
        if (divideButton != null) divideButton.interactable = true;

        switch (banned)
        {
            case 1: if (addButton != null) addButton.interactable = false; break;
            case 2: if (subtractButton != null) subtractButton.interactable = false; break;
            case 3: if (multiplyButton != null) multiplyButton.interactable = false; break;
            case 4: if (divideButton != null) divideButton.interactable = false; break;
        }
    }

    // ──────────────────────────────────────────
    // Update
    // ──────────────────────────────────────────

    private void Update()
    {
        if (!isRoundActive) return;

        timeRemaining -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(timeRemaining).ToString();

        if (timeRemaining <= 0f)
            EndRound();
    }

    // ──────────────────────────────────────────
    // Sayı Seçimi
    // ──────────────────────────────────────────

    public void OnNumberSelected(NumberTile tile)
    {
        if (!isRoundActive) return;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        // İlk sayı seçilmemişse — seç
        if (firstSelected == null)
        {
            firstSelected = tile;
            firstSelected.SetSelected(true);
            feedbackText.text = "İlk sayı: " + tile.Value + "\nŞimdi bir işlem seç!";
            return;
        }

        // İlk sayıya tekrar basıldıysa — seçimi kaldır
        if (tile == firstSelected)
        {
            firstSelected.SetSelected(false);
            firstSelected = null;
            feedbackText.text = "2 sayı ve işlem seç.";
            return;
        }

        // İlk sayı seçili ama işlem seçilmemişse — uyar, ikinci sayı seçilemesin
        if (currentOperation == MathOperation.None)
        {
            feedbackText.text = "Önce işlem seç!";
            if (AudioManager.Instance != null) AudioManager.Instance.PlayIncorrect();
            return;
        }

        // İkinci sayı seçilmemişse — seç
        if (secondSelected == null)
        {
            secondSelected = tile;
            secondSelected.SetSelected(true);
            feedbackText.text = "İkinci sayı: " + tile.Value;
            return;
        }

        // İkinci sayıya tekrar basıldıysa — seçimi kaldır
        if (tile == secondSelected)
        {
            secondSelected.SetSelected(false);
            secondSelected = null;
            feedbackText.text = "İkinci seçim kaldırıldı.";
        }
    }

    // ──────────────────────────────────────────
    // İşlem Seçimi
    // ──────────────────────────────────────────

    public void SelectAdd()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        currentOperation = MathOperation.Add;
        feedbackText.text = firstSelected != null
            ? $"İlk sayı: {firstSelected.Value} | İşlem: +"
            : "İşlem: +";
    }

    public void SelectSubtract()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        currentOperation = MathOperation.Subtract;
        feedbackText.text = firstSelected != null
            ? $"İlk sayı: {firstSelected.Value} | İşlem: -"
            : "İşlem: -";
    }

    public void SelectMultiply()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        currentOperation = MathOperation.Multiply;
        feedbackText.text = firstSelected != null
            ? $"İlk sayı: {firstSelected.Value} | İşlem: x"
            : "İşlem: x";
    }

    public void SelectDivide()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        currentOperation = MathOperation.Divide;
        feedbackText.text = firstSelected != null
            ? $"İlk sayı: {firstSelected.Value} | İşlem: ÷"
            : "İşlem: ÷";
    }

    // ──────────────────────────────────────────
    // İşlem Uygula
    // ──────────────────────────────────────────

    public void ApplyOperation()
    {
        if (!isRoundActive) return;

        if (firstSelected == null || secondSelected == null || currentOperation == MathOperation.None)
        {
            feedbackText.text = "Önce 2 sayı ve işlem seç.";
            return;
        }

        if (maxOperationSteps > 0 && currentSteps >= maxOperationSteps)
        {
            feedbackText.text = "İşlem hakkın bitti!";
            if (AudioManager.Instance != null) AudioManager.Instance.PlayIncorrect();
            FinishMathRoundWithValue(GetClosestValueToTarget());
            return;
        }

        int a = firstSelected.Value;
        int b = secondSelected.Value;

        if (!TryCalculate(a, b, currentOperation, out int result))
        {
            feedbackText.text = "Geçersiz işlem.";
            if (AudioManager.Instance != null) AudioManager.Instance.PlayIncorrect();
            return;
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayCorrect();

        string operationSymbol = GetOperationSymbol(currentOperation);
        lastOperationText.text = a + " " + operationSymbol + " " + b + " = " + result;
        feedbackText.text = "";

        RemoveOneValueFromActiveNumbers(a);
        RemoveOneValueFromActiveNumbers(b);
        activeNumbers.Add(result);

        currentSteps++;
        UpdateStepLimitUI();

        ClearSelections();
        RefreshNumberTiles();

        if (result == target)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayLevelComplete();
            FinishMathRoundWithValue(result);
            return;
        }

        if (activeNumbers.Count == 1)
            FinishMathRoundWithValue(activeNumbers[0]);
    }

    // ──────────────────────────────────────────
    // İpucu
    // ──────────────────────────────────────────

    public void OnHintButton()
    {
        if (!isRoundActive) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        int hintCount = PlayerPrefs.GetInt("HintCount", 0);

        if (hintCount > 0)
        {
            PlayerPrefs.SetInt("HintCount", hintCount - 1);
            PlayerPrefs.Save();

            if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
                GameManager.Instance.uiManager.UpdateHintCount();

            GiveHint();
            if (hintButton != null) hintButton.interactable = false;
        }
        else
        {
            if (hintButton != null) hintButton.interactable = false;

            if (AdManager.Instance != null)
                AdManager.Instance.ShowRewarded(() => GiveHint());
            else
                GiveHint();
        }
    }

    private void GiveHint()
    {
        int closest = GetClosestValueToTarget();
        int diff = Mathf.Abs(target - closest);
        feedbackText.text = $"İpucu: En yakın sayı {closest} (fark: {diff})";
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

        GameManager.Instance.OnMathRoundPassed();
    }

    // ──────────────────────────────────────────
    // Erken Bitir
    // ──────────────────────────────────────────

    public void FinishEarly()
    {
        if (!isRoundActive) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        isRoundActive = false;
        feedbackText.text = "Pes edildi.";

        GameManager.Instance.OnMathRoundFinished(0, target, 0, target);
    }

    private void FinishMathRoundWithValue(int finalValue)
    {
        isRoundActive = false;

        int difference = Mathf.Abs(target - finalValue);
        int score = CalculateScore(difference);

        int bonusScore = 0;
        if (hasSpeedBonus && difference == 0)
        {
            float timeUsed = totalTime - timeRemaining;
            float timeRatio = timeUsed / totalTime;

            if (timeRatio <= speedBonusThreshold)
            {
                bonusScore = 3;
                score += bonusScore;
            }
        }

        if (bonusScore > 0)
            feedbackText.text = $"⚡ Tam isabet + Hız bonusu! +{score} puan";
        else if (difference == 0)
            feedbackText.text = "Tam isabet! +" + score + " puan";
        else
            feedbackText.text = "Sonucun: " + finalValue + " | Fark: " + difference + " | +" + score + " puan";

        GameManager.Instance.OnMathRoundFinished(score, target, finalValue, difference);
    }

    private int CalculateScore(int difference)
    {
        if (difference == 0) return 10;
        if (difference <= 5) return 7;
        if (difference <= 10) return 5;
        if (difference <= 20) return 3;
        return 0;
    }

    // ──────────────────────────────────────────
    // Yardımcılar
    // ──────────────────────────────────────────

    public bool TryCalculate(int a, int b, MathOperation op, out int result)
    {
        result = 0;
        switch (op)
        {
            case MathOperation.Add: result = a + b; return true;
            case MathOperation.Subtract: result = a - b; return result > 0;
            case MathOperation.Multiply: result = a * b; return true;
            case MathOperation.Divide:
                if (b == 0 || a % b != 0) return false;
                result = a / b; return true;
        }
        return false;
    }

    private void RefreshNumberTiles()
    {
        for (int i = 0; i < numberTiles.Count; i++)
        {
            if (i < activeNumbers.Count)
            {
                numberTiles[i].gameObject.SetActive(true);
                numberTiles[i].Setup(activeNumbers[i], this);
            }
            else
            {
                numberTiles[i].gameObject.SetActive(false);
            }
        }
    }

    private void RemoveOneValueFromActiveNumbers(int value)
    {
        for (int i = 0; i < activeNumbers.Count; i++)
        {
            if (activeNumbers[i] == value)
            {
                activeNumbers.RemoveAt(i);
                return;
            }
        }
    }

    private void ClearSelections()
    {
        if (firstSelected != null) firstSelected.SetSelected(false);
        if (secondSelected != null) secondSelected.SetSelected(false);
        firstSelected = null;
        secondSelected = null;
        currentOperation = MathOperation.None;
    }

    private string GetOperationSymbol(MathOperation op)
    {
        switch (op)
        {
            case MathOperation.Add: return "+";
            case MathOperation.Subtract: return "-";
            case MathOperation.Multiply: return "x";
            case MathOperation.Divide: return "÷";
            default: return "?";
        }
    }

    private List<int> GenerateStartNumbers(int bigNumberCount)
    {
        List<int> numbers = new List<int>();
        int smallNumberCount = 6 - bigNumberCount;

        for (int i = 0; i < smallNumberCount; i++)
            numbers.Add(Random.Range(1, 11));

        int[] bigNumbers = { 25, 50, 75, 100 };
        for (int i = 0; i < bigNumberCount; i++)
            numbers.Add(bigNumbers[Random.Range(0, bigNumbers.Length)]);

        Shuffle(numbers);
        return numbers;
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            int temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }

    private void EndRound()
    {
        if (!isRoundActive) return;
        isRoundActive = false;
        feedbackText.text = "Süre doldu!";
        GameManager.Instance.OnMathRoundFinished(0, target, 0, target);
    }

    private int GetClosestValueToTarget()
    {
        if (activeNumbers.Count == 0) return 0;

        int closest = activeNumbers[0];
        int bestDiff = Mathf.Abs(target - closest);

        for (int i = 1; i < activeNumbers.Count; i++)
        {
            int diff = Mathf.Abs(target - activeNumbers[i]);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                closest = activeNumbers[i];
            }
        }
        return closest;
    }
}