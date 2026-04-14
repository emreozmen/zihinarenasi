using System;
using System.Collections.Generic;

[Serializable]
public class WordEntry
{
    public string word;
    public int length;
    public int[] counts;
}

[Serializable]
public class WordDatabase
{
    public List<WordEntry> entries = new List<WordEntry>();
}