using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ScriptableObjectUtility
{
    public static List<T> FindAllAssets<T>() where T : ScriptableObject
    {
        List<T> result = new();

        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset != null)
                result.Add(asset);
        }

        return result;
    }
}