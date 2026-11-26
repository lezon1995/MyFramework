using System;
using UnityEngine;

public class Test : MonoBehaviour
{
    SafeList<int> safeList = new();

    void Awake()
    {
        for (int i = 0; i < 10; i++)
        {
            safeList.add(i);
        }

        foreach (int value in safeList)
        {
            Debug.Log(value);
        }

        int index = 0;
        int removedCount = 0;
        foreach (int value in safeList)
        {
            if (index % 2 == 0)
            {
                safeList.removeAt(index - removedCount);
                ++removedCount;
            }
            ++index;
        }
    }

    void Start()
    {
        foreach (int value in safeList)
        {
            Debug.Log(value);
        }
    }
}
