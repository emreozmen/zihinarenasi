using System.Globalization;

public static class TurkishAlphabet
{
    public static readonly char[] Letters =
    {
        'A','B','C','Ç','D','E','F','G','Ğ','H','I','İ','J','K','L',
        'M','N','O','Ö','P','R','S','Ş','T','U','Ü','V','Y','Z'
    };

    private static readonly CultureInfo Tr = new CultureInfo("tr-TR");

    public static string Normalize(string input)
    {
        return input.Trim().ToLower(Tr);
    }

    public static string ToUpperTr(string input)
    {
        return input.ToUpper(Tr);
    }

    public static int GetIndex(char c)
    {
        switch (c)
        {
            case 'A': return 0;
            case 'B': return 1;
            case 'C': return 2;
            case 'Ç': return 3;
            case 'D': return 4;
            case 'E': return 5;
            case 'F': return 6;
            case 'G': return 7;
            case 'Ğ': return 8;
            case 'H': return 9;
            case 'I': return 10;
            case 'İ': return 11;
            case 'J': return 12;
            case 'K': return 13;
            case 'L': return 14;
            case 'M': return 15;
            case 'N': return 16;
            case 'O': return 17;
            case 'Ö': return 18;
            case 'P': return 19;
            case 'R': return 20;
            case 'S': return 21;
            case 'Ş': return 22;
            case 'T': return 23;
            case 'U': return 24;
            case 'Ü': return 25;
            case 'V': return 26;
            case 'Y': return 27;
            case 'Z': return 28;
            default: return -1;
        }
    }

    public static bool ContainsOnlyTurkishLetters(string word)
    {
        string upper = ToUpperTr(word);

        foreach (char c in upper)
        {
            if (GetIndex(c) < 0)
                return false;
        }

        return true;
    }

    public static int[] BuildCountsFromWord(string word)
    {
        int[] counts = new int[Letters.Length];
        string upper = ToUpperTr(word);

        foreach (char c in upper)
        {
            int index = GetIndex(c);
            if (index >= 0)
                counts[index]++;
        }

        return counts;
    }

    public static int[] BuildCountsFromLetters(System.Collections.Generic.List<char> letters)
    {
        int[] counts = new int[Letters.Length];

        foreach (char c in letters)
        {
            int index = GetIndex(c);
            if (index >= 0)
                counts[index]++;
        }

        return counts;
    }

    public static bool CanBuild(int[] wordCounts, int[] availableCounts)
    {
        for (int i = 0; i < wordCounts.Length; i++)
        {
            if (wordCounts[i] > availableCounts[i])
                return false;
        }

        return true;
    }
}