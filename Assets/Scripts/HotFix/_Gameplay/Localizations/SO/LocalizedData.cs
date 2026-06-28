using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MarbleHero
{
    public class LocalizedData<T> : SerializedScriptableObject where T : class, new()
    {
        [Button]
        public void Sort()
        {
            Items.Sort((k1, k2) => string.Compare(k1.key, k2.key, StringComparison.Ordinal));
        }

        [Button]
        void SaveToJson()
        {
            if (Items == null)
                return;

            Sort();
            var dict = new Dictionary<string, T>();
            foreach (var item in Items)
            {
                T t = item.data;
                dict[item.key] = t;
                if (t is ILocalizedStringsSerialized l)
                {
                    l.beforeSerialized();
                }
            }

            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.NullValueHandling = NullValueHandling.Ignore;
            var json = JsonConvert.SerializeObject(dict, settings);
            File.WriteAllText($"{OutputPath}/{typeof(T).Name}.json", json);
            Debug.Log(json);
        }

        [Button]
        void SyncAllKeys()
        {
            var allDataList = ScriptableObjectUtility.FindAllAssets<LocalizedData<T>>();
            var unionKeys = new HashSet<string>();
            foreach (var data in allDataList)
            {
                unionKeys.addRange(data.Items.Select(item => item.key).ToList());
            }
            
            foreach (var data in allDataList)
            {
                var keys = data.Items.Select(item => item.key).ToList();
                var except = unionKeys.Except(keys);
                foreach (var key in except)
                {
                    data.Items.add(new()
                    {
                        key = key,
                        data = new T()
                    });
                }
                
                data.Sort();
            }
            
            Debug.Log("sync all keys");
        }

        [Button]
        void ApplyThisKeysToOther()
        {
            var allDataList = ScriptableObjectUtility.FindAllAssets<LocalizedData<T>>();
            foreach (var data in allDataList)
            {
                if (data == this)
                    continue;
                
                var thisKeys = Items.Select(item => item.key).ToList();
                var otherKeys = data.Items.Select(item => item.key).ToList();
                var otherDontHas = thisKeys.Except(otherKeys);
                foreach (var key in otherDontHas)
                {
                    data.Items.add(new()
                    {
                        key = key,
                        data = new T()
                    });
                }
                
                var thisDontHas = otherKeys.Except(thisKeys);
                foreach (var key in thisDontHas)
                {
                    data.Items.remove(item => item.key == key);
                }
                
                data.Sort();
            }
            
            Debug.Log("apply this keys to other");
        }
        
        [Button]
        void LoadFromJson()
        {
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.NullValueHandling = NullValueHandling.Ignore;
            var json = File.ReadAllText($"{OutputPath}/{typeof(T).Name}.json");
            var dict = JsonConvert.DeserializeObject<Dictionary<string, T>>(json, settings);
            Items.Clear();
            foreach (var (key, data) in dict)
            {
                Items.add(new()
                {
                    key = key,
                    data = data,
                });

                if (data is ILocalizedStringsSerialized l)
                {
                    l.afterDeserialized();
                }
            }
            Sort();
            Debug.Log(json);
        }

        [FolderPath]
        public string OutputPath;

        [Searchable]
        public List<Item> Items = new();

        [Serializable]
        public class Item
        {
            [FoldoutGroup("$key", false)]
            public string key;

            [FoldoutGroup("$key", false)]
            [HideLabel]
            public T data;
        }
    }
}