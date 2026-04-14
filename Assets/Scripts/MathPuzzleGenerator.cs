using System.Collections.Generic;
using UnityEngine;

public class MathPuzzleGenerator : MonoBehaviour
{
    private readonly int[] bigNumbers = { 25, 50, 75, 100 };

    public List<int> GenerateNumbers()
    {
        List<int> numbers = new List<int>();

        for (int i = 0; i < 4; i++)
        {
            numbers.Add(Random.Range(1, 11));
        }

        for (int i = 0; i < 2; i++)
        {
            numbers.Add(bigNumbers[Random.Range(0, bigNumbers.Length)]);
        }

        Shuffle(numbers);
        return numbers;
    }

    public int GenerateTarget()
    {
        return Random.Range(100, 1000);
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }
}