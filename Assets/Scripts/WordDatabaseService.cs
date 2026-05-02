using System.Collections.Generic;
using UnityEngine;

public class WordDatabaseService : MonoBehaviour
{
    public static WordDatabaseService Instance;

    private WordDatabase database;
    private HashSet<string> validWords = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadDatabase()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Dictionaries/word_database");

        if (jsonFile == null)
        {
            Debug.LogError("word_database.json bulunamadı. Önce Tools > Word Engine > Build Word Database çalıştır.");
            return;
        }

        database = JsonUtility.FromJson<WordDatabase>(jsonFile.text);

        validWords.Clear();

        if (database != null && database.entries != null)
        {
            foreach (WordEntry entry in database.entries)
            {
                validWords.Add(entry.word);
            }
        }

        Debug.Log("Word database yüklendi. Kelime sayısı: " + validWords.Count);
    }

    public bool IsValidWord(string word)
    {
        return validWords.Contains(TurkishAlphabet.Normalize(word));
    }

    public List<WordEntry> GetPossibleWords(List<char> letters)
    {
        List<WordEntry> results = new List<WordEntry>();

        if (database == null || database.entries == null)
            return results;

        int[] availableCounts = TurkishAlphabet.BuildCountsFromLetters(letters);

        foreach (WordEntry entry in database.entries)
        {
            if (TurkishAlphabet.CanBuild(entry.counts, availableCounts))
            {
                results.Add(entry);
            }
        }

        return results;
    }

    public WordEntry GetBestWord(List<char> letters)
    {
        WordEntry best = null;

        if (database == null || database.entries == null)
            return best;

        int[] availableCounts = TurkishAlphabet.BuildCountsFromLetters(letters);

        foreach (WordEntry entry in database.entries)
        {
            if (!TurkishAlphabet.CanBuild(entry.counts, availableCounts))
                continue;

            if (best == null)
            {
                best = entry;
                continue;
            }

            if (entry.length > best.length)
            {
                best = entry;
            }
            else if (entry.length == best.length && string.Compare(entry.word, best.word, System.StringComparison.Ordinal) < 0)
            {
                best = entry;
            }
        }

        return best;
    }

    public int CalculateWordScore(string playerWord, List<char> letters)
    {
        if (!IsValidWord(playerWord))
            return 0;

        string normalized = TurkishAlphabet.Normalize(playerWord);
        int[] playerCounts = TurkishAlphabet.BuildCountsFromWord(normalized);
        int[] availableCounts = TurkishAlphabet.BuildCountsFromLetters(letters);

        if (!TurkishAlphabet.CanBuild(playerCounts, availableCounts))
            return 0;

        WordEntry best = GetBestWord(letters);

        if (best == null)
            return 0;

        int playerLength = normalized.Length;
        int difference = best.length - playerLength;

        if (difference <= 0) return 10;
        if (difference == 1) return 7;
        if (difference == 2) return 5;
        if (difference == 3) return 3;
        return 1;
    }
}