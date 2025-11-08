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

        using var _ = new SafeListReader<int>(safeList, out var readList);
        for (var i = 0; i < readList.Count; i++)
        {
            var value = readList[i];
            Debug.Log(value);
        }
        
        for (var i = 0; i < readList.Count; i++)
        {
            if (i % 2 == 0)
            {
                safeList.removeAt(i);
            }
        }
    }

    void Start()
    {
        using var _ = new SafeListReader<int>(safeList, out var readList);
        for (var i = 0; i < readList.Count; i++)
        {
            var value = readList[i];
            Debug.Log(value);
        }
    }
}