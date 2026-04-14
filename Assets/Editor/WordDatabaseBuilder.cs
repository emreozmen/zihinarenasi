using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class WordDatabaseBuilder
{
    private const string RawPath = "Assets/Resources/Dictionaries/words_tr_raw.txt";
    private const string OutputPath = "Assets/Resources/Dictionaries/word_database.json";

    [MenuItem("Tools/Word Engine/Build Word Database")]
    public static void BuildDatabase()
    {
        if (!File.Exists(RawPath))
        {
            Debug.LogError("Ham sözlük bulunamadı: " + RawPath);
            return;
        }

        string[] lines = File.ReadAllLines(RawPath);
        HashSet<string> uniqueWords = new HashSet<string>();
        WordDatabase database = new WordDatabase();

        foreach (string rawLine in lines)
        {
            string word = TurkishAlphabet.Normalize(rawLine);

            if (string.IsNullOrWhiteSpace(word))
                continue;

            if (word.Length < 3 || word.Length > 9)
                continue;

            if (!TurkishAlphabet.ContainsOnlyTurkishLetters(word))
                continue;

            if (uniqueWords.Contains(word))
                continue;

            uniqueWords.Add(word);

            WordEntry entry = new WordEntry
            {
                word = word,
                length = word.Length,
                counts = TurkishAlphabet.BuildCountsFromWord(word)
            };

            database.entries.Add(entry);
        }

        string json = JsonUtility.ToJson(database, true);
        File.WriteAllText(OutputPath, json);

        AssetDatabase.Refresh();

        Debug.Log("Kelime veritabanı oluşturuldu. Toplam kelime: " + database.entries.Count);
        Debug.Log("Kayıt yeri: " + OutputPath);
    }
}