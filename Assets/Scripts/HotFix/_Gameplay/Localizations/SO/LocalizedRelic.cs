using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace MarbleHero
{
    [CreateAssetMenu(fileName = "LocalizedRelic", menuName = "MarbleHero/Localization/Relic")]
    public class LocalizedRelic : LocalizedData<RelicStrings>
    {
        [Button]
        void ReflectToSync()
        {
            var types = TypeCache.GetTypesDerivedFrom<ARelic>();

            var allDataList = ScriptableObjectUtility.FindAllAssets<LocalizedData<RelicStrings>>();
            var unionKeys = new HashSet<string>(types.Select(type => type.Name));
            foreach (var data in allDataList)
            {
                var keys = data.Items.Select(item => item.key).ToList();
                var except = unionKeys.Except(keys);
                foreach (var key in except)
                {
                    data.Items.add(new()
                    {
                        key = key,
                        data = new()
                        {
                            NAME = key,
                            FLAVOR = key,
                        }
                    });
                }
                
                var thisDontHas = keys.Except(unionKeys);
                foreach (var key in thisDontHas)
                {
                    data.Items.remove(item => item.key == key);
                }
                
                data.Sort();
            }
        }
    }
}