using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class GameEntry : GameEntryBase
{
    public override void Awake()
    {
        base.Awake();
        // Game framework = new();
        // framework.init();
        // setFrameworkAOT(framework);

        int[] array = new int[10] { 9, 1, 7, 3, 4, 5, 6, 2, 8, 0 };
        QuickSort(array, 0, array.Length - 1);
        print(string.Join(",", array.Select(i => i.ToString())));
    }

    public void QuickSort(int[] array, int left, int right)
    {
        if (array == null || left >= right)
            return;

        int l = left;
        int r = right;
        int pivot = array[l + (r - l) / 2];

        while (l < r)
        {
            while (array[l] < pivot)
                l++;

            while (array[r] > pivot)
                r--;

            if (l <= r)
            {
                (array[l], array[r]) = (array[r], array[l]);
                l++;
                r--;
            }
        }

        if (r > left)
            QuickSort(array, left, r);

        if (l < right)
            QuickSort(array, l, right);
    }
}