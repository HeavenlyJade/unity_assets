using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using MiGame.Achievement;
using Newtonsoft.Json;

namespace MiGame.Editor.Tools
{
    /// <summary>
    /// JSON数据转换用的DTO类
    /// </summary>
    [System.Serializable]
    public class LevelItemDTO
    {
        public int 等级;
        public string 条件类型;
        public string 条件公式;
        public string 效果公式;
        public double 效果数值;
    }

    /// <summary>
    /// 将 配置exel/效果等级配置_等级效果列表.json 中的配置同步到 GameConf/效果等级配置 下的 EffectLevelConfig .asset 文件
    /// </summary>
    public static class 效果等级配置JSON同步工具
    {
        private const string JsonConfigPath = "Assets/配置exel/效果等级配置_等级效果列表.json";
        private const string AssetFolder = "Assets/GameConf/效果等级配置";

        [MenuItem("Tools/配置同步/效果等级配置JSON同步")]
        public static void 同步配置()
        {
            if (!File.Exists(JsonConfigPath))
            {
                Debug.LogError($"JSON配置文件不存在: {JsonConfigPath}");
                return;
            }

            try
            {
                // 读取JSON配置
                var jsonContent = File.ReadAllText(JsonConfigPath);
                var jsonData = JsonConvert.DeserializeObject<Dictionary<string, List<LevelItemDTO>>>(jsonContent);

                if (jsonData == null || jsonData.Count == 0)
                {
                    Debug.LogWarning("JSON配置文件为空或格式错误");
                    return;
                }

                // 显示选择窗口
                var window = EditorWindow.GetWindow<效果等级配置选择窗口>("效果等级配置同步");
                window.Initialize(jsonData);
                window.Show();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"读取JSON配置时发生错误: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 执行同步操作
        /// </summary>
        public static void ExecuteSync(Dictionary<string, List<LevelItemDTO>> jsonData, List<string> selectedConfigs)
        {
            try
            {
                Debug.Log($"开始同步 {selectedConfigs.Count} 个效果等级配置...");

                // 加载所有现有的EffectLevelConfig资源
                var existingConfigs = LoadAllEffectLevelConfigs();
                var configMap = existingConfigs.ToDictionary(cfg => cfg.name, cfg => cfg);
                Debug.Log($"已加载 {existingConfigs.Count} 个现有配置");

                int updatedCount = 0;
                int createdCount = 0;
                int errorCount = 0;

                foreach (var configName in selectedConfigs)
                {
                    try
                    {
                        if (jsonData.TryGetValue(configName, out var levelEffects))
                        {
                            if (levelEffects == null || levelEffects.Count == 0)
                            {
                                Debug.LogWarning($"配置 {configName} 的等级效果数据为空，跳过同步");
                                continue;
                            }

                            if (configMap.TryGetValue(configName, out var existingConfig))
                            {
                                // 更新现有配置
                                Debug.Log($"正在更新现有配置: {configName}");
                                UpdateEffectLevelConfig(existingConfig, levelEffects);
                                updatedCount++;
                                Debug.Log($"✓ 已更新配置: {configName}");
                            }
                            else
                            {
                                // 创建新配置
                                Debug.Log($"正在创建新配置: {configName}");
                                var newConfig = CreateNewEffectLevelConfig(configName, levelEffects);
                                if (newConfig != null)
                                {
                                    createdCount++;
                                    Debug.Log($"✓ 已创建新配置: {configName}");
                                }
                                else
                                {
                                    errorCount++;
                                    Debug.LogError($"✗ 创建配置失败: {configName}");
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"JSON数据中未找到配置: {configName}");
                            errorCount++;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        errorCount++;
                        Debug.LogError($"处理配置 {configName} 时发生错误: {ex.Message}");
                    }
                }

                // 强制保存所有资源
                Debug.Log("正在保存资源...");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var resultMessage = $"同步完成! 更新: {updatedCount} 个, 创建: {createdCount} 个";
                if (errorCount > 0)
                {
                    resultMessage += $", 错误: {errorCount} 个";
                    Debug.LogWarning(resultMessage);
                }
                else
                {
                    Debug.Log(resultMessage);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"同步过程中发生严重错误: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 更新现有的效果等级配置
        /// </summary>
        private static void UpdateEffectLevelConfig(EffectLevelConfig config, List<LevelItemDTO> levelEffects)
        {
            if (config == null || levelEffects == null) return;

            // 记录原有数据数量
            var originalCount = config.等级效果列表?.Count ?? 0;
            Debug.Log($"配置 {config.name}: 原有等级效果数量: {originalCount}");

            // 确保列表已初始化
            if (config.等级效果列表 == null)
            {
                config.等级效果列表 = new List<LevelEffectData>();
            }

            // 完全清空现有列表
            config.等级效果列表.Clear();
            Debug.Log($"配置 {config.name}: 已清空原有等级效果列表");

            // 添加新的等级效果数据
            foreach (var item in levelEffects)
            {
                var levelEffect = new LevelEffectData
                {
                    等级 = item.等级,
                    条件类型 = ParseConditionType(item.条件类型),
                    条件公式 = item.条件公式 ?? "",
                    效果公式 = item.效果公式 ?? "",
                    效果数值 = item.效果数值
                };
                config.等级效果列表.Add(levelEffect);
            }

            Debug.Log($"配置 {config.name}: 新增等级效果数量: {config.等级效果列表.Count}");

            // 标记为已修改
            EditorUtility.SetDirty(config);
        }

        /// <summary>
        /// 创建新的效果等级配置
        /// </summary>
        private static EffectLevelConfig CreateNewEffectLevelConfig(string configName, List<LevelItemDTO> levelEffects)
        {
            try
            {
                var config = ScriptableObject.CreateInstance<EffectLevelConfig>();
                config.name = configName;
                config.配置名称 = configName;
                config.配置描述 = $"从JSON同步的{configName}配置";
                config.目标类型 = TargetType.玩家变量;
                config.作用目标变量 = "数据_固定值_移动速度"; // 根据战力置换速度配置推断

                // 确保列表已初始化并清空
                config.等级效果列表 = new List<LevelEffectData>();
                config.特殊宠物效果列表 = new List<SpecialPetEffect>();

                Debug.Log($"创建新配置 {configName}: 准备添加 {levelEffects.Count} 个等级效果");

                // 添加等级效果数据
                foreach (var item in levelEffects)
                {
                    var levelEffect = new LevelEffectData
                    {
                        等级 = item.等级,
                        条件类型 = ParseConditionType(item.条件类型),
                        条件公式 = item.条件公式 ?? "",
                        效果公式 = item.效果公式 ?? "",
                        效果数值 = item.效果数值
                    };
                    config.等级效果列表.Add(levelEffect);
                }

                Debug.Log($"新配置 {configName}: 已添加 {config.等级效果列表.Count} 个等级效果");

                // 保存到文件
                var assetPath = Path.Combine(AssetFolder, $"{configName}.asset");
                AssetDatabase.CreateAsset(config, assetPath);
                
                return config;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"创建配置 {configName} 时发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 解析条件类型字符串为枚举值
        /// </summary>
        private static ConditionType ParseConditionType(string typeString)
        {
            if (string.IsNullOrEmpty(typeString)) return ConditionType.固定;
            
            return typeString switch
            {
                "公式" => ConditionType.公式,
                "固定" => ConditionType.固定,
                _ => ConditionType.固定
            };
        }

        /// <summary>
        /// 加载所有现有的EffectLevelConfig资源
        /// </summary>
        private static List<EffectLevelConfig> LoadAllEffectLevelConfigs()
        {
            var guids = AssetDatabase.FindAssets("t:EffectLevelConfig", new[] { AssetFolder });
            return guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<EffectLevelConfig>(path))
                .Where(x => x != null)
                .ToList();
        }


    }

    /// <summary>
    /// 效果等级配置选择窗口
    /// </summary>
    public class 效果等级配置选择窗口 : EditorWindow
    {
        private Dictionary<string, List<LevelItemDTO>> _jsonData;
        private List<string> _availableConfigs;
        private List<string> _selectedConfigs = new List<string>();
        private Vector2 _scrollPosition;
        private bool _selectAll = false;

        public void Initialize(Dictionary<string, List<LevelItemDTO>> jsonData)
        {
            _jsonData = jsonData;
            _availableConfigs = jsonData.Keys.ToList();
            _selectedConfigs.Clear();
            _selectAll = false;
        }

        private void OnGUI()
        {
            if (_jsonData == null || _availableConfigs == null)
            {
                EditorGUILayout.HelpBox("未加载配置数据", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("选择要同步的效果等级配置", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 全选/取消全选
            var newSelectAll = EditorGUILayout.Toggle("全选", _selectAll);
            if (newSelectAll != _selectAll)
            {
                _selectAll = newSelectAll;
                if (_selectAll)
                {
                    _selectedConfigs.Clear();
                    _selectedConfigs.AddRange(_availableConfigs);
                }
                else
                {
                    _selectedConfigs.Clear();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"可用配置 ({_availableConfigs.Count} 个):", EditorStyles.boldLabel);

            // 配置列表
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            foreach (var configName in _availableConfigs)
            {
                var isSelected = _selectedConfigs.Contains(configName);
                var newSelected = EditorGUILayout.ToggleLeft(configName, isSelected);
                
                if (newSelected != isSelected)
                {
                    if (newSelected)
                    {
                        _selectedConfigs.Add(configName);
                    }
                    else
                    {
                        _selectedConfigs.Remove(configName);
                    }
                    
                    // 更新全选状态
                    _selectAll = _selectedConfigs.Count == _availableConfigs.Count;
                }
            }
            
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"已选择: {_selectedConfigs.Count} 个配置", EditorStyles.boldLabel);

            // 同步按钮
            EditorGUI.BeginDisabledGroup(_selectedConfigs.Count == 0);
            if (GUILayout.Button($"同步选中的 {_selectedConfigs.Count} 个配置", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("确认同步", 
                    $"确定要同步选中的 {_selectedConfigs.Count} 个配置吗？\n这将覆盖现有的配置数据。", 
                    "确定", "取消"))
                {
                    效果等级配置JSON同步工具.ExecuteSync(_jsonData, _selectedConfigs);
                    Close();
                }
            }
            EditorGUI.EndDisabledGroup();

            // 取消按钮
            if (GUILayout.Button("取消", GUILayout.Height(25)))
            {
                Close();
            }
        }
    }
}
