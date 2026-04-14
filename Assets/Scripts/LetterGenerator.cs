using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class LetterGenerator : MonoBehaviour
{
    private readonly char[] extraLetters =
    {
        'A','E','I','İ','O','Ö','U','Ü',
        'B','C','Ç','D','F','G','Ğ','H','J',
        'K','L','M','N','P','R','S','Ş','T','V','Y','Z'
    };

    private CultureInfo trCulture = new CultureInfo("tr-TR");

    /// <summary>
    /// LevelData ile birlikte çağrıldığında zorunlu harf index'ini de set eder.
    /// </summary>
    public List<char> GenerateLettersFromWord(string baseWord, int totalCount, LevelData levelData)
    {
        List<char> result = new List<char>();
        string upperWord = baseWord.ToUpper(trCulture);

        // Seed kelimeden harfleri ekle
        int seedLetterCount = 0;
        foreach (char c in upperWord)
        {
            result.Add(c);
            seedLetterCount++;
            if (result.Count >= totalCount) break;
        }

        // Eksik harfleri rastgele tamamla
        while (result.Count < totalCount)
            result.Add(extraLetters[Random.Range(0, extraLetters.Length)]);

        // Karıştır
        Shuffle(result);

        // Zorunlu harf belirle (seed kelimeden gelen harflerden biri)
        if (levelData != null && levelData.hasRequiredLetter && seedLetterCount > 0)
        {
            // Seed harflerinden rastgele birini seç
            // Shuffle sonrası hangi harflerin seed'den geldiğini bilemeyiz,
            // bu yüzden sadece index 0-seedLetterCount arasından seçiyoruz
            int safeMax = Mathf.Min(seedLetterCount, result.Count);
            levelData.requiredLetterIndex = Random.Range(0, safeMax);
        }

        return result;
    }

    /// <summary>
    /// Geriye dönük uyumluluk — LevelData olmadan çağrılabilir.
    /// </summary>
    public List<char> GenerateLettersFromWord(string baseWord, int totalCount = 8)
    {
        List<char> result = new List<char>();
        string upperWord = baseWord.ToUpper(trCulture);

        foreach (char c in upperWord)
        {
            result.Add(c);
            if (result.Count >= totalCount) break;
        }

        while (result.Count < totalCount)
            result.Add(extraLetters[Random.Range(0, extraLetters.Length)]);

        Shuffle(result);
        return result;
    }

    private void Shuffle(List<char> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }
}