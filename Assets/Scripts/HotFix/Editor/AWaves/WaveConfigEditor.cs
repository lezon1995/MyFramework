using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 波次配置编辑器窗口 - 用于可视化编辑关卡波次配置
    /// </summary>
    public class WaveConfigEditor : EditorWindow
    {
        private WaveLevelConfig _currentConfig;
        private Vector2 _scrollPosition;
        private int _selectedWaveIndex = -1;

        [MenuItem("MoreMountains/Wave Config Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<WaveConfigEditor>("Wave Config Editor");
            window.minSize = new Vector2(600, 400);
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Wave Level Configuration", EditorStyles.boldLabel);

            // 加载或创建配置
            EditorGUILayout.BeginHorizontal();
            _currentConfig = (WaveLevelConfig)EditorGUILayout.ObjectField("Level Config", _currentConfig, typeof(WaveLevelConfig), false);

            if (GUILayout.Button("Create New", GUILayout.Width(80)))
            {
                CreateNewConfig();
            }
            EditorGUILayout.EndHorizontal();

            if (_currentConfig == null)
            {
                EditorGUILayout.HelpBox("Please select or create a Wave Level Configuration.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            EditorGUILayout.Space(10);

            // 基本信息
            EditorGUILayout.LabelField("Basic Info", EditorStyles.boldLabel);
            _currentConfig.levelName = EditorGUILayout.TextField("Level Name", _currentConfig.levelName);
            _currentConfig.levelDescription = EditorGUILayout.TextField("Description", _currentConfig.levelDescription);

            EditorGUILayout.Space(10);

            // 全局设置
            EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);
            _currentConfig.globalMaxActiveMonsters = EditorGUILayout.IntField("Max Active Monsters", _currentConfig.globalMaxActiveMonsters);
            _currentConfig.globalMinActiveMonsters = EditorGUILayout.IntField("Min Active Monsters", _currentConfig.globalMinActiveMonsters);
            _currentConfig.globalBaseSpawnInterval = EditorGUILayout.FloatField("Base Spawn Interval", _currentConfig.globalBaseSpawnInterval);

            EditorGUILayout.Space(5);

            // 属性增长倍率
            EditorGUILayout.LabelField("Global Scaling (Per Wave)", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Health", GUILayout.Width(80));
            _currentConfig.globalHealthScalingPerWave = EditorGUILayout.Slider(_currentConfig.globalHealthScalingPerWave, 1f, 2f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Damage", GUILayout.Width(80));
            _currentConfig.globalDamageScalingPerWave = EditorGUILayout.Slider(_currentConfig.globalDamageScalingPerWave, 1f, 2f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Speed", GUILayout.Width(80));
            _currentConfig.globalSpeedScalingPerWave = EditorGUILayout.Slider(_currentConfig.globalSpeedScalingPerWave, 1f, 1.5f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Defense", GUILayout.Width(80));
            _currentConfig.globalDefenseScalingPerWave = EditorGUILayout.Slider(_currentConfig.globalDefenseScalingPerWave, 1f, 2f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 生成区域
            EditorGUILayout.LabelField("Spawn Area", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _currentConfig.spawnAreaLeft = EditorGUILayout.FloatField("Left", _currentConfig.spawnAreaLeft);
            _currentConfig.spawnAreaRight = EditorGUILayout.FloatField("Right", _currentConfig.spawnAreaRight);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            _currentConfig.spawnAreaBottom = EditorGUILayout.FloatField("Bottom", _currentConfig.spawnAreaBottom);
            _currentConfig.spawnAreaTop = EditorGUILayout.FloatField("Top", _currentConfig.spawnAreaTop);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 波次列表
            EditorGUILayout.LabelField($"Waves ({_currentConfig.waves.Count})", EditorStyles.boldLabel);

            if (_currentConfig.waves.Count > 0)
            {
                // 显示波次列表
                for (int i = 0; i < _currentConfig.waves.Count; i++)
                {
                    DrawWaveItem(i);
                }
            }

            EditorGUILayout.Space(5);

            // 添加新波次按钮
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+ Add Wave", GUILayout.Width(120)))
            {
                AddNewWave();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 保存按钮
            if (GUI.changed)
            {
                EditorUtility.SetDirty(_currentConfig);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawWaveItem(int index)
        {
            var wave = _currentConfig.waves[index];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 波次标题栏
            EditorGUILayout.BeginHorizontal();
            string title = $"Wave {index + 1}";
            if (!string.IsNullOrEmpty(wave.waveName))
            {
                title += $": {wave.waveName}";
            }

            bool foldout = EditorGUILayout.Foldout(_selectedWaveIndex == index, title, true);
            if (foldout)
            {
                _selectedWaveIndex = index;
            }
            else if (_selectedWaveIndex == index)
            {
                _selectedWaveIndex = -1;
            }

            // 删除按钮
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                if (EditorUtility.DisplayDialog("Delete Wave", $"Delete Wave {index + 1}?", "Yes", "Cancel"))
                {
                    _currentConfig.waves.RemoveAt(index);
                    if (_selectedWaveIndex == index)
                        _selectedWaveIndex = -1;
                    GUIUtility.ExitGUI();
                }
            }
            EditorGUILayout.EndHorizontal();

            // 展开内容
            if (_selectedWaveIndex == index)
            {
                EditorGUI.indentLevel++;
                DrawWaveDetails(wave);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawWaveDetails(WaveConfig wave)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Wave Number", GUILayout.Width(100));
            wave.waveNumber = EditorGUILayout.IntField(wave.waveNumber);
            EditorGUILayout.EndHorizontal();

            wave.waveName = EditorGUILayout.TextField("Wave Name", wave.waveName);
            wave.duration = EditorGUILayout.FloatField("Duration (s)", wave.duration);
            wave.clearStrategy = (WaveClearStrategy)EditorGUILayout.EnumPopup("Clear Strategy", wave.clearStrategy);

            EditorGUILayout.Space(5);

            // 怪物数量控制
            EditorGUILayout.LabelField("Monster Limits", EditorStyles.miniLabel);
            wave.maxActiveMonsters = EditorGUILayout.IntField("Max Active", wave.maxActiveMonsters);
            wave.minActiveMonsters = EditorGUILayout.IntField("Min Active", wave.minActiveMonsters);

            // 击败所有怪物策略的最大生成数量
            if (wave.clearStrategy == WaveClearStrategy.DefeatAllMonsters)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("DefeatAll Max Spawn", GUILayout.Width(140));
                wave.defeatAllMaxTotalSpawn = EditorGUILayout.IntField(wave.defeatAllMaxTotalSpawn, GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField($"Default: {wave.maxActiveMonsters * 3} (MaxActive * 3)", EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(5);

            // 怪物类型权重
            EditorGUILayout.LabelField("Monster Type Weights", EditorStyles.miniLabel);
            wave.normalMonsterWeight = EditorGUILayout.Slider("Normal", wave.normalMonsterWeight, 0, 100);
            wave.eliteMonsterWeight = EditorGUILayout.Slider("Elite", wave.eliteMonsterWeight, 0, 100);
            wave.bossMonsterWeight = EditorGUILayout.Slider("Boss", wave.bossMonsterWeight, 0, 100);

            EditorGUILayout.Space(5);

            // 属性增长
            EditorGUILayout.LabelField("Wave Scaling", EditorStyles.miniLabel);
            wave.healthScaling = EditorGUILayout.Slider("Health", wave.healthScaling, 1f, 2f);
            wave.damageScaling = EditorGUILayout.Slider("Damage", wave.damageScaling, 1f, 2f);
            wave.speedScaling = EditorGUILayout.Slider("Speed", wave.speedScaling, 1f, 1.5f);
            wave.defenseScaling = EditorGUILayout.Slider("Defense", wave.defenseScaling, 1f, 2f);

            EditorGUILayout.Space(5);

            // Boss设置
            if (wave.clearStrategy == WaveClearStrategy.DefeatBoss)
            {
                EditorGUILayout.LabelField("Boss Settings", EditorStyles.miniLabel);
                wave.bossMonsterId = EditorGUILayout.TextField("Boss Monster ID", wave.bossMonsterId);
                wave.bossSpawnTime = EditorGUILayout.FloatField("Boss Spawn Time", wave.bossSpawnTime);
            }

            EditorGUILayout.Space(5);

            // 生成位置设置
            EditorGUILayout.LabelField("Spawn Position", EditorStyles.miniLabel);
            wave.edgeBiasProbability = EditorGUILayout.Slider("Edge Bias Probability", wave.edgeBiasProbability, 0f, 1f);
            wave.edgeBiasPercent = EditorGUILayout.Slider("Edge Bias Percent", wave.edgeBiasPercent, 0f, 1f);
            wave.edgeBiasPercentAmplitude = EditorGUILayout.Slider("Edge Bias Percent Amplitude", wave.edgeBiasPercentAmplitude, 0f, 1f);
            EditorGUILayout.LabelField($"0 = Random, 1 = Always Edge", EditorStyles.miniLabel);

            EditorGUILayout.Space(5);

            // 智能刷怪设置
            EditorGUILayout.LabelField("Smart Spawning", EditorStyles.miniLabel);
            wave.enableSmartSpawning = EditorGUILayout.Toggle("Enable Smart Spawning", wave.enableSmartSpawning);
            if (wave.enableSmartSpawning)
            {
                wave.denseRadius = EditorGUILayout.FloatField("Dense Radius", wave.denseRadius);
                wave.sparseThreshold = EditorGUILayout.IntField("Sparse Threshold", wave.sparseThreshold);
            }

            EditorGUILayout.Space(5);

            // 可用怪物列表
            EditorGUILayout.LabelField($"Available Monsters ({wave.availableMonsters.Count})", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID", GUILayout.Width(80));
            EditorGUILayout.LabelField("Type", GUILayout.Width(80));
            EditorGUILayout.LabelField("Weight", GUILayout.Width(60));
            EditorGUILayout.LabelField("Force", GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            if (wave.availableMonsters.Count > 0)
            {
                for (int i = wave.availableMonsters.Count - 1; i >= 0; i--)
                {
                    var monster = wave.availableMonsters[i];

                    EditorGUILayout.BeginHorizontal();

                    EditorGUI.BeginChangeCheck();
                    monster.monsterId = EditorGUILayout.TextField(monster.monsterId, GUILayout.Width(80));
                    monster.enemyType = (SpawnEnemyType)EditorGUILayout.EnumPopup(monster.enemyType, GUILayout.Width(80));
                    monster.spawnWeight = EditorGUILayout.FloatField(monster.spawnWeight, GUILayout.Width(60));
                    monster.forceSpawnOnce = EditorGUILayout.Toggle(monster.forceSpawnOnce, GUILayout.Width(50));

                    if (GUILayout.Button("X", GUILayout.Width(30)))
                    {
                        wave.availableMonsters.RemoveAt(i);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space(3);

            // 添加按钮
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+ Add Monster", GUILayout.Width(120)))
            {
                wave.availableMonsters.Add(new MonsterSpawnConfig());
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void CreateNewConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Wave Level Config",
                "WaveLevelConfig",
                "asset",
                "Choose a location to save the new Wave Level Configuration"
            );

            if (!string.IsNullOrEmpty(path))
            {
                var newConfig = CreateInstance<WaveLevelConfig>();
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                _currentConfig = newConfig;
                Selection.activeObject = newConfig;
            }
        }

        private void AddNewWave()
        {
            var newWave = new WaveConfig
            {
                waveNumber = _currentConfig.waves.Count + 1,
                waveName = $"Wave {_currentConfig.waves.Count + 1}",
                duration = 60f,
                clearStrategy = WaveClearStrategy.SurviveUntilEnd,
                maxActiveMonsters = _currentConfig.globalMaxActiveMonsters,
                minActiveMonsters = _currentConfig.globalMinActiveMonsters
            };

            _currentConfig.waves.Add(newWave);
            EditorUtility.SetDirty(_currentConfig);
        }
    }
}
