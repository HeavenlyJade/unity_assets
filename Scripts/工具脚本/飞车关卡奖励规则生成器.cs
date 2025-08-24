using UnityEngine;
using System.Collections.Generic;
using MiGame.Data;
using MiGame.Items;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MiGame.Tools
{
    /// <summary>
    /// 飞车关卡奖杯奖励规则生成器
    /// 用于自动生成400万到1000万距离的奖杯奖励规则
    /// </summary>
    public class 飞车关卡奖励规则生成器 : MonoBehaviour
    {
        [Header("关卡配置选择")]
        [Tooltip("要配置的关卡配置")]
        public LevelConfig 目标关卡配置;
        
        [Tooltip("关卡配置文件路径（相对于Assets文件夹）")]
        public string 关卡配置路径 = "Assets/GameConf/关卡/飞车关卡初级.asset";
        
        [Header("生成配置")]
        [Tooltip("最短距离（米）")]
        public int 最短距离 = 300000; // 30万米
        
        [Tooltip("最大距离（米）")]
        public int 最大距离 = 5000000; // 500万米
        
        [Tooltip("30万阶段的奖杯数量")]
        public int 三十万奖杯 = 30;
        
        [Tooltip("50万阶段的奖杯数量")]
        public int 五十万奖杯 = 50;
        
        [Tooltip("100万阶段的奖杯数量")]
        public int 一百万奖杯 = 80;
        
        [Tooltip("奖杯物品类型")]
        public ItemType 奖杯物品类型;
        
        [Header("生成结果")]
        [Tooltip("生成的奖励规则列表")]
        public List<RealtimeRewardRule> 生成的奖励规则 = new List<RealtimeRewardRule>();
        
        [Header("调试信息")]
        [Tooltip("是否在控制台输出生成的规则")]
        public bool 输出调试信息 = true;
        
        #if UNITY_EDITOR
        /// <summary>
        /// Unity编辑器菜单项 - 打开飞车关卡奖励规则生成器窗口
        /// </summary>
        [MenuItem("Tools/关卡配置/飞车关卡奖励规则生成器")]
        public static void 打开生成器窗口()
        {
            var 窗口 = EditorWindow.GetWindow<飞车关卡奖励规则生成器窗口>("飞车关卡奖励规则生成器");
            窗口.Show();
        }
        
        /// <summary>
        /// Unity编辑器菜单项 - 快速生成飞车关卡奖励规则
        /// </summary>
        [MenuItem("Tools/关卡配置/快速生成飞车关卡奖励规则")]
        public static void 快速生成飞车关卡奖励规则()
        {
            // 查找飞车关卡配置
            var 飞车关卡配置s = Resources.FindObjectsOfTypeAll<LevelConfig>();
            LevelConfig 目标配置 = null;
            
            foreach (var 配置 in 飞车关卡配置s)
            {
                if (配置.关卡名称.Contains("飞车"))
                {
                    目标配置 = 配置;
                    break;
                }
            }
            
            if (目标配置 == null)
            {
                EditorUtility.DisplayDialog("错误", "未找到飞车关卡配置！", "确定");
                return;
            }
            
            // 生成默认奖励规则
            var 生成器 = new 飞车关卡奖励规则生成器();
            生成器.目标关卡配置 = 目标配置;
            生成器.最短距离 = 300000; // 30万米
            生成器.最大距离 = 5000000; // 500万米
            生成器.三十万奖杯 = 30;
            生成器.五十万奖杯 = 50;
            生成器.一百万奖杯 = 80;
            
            // 自动查找奖杯物品类型
            生成器.自动查找奖杯物品();
            
            生成器.生成奖励规则();
            生成器.应用规则到选中关卡();
            
            EditorUtility.DisplayDialog("成功", $"已为关卡 '{目标配置.关卡名称}' 生成并应用了 {生成器.生成的奖励规则.Count} 个奖励规则！", "确定");
        }
        
        /// <summary>
        /// Unity编辑器菜单项 - 批量生成所有关卡奖励规则
        /// </summary>
        [MenuItem("Tools/关卡配置/批量生成所有关卡奖励规则")]
        public static void 批量生成所有关卡奖励规则()
        {
            var 所有关卡配置 = Resources.FindObjectsOfTypeAll<LevelConfig>();
            if (所有关卡配置.Length == 0)
            {
                EditorUtility.DisplayDialog("错误", "未找到任何关卡配置！", "确定");
                return;
            }
            
            int 成功数量 = 0;
            var 生成器 = new 飞车关卡奖励规则生成器();
            
            foreach (var 关卡配置 in 所有关卡配置)
            {
                try
                {
                    生成器.目标关卡配置 = 关卡配置;
                    生成器.最短距离 = 300000; // 30万米
                    生成器.最大距离 = 5000000; // 500万米
                    生成器.三十万奖杯 = 30;
                    生成器.五十万奖杯 = 50;
                    生成器.一百万奖杯 = 80;
                    
                    // 自动查找奖杯物品类型
                    生成器.自动查找奖杯物品();
                    
                    生成器.生成奖励规则();
                    生成器.应用规则到选中关卡();
                    成功数量++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"为关卡 '{关卡配置.关卡名称}' 生成奖励规则时出错: {e.Message}");
                }
            }
            
            EditorUtility.DisplayDialog("完成", $"成功为 {成功数量}/{所有关卡配置.Length} 个关卡生成了奖励规则！", "确定");
        }
        #endif
        
        /// <summary>
        /// 加载关卡配置
        /// </summary>
        [ContextMenu("加载关卡配置")]
        public void 加载关卡配置()
        {
            if (目标关卡配置 != null)
            {
                Debug.Log($"已选择关卡配置: {目标关卡配置.关卡名称}");
                return;
            }
            
            // 尝试从路径加载
            if (!string.IsNullOrEmpty(关卡配置路径))
            {
                #if UNITY_EDITOR
                var 配置 = AssetDatabase.LoadAssetAtPath<LevelConfig>(关卡配置路径);
                if (配置 != null)
                {
                    目标关卡配置 = 配置;
                    Debug.Log($"从路径加载关卡配置成功: {配置.关卡名称}");
                }
                else
                {
                    Debug.LogWarning($"无法从路径加载关卡配置: {关卡配置路径}");
                }
                #endif
            }
            
            // 如果还是没有，尝试查找所有关卡配置
            if (目标关卡配置 == null)
            {
                var 所有关卡配置 = Resources.FindObjectsOfTypeAll<LevelConfig>();
                if (所有关卡配置.Length > 0)
                {
                    目标关卡配置 = 所有关卡配置[0];
                    Debug.Log($"自动选择第一个关卡配置: {目标关卡配置.关卡名称}");
                }
            }
        }
        
        /// <summary>
        /// 生成奖励规则
        /// </summary>
        [ContextMenu("生成奖励规则")]
        public void 生成奖励规则()
        {
            if (目标关卡配置 == null)
            {
                Debug.LogWarning("请先选择关卡配置！");
                加载关卡配置();
                if (目标关卡配置 == null)
                {
                    return;
                }
            }
            
            // 自动查找奖杯物品类型
            if (奖杯物品类型 == null)
            {
                自动查找奖杯物品();
            }
            
            生成的奖励规则.Clear();
            
            int 规则计数器 = 1;
            
            // 生成30万阶段的规则
            for (int i = 1; i <= 最大距离 / 300000; i++)
            {
                int 距离 = i * 300000;
                if (距离 >= 最短距离 && 距离 <= 最大距离)
                {
                    var 规则 = 创建奖励规则($"distance>={距离}", $"rule_{规则计数器:D4}", 三十万奖杯);
                    生成的奖励规则.Add(规则);
                    规则计数器++;
                    
                    if (输出调试信息)
                    {
                        Debug.Log($"生成30万阶段规则: 距离>={距离}, 奖杯={三十万奖杯}");
                    }
                }
            }
            
            // 生成50万阶段的规则
            for (int i = 1; i <= 最大距离 / 500000; i++)
            {
                int 距离 = i * 500000;
                if (距离 >= 最短距离 && 距离 <= 最大距离)
                {
                    var 规则 = 创建奖励规则($"distance>={距离}", $"rule_{规则计数器:D4}", 五十万奖杯);
                    生成的奖励规则.Add(规则);
                    规则计数器++;
                    
                    if (输出调试信息)
                    {
                        Debug.Log($"生成50万阶段规则: 距离>={距离}, 奖杯={五十万奖杯}");
                    }
                }
            }
            
            // 生成100万阶段的规则
            for (int i = 1; i <= 最大距离 / 1000000; i++)
            {
                int 距离 = i * 1000000;
                if (距离 >= 最短距离 && 距离 <= 最大距离)
                {
                    var 规则 = 创建奖励规则($"distance>={距离}", $"rule_{规则计数器:D4}", 一百万奖杯);
                    生成的奖励规则.Add(规则);
                    规则计数器++;
                    
                    if (输出调试信息)
                    {
                        Debug.Log($"生成100万阶段规则: 距离>={距离}, 奖杯={一百万奖杯}");
                    }
                }
            }
            
            // 按距离排序
            生成的奖励规则.Sort((a, b) => 
            {
                int 距离A = 提取距离(a.触发条件);
                int 距离B = 提取距离(b.触发条件);
                return 距离A.CompareTo(距离B);
            });
            
            Debug.Log($"成功生成 {生成的奖励规则.Count} 个奖励规则");
        }
        
        /// <summary>
        /// 自动查找奖杯物品类型
        /// </summary>
        private void 自动查找奖杯物品()
        {
            var 奖杯物品s = Resources.FindObjectsOfTypeAll<ItemType>();
            foreach (var 物品 in 奖杯物品s)
            {
                if (物品.name.Contains("奖杯") || 物品.name.Contains("trophy") || 物品.name.Contains("Trophy"))
                {
                    奖杯物品类型 = 物品;
                    Debug.Log($"自动找到奖杯物品: {物品.name}");
                    return;
                }
            }
            
            // 如果没找到，尝试查找包含"杯"的物品
            foreach (var 物品 in 奖杯物品s)
            {
                if (物品.name.Contains("杯"))
                {
                    奖杯物品类型 = 物品;
                    Debug.Log($"自动找到奖杯物品: {物品.name}");
                    return;
                }
            }
            
            Debug.LogWarning("未找到奖杯物品类型，请手动设置！");
        }
        
        /// <summary>
        /// 创建奖励规则
        /// </summary>
        private RealtimeRewardRule 创建奖励规则(string 触发条件, string 规则ID, int 奖杯数量)
        {
            return new RealtimeRewardRule
            {
                触发条件 = 触发条件,
                规则ID = 规则ID,
                奖励物品 = 奖杯物品类型,
                奖励公式 = 奖杯数量.ToString()
            };
        }
        
        /// <summary>
        /// 从触发条件中提取距离数值
        /// </summary>
        private int 提取距离(string 触发条件)
        {
            if (string.IsNullOrEmpty(触发条件)) return 0;
            
            // 查找 ">=" 后面的数字
            int 索引 = 触发条件.IndexOf(">=");
            if (索引 >= 0)
            {
                string 数字部分 = 触发条件.Substring(索引 + 2).Trim();
                if (int.TryParse(数字部分, out int 距离))
                {
                    return 距离;
                }
            }
            
            return 0;
        }
        
        /// <summary>
        /// 应用生成的规则到选中的关卡配置
        /// </summary>
        [ContextMenu("应用规则到选中关卡")]
        public void 应用规则到选中关卡()
        {
            if (生成的奖励规则.Count == 0)
            {
                Debug.LogWarning("请先生成奖励规则！");
                return;
            }
            
            if (目标关卡配置 == null)
            {
                Debug.LogWarning("请先选择关卡配置！");
                return;
            }
            
            // 备份原有规则
            var 原有规则 = new List<RealtimeRewardRule>(目标关卡配置.实时奖励规则);
            
            // 应用新规则
            目标关卡配置.实时奖励规则 = new List<RealtimeRewardRule>(生成的奖励规则);
            
            Debug.Log($"已应用 {生成的奖励规则.Count} 个奖励规则到关卡: {目标关卡配置.关卡名称}");
            Debug.Log($"原有规则数量: {原有规则.Count}");
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(目标关卡配置);
            AssetDatabase.SaveAssets();
            Debug.Log("关卡配置已保存");
            #endif
        }
        
        /// <summary>
        /// 应用生成的规则到关卡配置（兼容旧版本）
        /// </summary>
        [ContextMenu("应用规则到关卡配置")]
        public void 应用规则到关卡配置()
        {
            if (生成的奖励规则.Count == 0)
            {
                Debug.LogWarning("请先生成奖励规则！");
                return;
            }
            
            // 查找关卡配置
            var 关卡配置s = Resources.FindObjectsOfTypeAll<LevelConfig>();
            if (关卡配置s.Length == 0)
            {
                Debug.LogWarning("未找到关卡配置！");
                return;
            }
            
            foreach (var 关卡配置 in 关卡配置s)
            {
                if (关卡配置.关卡名称.Contains("飞车"))
                {
                    关卡配置.实时奖励规则 = new List<RealtimeRewardRule>(生成的奖励规则);
                    Debug.Log($"已应用 {生成的奖励规则.Count} 个奖励规则到关卡: {关卡配置.关卡名称}");
                    
                    #if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(关卡配置);
                    #endif
                }
            }
        }
        
        /// <summary>
        /// 预览生成的规则（不应用）
        /// </summary>
        [ContextMenu("预览生成的规则")]
        public void 预览生成的规则()
        {
            if (生成的奖励规则.Count == 0)
            {
                Debug.LogWarning("请先生成奖励规则！");
                return;
            }
            
            Debug.Log($"=== 预览生成的 {生成的奖励规则.Count} 个奖励规则 ===");
            for (int i = 0; i < 生成的奖励规则.Count; i++)
            {
                var 规则 = 生成的奖励规则[i];
                Debug.Log($"{i + 1}. 规则ID: {规则.规则ID}, 触发条件: {规则.触发条件}, 奖杯: {规则.奖励公式}");
            }
        }
        
        /// <summary>
        /// 清空生成的规则
        /// </summary>
        [ContextMenu("清空生成的规则")]
        public void 清空生成的规则()
        {
            生成的奖励规则.Clear();
            Debug.Log("已清空生成的奖励规则");
        }
        
        /// <summary>
        /// 导出规则为JSON格式
        /// </summary>
        [ContextMenu("导出规则为JSON")]
        public void 导出规则为JSON()
        {
            if (生成的奖励规则.Count == 0)
            {
                Debug.LogWarning("请先生成奖励规则！");
                return;
            }
            
            var 导出数据 = new List<object>();
            foreach (var 规则 in 生成的奖励规则)
            {
                导出数据.Add(new
                {
                    触发条件 = 规则.触发条件,
                    规则ID = 规则.规则ID,
                    奖励物品 = 规则.奖励物品?.name ?? "未知",
                    奖励公式 = 规则.奖励公式
                });
            }
            
            string json = JsonUtility.ToJson(new { 奖励规则 = 导出数据 }, true);
            Debug.Log($"奖励规则JSON:\n{json}");
        }
        
        /// <summary>
        /// 重置关卡配置选择
        /// </summary>
        [ContextMenu("重置关卡配置选择")]
        public void 重置关卡配置选择()
        {
            目标关卡配置 = null;
            Debug.Log("已重置关卡配置选择");
        }
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// 飞车关卡奖励规则生成器编辑器窗口
    /// </summary>
    public class 飞车关卡奖励规则生成器窗口 : EditorWindow
    {
        private 飞车关卡奖励规则生成器 生成器;
        private Vector2 滚动位置;
        
        private void OnEnable()
        {
            生成器 = new 飞车关卡奖励规则生成器();
        }
        
        private void OnGUI()
        {
            titleContent = new GUIContent("飞车关卡奖励规则生成器");
            
            EditorGUILayout.BeginVertical();
            
            // 标题
            EditorGUILayout.LabelField("飞车关卡奖杯奖励规则生成器", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            滚动位置 = EditorGUILayout.BeginScrollView(滚动位置);
            
            // 关卡配置选择
            EditorGUILayout.LabelField("关卡配置选择", EditorStyles.boldLabel);
            生成器.目标关卡配置 = (LevelConfig)EditorGUILayout.ObjectField("目标关卡配置", 生成器.目标关卡配置, typeof(LevelConfig), false);
            生成器.关卡配置路径 = EditorGUILayout.TextField("关卡配置路径", 生成器.关卡配置路径);
            
            if (GUILayout.Button("加载关卡配置"))
            {
                生成器.加载关卡配置();
            }
            
            EditorGUILayout.Space();
            
            // 生成配置
            EditorGUILayout.LabelField("生成配置", EditorStyles.boldLabel);
            生成器.最短距离 = EditorGUILayout.IntField("最短距离（米）", 生成器.最短距离);
            生成器.最大距离 = EditorGUILayout.IntField("最大距离（米）", 生成器.最大距离);
            生成器.三十万奖杯 = EditorGUILayout.IntField("30万阶段奖杯", 生成器.三十万奖杯);
            生成器.五十万奖杯 = EditorGUILayout.IntField("50万阶段奖杯", 生成器.五十万奖杯);
            生成器.一百万奖杯 = EditorGUILayout.IntField("100万阶段奖杯", 生成器.一百万奖杯);
            生成器.奖杯物品类型 = (ItemType)EditorGUILayout.ObjectField("奖杯物品类型", 生成器.奖杯物品类型, typeof(ItemType), false);
            
            EditorGUILayout.Space();
            
            // 操作按钮
            EditorGUILayout.LabelField("操作", EditorStyles.boldLabel);
            
            if (GUILayout.Button("生成奖励规则", GUILayout.Height(30)))
            {
                生成器.生成奖励规则();
            }
            
            if (GUILayout.Button("预览生成的规则", GUILayout.Height(25)))
            {
                生成器.预览生成的规则();
            }
            
            if (GUILayout.Button("应用规则到选中关卡", GUILayout.Height(25)))
            {
                生成器.应用规则到选中关卡();
            }
            
            if (GUILayout.Button("导出规则为JSON", GUILayout.Height(25)))
            {
                生成器.导出规则为JSON();
            }
            
            if (GUILayout.Button("清空生成的规则", GUILayout.Height(25)))
            {
                生成器.清空生成的规则();
            }
            
            EditorGUILayout.Space();
            
            // 生成结果
            if (生成器.生成的奖励规则.Count > 0)
            {
                EditorGUILayout.LabelField($"生成结果: {生成器.生成的奖励规则.Count} 个规则", EditorStyles.boldLabel);
                
                for (int i = 0; i < Mathf.Min(生成器.生成的奖励规则.Count, 10); i++)
                {
                    var 规则 = 生成器.生成的奖励规则[i];
                    EditorGUILayout.LabelField($"{i + 1}. {规则.规则ID}: {规则.触发条件} → {规则.奖励公式}奖杯");
                }
                
                if (生成器.生成的奖励规则.Count > 10)
                {
                    EditorGUILayout.LabelField($"... 还有 {生成器.生成的奖励规则.Count - 10} 个规则");
                }
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
    #endif
}
