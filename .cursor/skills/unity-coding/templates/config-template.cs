using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.ModuleName
{
    /// <summary>
    /// [配置名] 策划可编辑配置数据。
    /// </summary>
    [CreateAssetMenu(fileName = "XxxConfig", menuName = "MyGame/ModuleName/Xxx Config")]
    public class XxxConfig : ScriptableObject
    {
        [Header("基础")]
        [SerializeField] private string displayName;
        [SerializeField, Min(0)] private int baseValue;

        [Header("条目")]
        [SerializeField] private List<XxxEntry> entries = new List<XxxEntry>();

        public string DisplayName => displayName;
        public int BaseValue => baseValue;
        public IReadOnlyList<XxxEntry> Entries => entries;

        /// <summary>
        /// 按 id 查找条目，找不到返回 null。
        /// </summary>
        public XxxEntry Find(string id)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Id == id)
                {
                    return entries[i];
                }
            }

            return null;
        }

        private void OnValidate()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                for (int j = i + 1; j < entries.Count; j++)
                {
                    if (entries[i].Id == entries[j].Id)
                    {
                        Debug.LogWarning($"[XxxConfig] 重复 id: {entries[i].Id}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 单条配置项。
    /// </summary>
    [Serializable]
    public class XxxEntry
    {
        [SerializeField] private string id;
        [SerializeField] private float value;
        [SerializeField] private Sprite icon;

        public string Id => id;
        public float Value => value;
        public Sprite Icon => icon;
    }
}
