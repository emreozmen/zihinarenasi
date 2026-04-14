using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class WordDictionary : MonoBehaviour
{
    private HashSet<string> words = new HashSet<string>();
    private List<string> wordList = new List<string>();
    private CultureInfo trCulture = new CultureInfo("tr-TR");

    private void Awake()
    {
        LoadDictionary();
    }

    private void LoadDictionary()
    {
        TextAsset dictionaryFile = Resources.Load<TextAsset>("Dictionaries/words_tr");

        if (dictionaryFile == null)
        {
            Debug.LogError("Sözlük dosyası bulunamadı: Resources/Dictionaries/words_tr.txt");
            return;
        }

        string[] lines = dictionaryFile.text.Split('\n');

        foreach (string line in lines)
        {
            string word = Normalize(line.Trim());

            if (!string.IsNullOrEmpty(word) && !words.Contains(word))
            {
                words.Add(word);
                wordList.Add(word);
            }
        }

        Debug.Log("Sözlük yüklendi. Kelime sayısı: " + wordList.Count);
    }

    public bool IsValidWord(string word)
    {
        return words.Contains(Normalize(word));
    }

    public string GetRandomWord(int minLength = 5, int maxLength = 7)
    {
        List<string> filtered = wordList.FindAll(w => w.Length >= minLength && w.Length <= maxLength);

        if (filtered.Count == 0)
        {
            Debug.LogWarning("Uygun uzunlukta kelime bulunamadı.");
            return null;
        }

        return filtered[Random.Range(0, filtered.Count)];
    }

    public List<string> GetPossibleWords(List<char> letters)
    {
        List<string> possibleWords = new List<string>();

        foreach (string word in wordList)
        {
            if (CanBuildWord(word, letters))
            {
                possibleWords.Add(word);
            }
        }

        return possibleWords;
    }

    public string GetLongestPossibleWord(List<char> letters)
    {
        string bestWord = "";

        foreach (string word in wordList)
        {
            if (CanBuildWord(word, letters))
            {
                if (word.Length > bestWord.Length)
                {
                    bestWord = word;
                }
            }
        }

        return bestWord;
    }

    public int GetBestPossibleLength(List<char> letters)
    {
        int bestLength = 0;

        foreach (string word in wordList)
        {
            if (CanBuildWord(word, letters))
            {
                if (word.Length > bestLength)
                {
                    bestLength = word.Length;
                }
            }
        }

        return bestLength;
    }

    public bool CanBuildWord(string word, List<char> letters)
    {
        List<char> available = new List<char>(letters);
        string upperWord = word.ToUpper(trCulture);

        foreach (char c in upperWord)
        {
            if (available.Contains(c))
            {
                available.Remove(c);
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private string Normalize(string input)
    {
        return input.Trim().ToLower(trCulture);
    }
}